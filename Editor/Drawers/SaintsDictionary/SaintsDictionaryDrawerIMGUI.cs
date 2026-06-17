using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        private sealed class CellContextIMGUI
        {
            public FieldInfo FieldInfo;
            public Type RawType;
            public WrapType WrapType;
            public bool HasSerializeReference;
            public IReadOnlyList<Attribute> Attributes;
        }

        private sealed class DictionaryContextIMGUI
        {
            public SerializedProperty RootProperty;
            public SerializedProperty KeysProp;
            public SerializedProperty ValuesProp;
            public SaintsDictionaryAttribute Attribute;
            public FieldInfo Info;
            public object Parent;
            public string Label;
            public ResponsiveLength KeyWidth;
            public ResponsiveLength ValueWidth;
            public CellContextIMGUI KeyCellContext;
            public CellContextIMGUI ValueCellContext;
        }

        private sealed class AsyncSearchItemsIMGUI
        {
            public bool Started = true;
            public bool Finished = true;
            public IEnumerator<int> SourceGenerator;
            public string KeySearchText = "";
            public string ValueSearchText = "";
            public double DebounceSearchTime;
            public List<int> HitTargetIndexes = new List<int>();
            public List<int> CachedHitTargetIndexes = new List<int>();
            public readonly List<int> VisibleIndexes = new List<int>();
            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;
        }

        private sealed class InfoIMGUI
        {
            public readonly AsyncSearchItemsIMGUI AsyncSearchItems = new AsyncSearchItemsIMGUI();
            public readonly IMGUILoading KeyLoading = new IMGUILoading();
            public readonly IMGUILoading ValueLoading = new IMGUILoading();
            public SaintsDictionaryTable Table;
            public DictionaryContextIMGUI Context;
            public string KeysPropPath;
            public string ValuesPropPath;
        }

        private class SaintsDictionaryTable :
            TreeView
#if UNITY_6000_2_OR_NEWER
            <int>
#endif
        {
            private readonly SerializedProperty _property;
            private readonly SerializedProperty _keysProp;
            private readonly SerializedProperty _valuesProp;
            private readonly CellContextIMGUI _keyCellContext;
            private readonly CellContextIMGUI _valueCellContext;
            private readonly FieldInfo _info;
            private readonly object _parent;
            private readonly ResponsiveLength _keyWidth;
            private readonly ResponsiveLength _valueWidth;

            private List<int> _itemIndexToPropertyIndex = new List<int>();
            private bool _treeLoaded;
            private float _lastContentWidth = -1f;
            public class SwapEvent: UnityEvent<IReadOnlyList<(int fromIndex, int toIndex)>> {}

            public readonly SwapEvent IndexSwapEvent = new SwapEvent();

            public SaintsDictionaryTable(
                TreeViewState
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    state,
                MultiColumnHeader multiColumnHeader,
                SerializedProperty property,
                SerializedProperty keysProp,
                SerializedProperty valuesProp,
                CellContextIMGUI keyCellContext,
                CellContextIMGUI valueCellContext,
                FieldInfo info,
                object parent,
                ResponsiveLength keyWidth,
                ResponsiveLength valueWidth) : base(state, multiColumnHeader)
            {
                rowHeight = SaintsDictionaryDrawer.SingleLineHeight;
                showAlternatingRowBackgrounds = true;
                showBorder = true;

                _property = property;
                _keysProp = keysProp;
                _valuesProp = valuesProp;
                _keyCellContext = keyCellContext;
                _valueCellContext = valueCellContext;
                _info = info;
                _parent = parent;
                _keyWidth = keyWidth;
                _valueWidth = valueWidth;
            }

            public void SetColumnWidths(float totalWidth)
            {
                float contentWidth = Mathf.Max(100f, totalWidth - 25f);
                if (Mathf.Abs(_lastContentWidth - contentWidth) < 0.5f)
                {
                    return;
                }

                _lastContentWidth = contentWidth;
                (float keyWidth, float valueWidth) = GetColumnWidths(contentWidth, _keyWidth, _valueWidth);

                MultiColumnHeaderState.Column[] columns = multiColumnHeader.state.columns;
                if (columns.Length >= 2)
                {
                    columns[0].width = keyWidth;
                    columns[0].minWidth = 40f;
                    columns[1].width = valueWidth;
                    columns[1].minWidth = 40f;
                }
            }

            public void SetItemIndexToPropertyIndex(IReadOnlyList<int> itemIndexToPropertyIndex)
            {
                if (_treeLoaded && _itemIndexToPropertyIndex.SequenceEqual(itemIndexToPropertyIndex))
                {
                    return;
                }

                _itemIndexToPropertyIndex = itemIndexToPropertyIndex.ToList();
                Reload();
                _treeLoaded = true;
            }

            protected override
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                BuildRoot()
            {
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    root = new
                        TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                    {
                        id = -1,
                        depth = -1,
                        displayName = "Root",
                    };

                List<
                    TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                > allItems = _itemIndexToPropertyIndex
                    .Select(index => new
                        TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                    {
                        id = index,
                        depth = 0,
                        displayName = $"{index}",
                    })
                    .ToList();
                SetupParentsAndChildrenFromDepths(root, allItems);
                return root;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    item = args.item;

                for (int visibleColumn = 0; visibleColumn < args.GetNumVisibleColumns(); visibleColumn++)
                {
                    CellGUI(args.GetCellRect(visibleColumn), item, args.GetColumn(visibleColumn));
                }
            }

            protected override float GetCustomRowHeight(
                int row,
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    item)
            {
                int index = item.id;
                float keyHeight = GetCellHeight(_keysProp, index, _keyCellContext);
                float valueHeight = GetCellHeight(_valuesProp, index, _valueCellContext);
                return Mathf.Max(keyHeight, valueHeight);
            }

            private float GetCellHeight(SerializedProperty listProp, int index, CellContextIMGUI context)
            {
                if (index < 0 || index >= listProp.arraySize)
                {
                    return SaintsDictionaryDrawer.SingleLineHeight;
                }

                SerializedProperty elementProp = listProp.GetArrayElementAtIndex(index);
                elementProp.isExpanded = true;

                SaintsWrapUtils.CellInfoIMGUI cellInfo = SaintsWrapUtils.GetCellInfoIMGUI(context.WrapType,
                    context.FieldInfo, context.RawType, elementProp, context.Attributes,
                    context.HasSerializeReference, false, "");

                if (!cellInfo.IsValid)
                {
                    return EditorGUI.GetPropertyHeight(elementProp, GUIContent.none, true);
                }

                cellInfo.Property.isExpanded = true;
                return IMGUIRawDraw.GetPropertyHeight(cellInfo.Drawer, GUIContent.none, cellInfo.Property,
                    cellInfo.Attributes, cellInfo.RawType, cellInfo.Info, false);
            }

            private void CellGUI(
                Rect cellRect,
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    item,
                int columnIndex)
            {
                int index = item.id;
                bool isKeyColumn = columnIndex == 0;
                SerializedProperty listProp = isKeyColumn ? _keysProp : _valuesProp;
                CellContextIMGUI cellContext = isKeyColumn ? _keyCellContext : _valueCellContext;

                if (index < 0 || index >= listProp.arraySize)
                {
                    return;
                }

                if (isKeyColumn && IsKeyConflicted(index))
                {
                    EditorGUI.DrawRect(cellRect, new Color(WarningColor.r, WarningColor.g, WarningColor.b, 0.22f));
                }

                SerializedProperty elementProp = listProp.GetArrayElementAtIndex(index);
                elementProp.isExpanded = true;

                SaintsWrapUtils.CellInfoIMGUI cellInfo = SaintsWrapUtils.GetCellInfoIMGUI(cellContext.WrapType,
                    cellContext.FieldInfo, cellContext.RawType, elementProp, cellContext.Attributes,
                    cellContext.HasSerializeReference, false, "");

                if (!cellInfo.IsValid)
                {
                    EditorGUI.PropertyField(cellRect, elementProp, GUIContent.none, true);
                    return;
                }

                cellInfo.Property.isExpanded = true;
                Rect useRect = cellInfo.ShouldIndent
                    ? new Rect(cellRect)
                    {
                        x = cellRect.x + 12f,
                        width = Mathf.Max(0f, cellRect.width - 12f),
                    }
                    : cellRect;
                IMGUIRawDraw.OnGUI(cellInfo.Drawer, useRect, cellInfo.Property, cellInfo.Attributes, cellInfo.RawType,
                    GUIContent.none, cellInfo.Info, false, false);
            }

            private bool IsKeyConflicted(int index)
            {
                (string error, int _, object dictValue) = Util.GetValue(_property, _info, _parent);
                if (error != "")
                {
                    return false;
                }

                IEnumerable allKeyList = _keyCellContext.FieldInfo.GetValue(dictValue) as IEnumerable;
                if (allKeyList == null)
                {
                    return false;
                }

                object[] keys = allKeyList.Cast<object>().ToArray();
                if (index < 0 || index >= keys.Length)
                {
                    return false;
                }

                object thisKey = keys[index];
                for (int compareIndex = 0; compareIndex < keys.Length; compareIndex++)
                {
                    if (compareIndex == index)
                    {
                        continue;
                    }

                    if (Util.GetIsEqual(keys[compareIndex], thisKey))
                    {
                        return true;
                    }
                }

                return false;
            }

            private int[] _draggedPropertyIndexes = Array.Empty<int>();

            protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
            {
                if (args.performDrop && _draggedPropertyIndexes.Length > 0 && _itemIndexToPropertyIndex.Count > 0)
                {
                    List<(int fromIndex, int toIndex)> propIndexFromTo = new List<(int fromIndex, int toIndex)>();
                    foreach (int fromIndex in _draggedPropertyIndexes)
                    {
                        int insertIndex = Mathf.Clamp(args.insertAtIndex, 0, _itemIndexToPropertyIndex.Count - 1);
                        int toIndex = _itemIndexToPropertyIndex[insertIndex];
                        propIndexFromTo.Add((fromIndex, toIndex));
                    }

                    _draggedPropertyIndexes = Array.Empty<int>();
                    IndexSwapEvent.Invoke(propIndexFromTo);
                }

                return DragAndDropVisualMode.Move;
            }

            protected override bool CanStartDrag(CanStartDragArgs args)
            {
                _draggedPropertyIndexes = args.draggedItemIDs.OrderByDescending(each => each).ToArray();
                return true;
            }

            protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.StartDrag("SaintsDictionary");
            }

            public IReadOnlyList<int> GetSelectedIndex() => GetSelection().ToArray();
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private const double DebounceTimeIMGUI = 0.6d;
        private const float PagerInputWidth = 30f;
        private const float PagerItemsLabelWidth = 65f;
        private const float PagerButtonWidth = 19f;
        private const float PagerPageLabelWidth = 30f;
        private const float PagerSepWidth = 8f;
        private const float SearchGap = 5f;
        private const float SizeWidth = 50f;
        private const float ButtonWidth = 18f;
        private const float ControlGap = 4f;
        private const float TableHeightPadding = 4f;

        private static Texture2D _leftIcon;
        private static Texture2D _rightIcon;
        private static GUIStyle _iconButtonStyle;

        protected override bool UseCreateFieldIMGUI => true;

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}[{index}]";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                cache.AsyncSearchItems.SourceGenerator?.Dispose();
                cache.AsyncSearchItems.SourceGenerator = null;
                InfoCacheIMGUI.Remove(key);
            });
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            (string contextError, DictionaryContextIMGUI context) =
                TryBuildContext(property, label, saintsAttribute as SaintsDictionaryAttribute, info, parent);
            if (contextError != "")
            {
                return GetContextErrorHeight(width, contextError);
            }

            InfoIMGUI cache = EnsureKey(property, index);
            cache.Context = context;

            EnsureSearchState(cache, context.KeysProp, context.ValuesProp, context.Attribute);
            TickAsyncSearch(cache, context.KeysProp, context.ValuesProp);
            EnsureTable(cache, width);
            SyncTableItems(cache);

            if (!property.isExpanded)
            {
                return SingleLineHeight;
            }

            bool searchable = context.Attribute?.Searchable ?? true;
            return cache.Table.totalHeight + TableHeightPadding + SingleLineHeight * 2f + (searchable ? SingleLineHeight : 0f);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            (string contextError, DictionaryContextIMGUI context) =
                TryBuildContext(property, label, saintsAttribute as SaintsDictionaryAttribute, info, parent);
            if (contextError != "")
            {
                DrawContextError(position, property, label, contextError);
                return;
            }

            InfoIMGUI cache = EnsureKey(property, index);
            cache.Context = context;

            EnsureSearchState(cache, context.KeysProp, context.ValuesProp, context.Attribute);
            TickAsyncSearch(cache, context.KeysProp, context.ValuesProp);
            EnsureTable(cache, position.width);
            SyncTableItems(cache);

            EnsureIcons();

            SaintsDictionaryAttribute saintsDictionaryAttribute = context.Attribute;
            bool searchable = saintsDictionaryAttribute?.Searchable ?? true;
            bool hasPaging = cache.AsyncSearchItems.NumberOfItemsPerPage > 0;

            Rect headerRect = new Rect(position)
            {
                height = SingleLineHeight,
            };

            Rect sizeRect = new Rect(headerRect)
            {
                x = headerRect.xMax - SizeWidth,
                width = SizeWidth,
            };
            Rect foldoutRect = new Rect(headerRect)
            {
                width = Mathf.Max(0f, headerRect.width - SizeWidth - ControlGap),
            };

            Rect foldoutUseRect = ShrinkRect(foldoutRect);
            property.isExpanded = EditorGUI.Foldout(foldoutUseRect, property.isExpanded, context.Label, true);

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(ShrinkRect(sizeRect), GUIContent.none, context.KeysProp.arraySize);
                if (changed.changed)
                {
                    SetArraySize(cache, context.KeysProp, context.ValuesProp, Mathf.Max(newSize, 0));
                    ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                    return;
                }
            }

            if (!property.isExpanded)
            {
                return;
            }

            Rect contentRect = new Rect(position)
            {
                y = headerRect.yMax,
                height = Mathf.Max(0f, position.height - headerRect.height),
            };
            GUI.Box(contentRect, GUIContent.none, EditorStyles.helpBox);

            Rect workRect = new Rect(contentRect)
            {
                x = contentRect.x + 1f,
                y = contentRect.y + 1f,
                width = Mathf.Max(0f, contentRect.width - 2f),
                height = Mathf.Max(0f, contentRect.height - 2f),
            };

            if (searchable)
            {
                (Rect searchRect, Rect restRect) = RectUtils.SplitHeightRect(workRect, SingleLineHeight);
                workRect = restRect;

                (Rect keySearchRawRect, Rect valueSearchRawRect) = RectUtils.SplitWidthRect(searchRect, searchRect.width * 0.5f);
                DrawSearchField(ShrinkRect(keySearchRawRect), true, cache, context);
                DrawSearchField(ShrinkRect(valueSearchRawRect), false, cache, context);
            }

            (Rect tableRect, Rect footerRect) = RectUtils.SplitHeightRect(workRect, Mathf.Max(0f, workRect.height - SingleLineHeight));

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                cache.Table.OnGUI(tableRect);
                if (changed.changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    cache.Table.Reload();
                    ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                }
            }

            DrawFooter(ShrinkRect(footerRect), cache, context, hasPaging);
        }

        private void DrawSearchField(Rect rect, bool isKeySearch, InfoIMGUI cache, DictionaryContextIMGUI context)
        {
            AsyncSearchItemsIMGUI searchItems = cache.AsyncSearchItems;
            bool searching = searchItems.Started && !searchItems.Finished;
            IMGUILoading loading = isKeySearch ? cache.KeyLoading : cache.ValueLoading;
            string controlName = $"{(isKeySearch ? "SaintsDictionaryKeySearch" : "SaintsDictionaryValueSearch")}_{context.RootProperty.propertyPath}";
            string placeholder = isKeySearch ? "Key Search" : "Value Search";
            string currentText = isKeySearch ? searchItems.KeySearchText : searchItems.ValueSearchText;

            Rect fieldRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - (searching ? 16f : 0f) - SearchGap),
            };

            if (searching)
            {
                Rect loadingRect = new Rect(rect)
                {
                    x = rect.xMax - 14f,
                    width = 12f,
                };
                loading.Draw(loadingRect);
            }

            GUI.SetNextControlName(controlName);
            string newText = EditorGUI.TextField(fieldRect, GUIContent.none, currentText);
            if (newText != currentText)
            {
                RestartSearch(cache, context.KeysProp, context.ValuesProp,
                    isKeySearch ? newText : searchItems.KeySearchText,
                    isKeySearch ? searchItems.ValueSearchText : newText,
                    true);
            }

            if (Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl() == controlName
                && !searchItems.Started
                && searchItems.SourceGenerator != null
                && searchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
            {
                searchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1d;
                CompleteSearchImmediately(cache, context.KeysProp);
                SyncTableItems(cache);
                cache.Table?.Reload();
                GUI.changed = true;
            }

            string activeText = isKeySearch ? searchItems.KeySearchText : searchItems.ValueSearchText;
            if (string.IsNullOrEmpty(activeText))
            {
                EditorGUI.LabelField(new Rect(rect)
                {
                    width = Mathf.Max(0f, rect.width - 6f),
                }, placeholder, PlaceholderStyle);
            }
        }

        private void DrawFooter(Rect rect, InfoIMGUI cache, DictionaryContextIMGUI context, bool hasPaging)
        {
            Rect controlsRect = rect;
            Rect addButtonRect = new Rect(controlsRect)
            {
                x = controlsRect.xMax - ButtonWidth,
                width = ButtonWidth,
            };
            Rect removeButtonRect = new Rect(addButtonRect)
            {
                x = addButtonRect.x - ControlGap - ButtonWidth,
            };
            Rect pagingRect = new Rect(controlsRect)
            {
                width = Mathf.Max(0f, removeButtonRect.x - ControlGap - controlsRect.x),
            };

            if (hasPaging)
            {
                Rect numberOfItemsPerPageRect = new Rect(pagingRect)
                {
                    width = PagerInputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newNumberOfItemsPerPage = EditorGUI.DelayedIntField(numberOfItemsPerPageRect, GUIContent.none,
                        cache.AsyncSearchItems.NumberOfItemsPerPage);
                    if (changed.changed)
                    {
                        cache.AsyncSearchItems.NumberOfItemsPerPage = Mathf.Max(newNumberOfItemsPerPage, 0);
                        cache.AsyncSearchItems.PageIndex = 0;
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
                        SyncTableItems(cache);
                    }
                }

                Rect numberOfItemsSepRect = new Rect(numberOfItemsPerPageRect)
                {
                    x = numberOfItemsPerPageRect.xMax,
                    width = PagerSepWidth,
                };
                EditorGUI.LabelField(numberOfItemsSepRect, "/");

                Rect totalItemsRect = new Rect(numberOfItemsSepRect)
                {
                    x = numberOfItemsSepRect.xMax,
                    width = PagerItemsLabelWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newCount = EditorGUI.DelayedIntField(totalItemsRect, GUIContent.none, context.KeysProp.arraySize);
                    if (changed.changed)
                    {
                        SetArraySize(cache, context.KeysProp, context.ValuesProp, Mathf.Max(newCount, 0));
                        ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                        return;
                    }
                }
                EditorGUI.LabelField(totalItemsRect, "Items", PlaceholderStyle);

                Rect prePageRect = new Rect(totalItemsRect)
                {
                    x = totalItemsRect.xMax,
                    width = PagerButtonWidth,
                };
                using (new EditorGUI.DisabledScope(cache.AsyncSearchItems.PageIndex <= 0))
                {
                    if (GUI.Button(prePageRect, _leftIcon, EditorStyles.miniButtonLeft))
                    {
                        cache.AsyncSearchItems.PageIndex = Mathf.Max(0, cache.AsyncSearchItems.PageIndex - 1);
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
                        SyncTableItems(cache);
                    }
                }

                Rect pageRect = new Rect(prePageRect)
                {
                    x = prePageRect.xMax,
                    width = PagerInputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newPageIndex = EditorGUI.DelayedIntField(pageRect, GUIContent.none,
                        cache.AsyncSearchItems.PageIndex + 1) - 1;
                    if (changed.changed)
                    {
                        cache.AsyncSearchItems.PageIndex = Mathf.Max(newPageIndex, 0);
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
                        SyncTableItems(cache);
                    }
                }

                Rect totalPageRect = new Rect(pageRect)
                {
                    x = pageRect.xMax,
                    width = PagerPageLabelWidth,
                };
                EditorGUI.LabelField(totalPageRect, $"/ {cache.AsyncSearchItems.TotalPage}");

                Rect nextPageRect = new Rect(totalPageRect)
                {
                    x = totalPageRect.xMax,
                    width = PagerButtonWidth,
                };
                using (new EditorGUI.DisabledScope(cache.AsyncSearchItems.PageIndex >= cache.AsyncSearchItems.TotalPage - 1))
                {
                    if (GUI.Button(nextPageRect, _rightIcon, EditorStyles.miniButtonRight))
                    {
                        cache.AsyncSearchItems.PageIndex = Mathf.Min(cache.AsyncSearchItems.PageIndex + 1,
                            cache.AsyncSearchItems.TotalPage - 1);
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
                        SyncTableItems(cache);
                    }
                }
            }

            if (GUI.Button(removeButtonRect, "-", IconButtonStyle))
            {
                List<int> selected = cache.Table.GetSelectedIndex().OrderByDescending(each => each).ToList();
                if (selected.Count == 0)
                {
                    int curSize = context.KeysProp.arraySize;
                    if (curSize == 0)
                    {
                        return;
                    }

                    selected.Add(curSize - 1);
                }

                DecreaseArraySize(selected, context.KeysProp, context.ValuesProp);
                ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                RestartSearch(cache, context.KeysProp, context.ValuesProp, cache.AsyncSearchItems.KeySearchText,
                    cache.AsyncSearchItems.ValueSearchText, false);
                SyncTableItems(cache);
                cache.Table.Reload();
                GUI.changed = true;
            }

            if (GUI.Button(addButtonRect, "+", IconButtonStyle))
            {
                IncreaseArraySize(context.KeysProp.arraySize + 1, context.KeysProp, context.ValuesProp);
                ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                RestartSearch(cache, context.KeysProp, context.ValuesProp, cache.AsyncSearchItems.KeySearchText,
                    cache.AsyncSearchItems.ValueSearchText, false);
                SyncTableItems(cache);
                cache.Table.Reload();
                GUI.changed = true;
            }
        }

        private static float GetContextErrorHeight(float width, string contextError) =>
            SingleLineHeight + ImGuiHelpBox.GetHeight(contextError, width, MessageType.Error);

        private void DrawContextError(Rect position, SerializedProperty property, GUIContent label,
            string contextError)
        {
            Rect labelRect = new Rect(position)
            {
                height = SingleLineHeight,
            };
            GUIContent useLabel = label;
            if (useLabel == null || string.IsNullOrWhiteSpace(useLabel.text))
            {
                useLabel = new GUIContent(GetPreferredLabel(property));
            }

            EditorGUI.LabelField(labelRect, useLabel);

            Rect helpBoxRect = new Rect(position)
            {
                y = labelRect.yMax,
                height = Mathf.Max(0f, position.height - SingleLineHeight),
            };
            ImGuiHelpBox.Draw(helpBoxRect, contextError, MessageType.Error);
        }

        private (string error, DictionaryContextIMGUI context) TryBuildContext(SerializedProperty property,
            GUIContent label, SaintsDictionaryAttribute saintsDictionaryAttribute, FieldInfo info, object parent)
        {
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;

            Type rawType = insideArray ? ReflectUtils.GetElementType(info.FieldType) : info.FieldType;
            if (rawType == null)
            {
                return ($"Failed to get dictionary raw type from {property.propertyPath}", null);
            }

            (string keysPropNameCompact, string valuesPropNameCompact) = GetKeysValuesPropName(rawType);
            SerializedProperty keysProp = FindPropertyCompact(property, keysPropNameCompact);
            SerializedProperty valuesProp = FindPropertyCompact(property, valuesPropNameCompact);
            if (keysProp == null || valuesProp == null)
            {
                return
                    ($"Failed to find dictionary props `{keysPropNameCompact}` / `{valuesPropNameCompact}` from {property.propertyPath}",
                        null);
            }

            object fieldValue;
            try
            {
                fieldValue = info.GetValue(parent);
            }
            catch (Exception e)
            {
                return ($"Failed to get dictionary field value from {property.propertyPath}: {e.Message}", null);
            }

            if (insideArray && fieldValue is IEnumerable enumerable)
            {
                object[] arrayValues = enumerable.Cast<object>().ToArray();
                if (arrayIndex < 0 || arrayIndex >= arrayValues.Length)
                {
                    return
                        ($"Failed to get dictionary array element at index {arrayIndex} from {property.propertyPath}",
                            null);
                }

                fieldValue = arrayValues[arrayIndex];
            }

            (FieldInfo keysField, object _) = GetTargetInfo(keysPropNameCompact, rawType, fieldValue);
            (FieldInfo valuesField, object __) = GetTargetInfo(valuesPropNameCompact, rawType, fieldValue);
            if (keysField == null || valuesField == null)
            {
                return
                    ($"Failed to get dictionary key/value fields `{keysPropNameCompact}` / `{valuesPropNameCompact}` from {property.propertyPath}",
                        null);
            }

            CellContextIMGUI keyCellContext = BuildKeyCellContext(property, info, keysField);
            CellContextIMGUI valueCellContext = BuildValueCellContext(property, info, valuesField);

            string labelText = string.IsNullOrWhiteSpace(label?.text) ? GetPreferredLabel(property) : label.text;
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = "Value";
            }

            return ("", new DictionaryContextIMGUI
            {
                RootProperty = property,
                KeysProp = keysProp,
                ValuesProp = valuesProp,
                Attribute = saintsDictionaryAttribute,
                Info = info,
                Parent = parent,
                Label = labelText,
                KeyWidth = saintsDictionaryAttribute?.KeyWidth ?? default,
                ValueWidth = saintsDictionaryAttribute?.ValueWidth ?? default,
                KeyCellContext = keyCellContext,
                ValueCellContext = valueCellContext,
            });
        }

        private static CellContextIMGUI BuildKeyCellContext(SerializedProperty property, FieldInfo info, FieldInfo keysField)
        {
            List<Attribute> keyInjectCreatedAttributes = new List<Attribute>();
            bool keyHasSerializeReference = false;
            foreach (KeyAttributeAttribute injectAttribute in ReflectCache.GetCustomAttributes<KeyAttributeAttribute>(info))
            {
                if (injectAttribute.Decorator == typeof(SerializeReference))
                {
                    keyHasSerializeReference = true;
                    continue;
                }

                ValueAttributeAttribute less1DepthInject = new ValueAttributeAttribute(injectAttribute.Depth - 1,
                    injectAttribute.Decorator, injectAttribute.Parameters);
                if (less1DepthInject.Depth == 0)
                {
                    Attribute injectedAttribute = SaintsWrapUtils.CreateInjectedAttribute(injectAttribute);
                    if (injectedAttribute != null)
                    {
                        keyInjectCreatedAttributes.Add(injectedAttribute);
                    }
                }
            }

            return new CellContextIMGUI
            {
                FieldInfo = keysField,
                RawType = ReflectUtils.GetElementType(keysField.FieldType),
                WrapType = SaintsWrapUtils.EnsureWrapType(property.FindPropertyRelative("_wrapTypeKey"), keysField,
                    keyHasSerializeReference),
                HasSerializeReference = keyHasSerializeReference,
                Attributes = keyInjectCreatedAttributes.ToArray(),
            };
        }

        private static CellContextIMGUI BuildValueCellContext(SerializedProperty property, FieldInfo info,
            FieldInfo valuesField)
        {
            List<Attribute> valueInjectCreatedAttributes = new List<Attribute>();
            bool valueHasSerializeReference = false;
            foreach (ValueAttributeAttribute injectAttribute in ReflectCache.GetCustomAttributes<ValueAttributeAttribute>(info))
            {
                if (injectAttribute.Decorator == typeof(SerializeReference))
                {
                    valueHasSerializeReference = true;
                    continue;
                }

                ValueAttributeAttribute less1DepthInject = new ValueAttributeAttribute(injectAttribute.Depth - 1,
                    injectAttribute.Decorator, injectAttribute.Parameters);
                if (less1DepthInject.Depth == 0)
                {
                    Attribute injectedAttribute = SaintsWrapUtils.CreateInjectedAttribute(injectAttribute);
                    if (injectedAttribute != null)
                    {
                        valueInjectCreatedAttributes.Add(injectedAttribute);
                    }
                }
            }

            return new CellContextIMGUI
            {
                FieldInfo = valuesField,
                RawType = ReflectUtils.GetElementType(valuesField.FieldType),
                WrapType = SaintsWrapUtils.EnsureWrapType(property.FindPropertyRelative("_wrapTypeValue"), valuesField,
                    valueHasSerializeReference),
                HasSerializeReference = valueHasSerializeReference,
                Attributes = valueInjectCreatedAttributes.ToArray(),
            };
        }

        private void EnsureTable(InfoIMGUI cache, float width)
        {
            DictionaryContextIMGUI context = cache.Context;
            if (context == null || context.KeysProp == null || context.ValuesProp == null)
            {
                return;
            }

            bool recreate = cache.Table == null
                            || cache.KeysPropPath != context.KeysProp.propertyPath
                            || cache.ValuesPropPath != context.ValuesProp.propertyPath;

            if (recreate)
            {
                cache.KeysPropPath = context.KeysProp.propertyPath;
                cache.ValuesPropPath = context.ValuesProp.propertyPath;

                MultiColumnHeaderState.Column[] columns =
                {
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent(GetKeyLabel(context.Attribute)),
                        autoResize = false,
                        canSort = false,
                        allowToggleVisibility = false,
                        minWidth = 40f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent(GetValueLabel(context.Attribute)),
                        autoResize = false,
                        canSort = false,
                        allowToggleVisibility = false,
                        minWidth = 40f,
                    },
                };

                cache.Table = new SaintsDictionaryTable(
                    new TreeViewState
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                    (),
                    new MultiColumnHeader(new MultiColumnHeaderState(columns)),
                    context.RootProperty,
                    context.KeysProp,
                    context.ValuesProp,
                    context.KeyCellContext,
                    context.ValueCellContext,
                    context.Info,
                    context.Parent,
                    context.KeyWidth,
                    context.ValueWidth);

                cache.Table.IndexSwapEvent.AddListener(swaps =>
                {
                    DictionaryContextIMGUI currentContext = cache.Context;
                    if (currentContext?.KeysProp == null || currentContext.ValuesProp == null)
                    {
                        return;
                    }

                    foreach ((int fromIndex, int toIndex) in swaps)
                    {
                        currentContext.KeysProp.MoveArrayElement(fromIndex, toIndex);
                        currentContext.ValuesProp.MoveArrayElement(fromIndex, toIndex);
                    }

                    ApplyAndTrigger(currentContext.RootProperty, currentContext.Info, currentContext.Parent);
                    RestartSearch(cache, currentContext.KeysProp, currentContext.ValuesProp,
                        cache.AsyncSearchItems.KeySearchText, cache.AsyncSearchItems.ValueSearchText, false);
                    cache.Table.Reload();
                });
            }

            cache.Table.SetColumnWidths(width);
        }

        private static void EnsureSearchState(InfoIMGUI cache, SerializedProperty keysProp, SerializedProperty valuesProp,
            SaintsDictionaryAttribute attribute)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            int numberOfItemsPerPage = attribute?.NumberOfItemsPerPage ?? -1;

            if (asyncSearchItems.Size == 0
                && asyncSearchItems.HitTargetIndexes.Count == 0
                && asyncSearchItems.CachedHitTargetIndexes.Count == 0
                && string.IsNullOrEmpty(asyncSearchItems.KeySearchText)
                && string.IsNullOrEmpty(asyncSearchItems.ValueSearchText))
            {
                asyncSearchItems.NumberOfItemsPerPage = numberOfItemsPerPage;
                SetFullResults(asyncSearchItems, keysProp.arraySize);
                return;
            }

            if (asyncSearchItems.NumberOfItemsPerPage != numberOfItemsPerPage)
            {
                asyncSearchItems.NumberOfItemsPerPage = numberOfItemsPerPage;
                UpdateVisibleIndexes(asyncSearchItems);
            }

            if (asyncSearchItems.Size != keysProp.arraySize)
            {
                RestartSearch(cache, keysProp, valuesProp, asyncSearchItems.KeySearchText,
                    asyncSearchItems.ValueSearchText, false);
            }
        }

        private static void RestartSearch(InfoIMGUI cache, SerializedProperty keysProp, SerializedProperty valuesProp,
            string keySearchText, string valueSearchText, bool resetPage)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            string safeKeySearch = keySearchText ?? "";
            string safeValueSearch = valueSearchText ?? "";

            if (resetPage)
            {
                asyncSearchItems.PageIndex = 0;
            }

            asyncSearchItems.Size = keysProp.arraySize;
            asyncSearchItems.SourceGenerator?.Dispose();
            asyncSearchItems.SourceGenerator = null;

            if (string.IsNullOrEmpty(safeKeySearch) && string.IsNullOrEmpty(safeValueSearch))
            {
                asyncSearchItems.KeySearchText = "";
                asyncSearchItems.ValueSearchText = "";
                SetFullResults(asyncSearchItems, keysProp.arraySize);
                return;
            }

            IReadOnlyList<int> currentResults = GetCurrentResults(asyncSearchItems);
            asyncSearchItems.CachedHitTargetIndexes.Clear();
            asyncSearchItems.CachedHitTargetIndexes.AddRange(currentResults);
            asyncSearchItems.HitTargetIndexes.Clear();
            asyncSearchItems.KeySearchText = safeKeySearch;
            asyncSearchItems.ValueSearchText = safeValueSearch;
            asyncSearchItems.Started = false;
            asyncSearchItems.Finished = false;
            asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTimeIMGUI;
            asyncSearchItems.SourceGenerator = Search(keysProp, valuesProp, safeKeySearch, safeValueSearch).GetEnumerator();
            UpdateVisibleIndexes(asyncSearchItems);
        }

        private static void SetFullResults(AsyncSearchItemsIMGUI asyncSearchItems, int size)
        {
            asyncSearchItems.Started = true;
            asyncSearchItems.Finished = true;
            asyncSearchItems.Size = size;
            asyncSearchItems.SourceGenerator?.Dispose();
            asyncSearchItems.SourceGenerator = null;
            asyncSearchItems.HitTargetIndexes.Clear();
            asyncSearchItems.HitTargetIndexes.AddRange(Enumerable.Range(0, size));
            asyncSearchItems.CachedHitTargetIndexes.Clear();
            asyncSearchItems.CachedHitTargetIndexes.AddRange(asyncSearchItems.HitTargetIndexes);
            UpdateVisibleIndexes(asyncSearchItems);
        }

        private static void TickAsyncSearch(InfoIMGUI cache, SerializedProperty keysProp, SerializedProperty valuesProp)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!asyncSearchItems.Started
                && asyncSearchItems.SourceGenerator != null
                && EditorApplication.timeSinceStartup > asyncSearchItems.DebounceSearchTime)
            {
                asyncSearchItems.Started = true;
                UpdateVisibleIndexes(asyncSearchItems);
            }

            if (!asyncSearchItems.Started || asyncSearchItems.Finished || asyncSearchItems.SourceGenerator == null)
            {
                return;
            }

            bool emptySearch = string.IsNullOrEmpty(asyncSearchItems.KeySearchText)
                               && string.IsNullOrEmpty(asyncSearchItems.ValueSearchText);
            int searchBatch = emptySearch ? int.MaxValue : 50;
            bool needRefresh = false;

            for (int searchTick = 0; searchTick < searchBatch; searchTick++)
            {
                if (asyncSearchItems.SourceGenerator.MoveNext())
                {
                    int current = asyncSearchItems.SourceGenerator.Current;
                    if (current != -1)
                    {
                        asyncSearchItems.HitTargetIndexes.Add(current);
                        needRefresh = true;
                    }
                }
                else
                {
                    asyncSearchItems.Finished = true;
                    asyncSearchItems.CachedHitTargetIndexes.Clear();
                    asyncSearchItems.CachedHitTargetIndexes.AddRange(asyncSearchItems.HitTargetIndexes);
                    asyncSearchItems.SourceGenerator.Dispose();
                    asyncSearchItems.SourceGenerator = null;
                    needRefresh = true;
                    break;
                }
            }

            if (needRefresh)
            {
                asyncSearchItems.Size = keysProp.arraySize;
                UpdateVisibleIndexes(asyncSearchItems);
            }
        }

        private static void CompleteSearchImmediately(InfoIMGUI cache, SerializedProperty keysProp)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            if (asyncSearchItems.SourceGenerator == null || asyncSearchItems.Finished)
            {
                return;
            }

            asyncSearchItems.Started = true;
            while (asyncSearchItems.SourceGenerator.MoveNext())
            {
                int current = asyncSearchItems.SourceGenerator.Current;
                if (current != -1)
                {
                    asyncSearchItems.HitTargetIndexes.Add(current);
                }
            }

            asyncSearchItems.Finished = true;
            asyncSearchItems.CachedHitTargetIndexes.Clear();
            asyncSearchItems.CachedHitTargetIndexes.AddRange(asyncSearchItems.HitTargetIndexes);
            asyncSearchItems.SourceGenerator.Dispose();
            asyncSearchItems.SourceGenerator = null;
            asyncSearchItems.Size = keysProp.arraySize;
            UpdateVisibleIndexes(asyncSearchItems);
        }

        private static IReadOnlyList<int> GetCurrentResults(AsyncSearchItemsIMGUI asyncSearchItems)
        {
            return asyncSearchItems.Started ? asyncSearchItems.HitTargetIndexes : asyncSearchItems.CachedHitTargetIndexes;
        }

        private static void UpdateVisibleIndexes(AsyncSearchItemsIMGUI asyncSearchItems)
        {
            IReadOnlyList<int> source = GetCurrentResults(asyncSearchItems);
            int numberOfItemsPerPage = asyncSearchItems.NumberOfItemsPerPage;

            int pageCount;
            int curPageIndex;
            int skipStart;
            int itemCount;
            if (numberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                curPageIndex = 0;
                skipStart = 0;
                itemCount = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.Max(1, Mathf.CeilToInt(source.Count / (float)numberOfItemsPerPage));
                curPageIndex = Mathf.Clamp(asyncSearchItems.PageIndex, 0, pageCount - 1);
                skipStart = curPageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            asyncSearchItems.TotalPage = Mathf.Max(1, pageCount);
            asyncSearchItems.PageIndex = curPageIndex;
            asyncSearchItems.VisibleIndexes.Clear();
            asyncSearchItems.VisibleIndexes.AddRange(source.Skip(skipStart).Take(itemCount));
        }

        private static void SyncTableItems(InfoIMGUI cache)
        {
            cache.Table?.SetItemIndexToPropertyIndex(cache.AsyncSearchItems.VisibleIndexes);
        }

        private static void SetArraySize(InfoIMGUI cache, SerializedProperty keysProp, SerializedProperty valuesProp,
            int newSize)
        {
            if (newSize >= keysProp.arraySize)
            {
                IncreaseArraySize(newSize, keysProp, valuesProp);
            }
            else
            {
                IReadOnlyList<int> deleteIndexes = Enumerable.Range(newSize, keysProp.arraySize - newSize)
                    .Reverse()
                    .ToList();
                DecreaseArraySize(deleteIndexes, keysProp, valuesProp);
            }

            RestartSearch(cache, keysProp, valuesProp, cache.AsyncSearchItems.KeySearchText,
                cache.AsyncSearchItems.ValueSearchText, false);
        }

        private static void ApplyAndTrigger(SerializedProperty property, FieldInfo info, object parent)
        {
            property.serializedObject.ApplyModifiedProperties();
            (string error, int _, object value) = Util.GetValue(property, info, parent);
            if (error == "")
            {
                TriggerChangedIMGUI(property, value);
            }
        }

        private static void EnsureIcons()
        {
            _leftIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-left.png");
            _rightIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-right.png");
            _iconButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(0, 0, 0, 0),
            };
        }

        private static GUIStyle PlaceholderStyle => new GUIStyle("label")
        {
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.gray },
            fontStyle = FontStyle.Italic,
        };

        private static GUIStyle IconButtonStyle
        {
            get
            {
                EnsureIcons();
                return _iconButtonStyle;
            }
        }

        private static Rect ShrinkRect(Rect rect) => new Rect(rect)
        {
            y = rect.y + 1f,
            height = Mathf.Max(0f, rect.height - 2f),
        };

        private static (float keyWidth, float valueWidth) GetColumnWidths(float totalWidth,
            ResponsiveLength keyWidth, ResponsiveLength valueWidth)
        {
            float defaultWidth = Mathf.Max(40f, totalWidth * 0.5f);
            float resolvedKeyWidth = ResolveWidth(keyWidth, totalWidth, defaultWidth);
            float resolvedValueWidth = ResolveWidth(valueWidth, totalWidth, defaultWidth);

            if (keyWidth.Type == ResponsiveType.None && valueWidth.Type == ResponsiveType.None)
            {
                resolvedKeyWidth = defaultWidth;
                resolvedValueWidth = Mathf.Max(40f, totalWidth - resolvedKeyWidth);
            }
            else if (keyWidth.Type == ResponsiveType.None)
            {
                resolvedKeyWidth = Mathf.Max(40f, totalWidth - resolvedValueWidth);
            }
            else if (valueWidth.Type == ResponsiveType.None)
            {
                resolvedValueWidth = Mathf.Max(40f, totalWidth - resolvedKeyWidth);
            }

            return (resolvedKeyWidth, resolvedValueWidth);
        }

        private static float ResolveWidth(ResponsiveLength responsiveLength, float totalWidth, float defaultWidth)
        {
            switch (responsiveLength.Type)
            {
                case ResponsiveType.None:
                    return defaultWidth;
                case ResponsiveType.Pixel:
                    return Mathf.Max(40f, responsiveLength.Value);
                case ResponsiveType.Percent:
                    return Mathf.Max(40f, totalWidth * responsiveLength.Value / 100f);
                default:
                    return defaultWidth;
            }
        }
    }
}
