using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer
{
    public partial class SaintsHashSetDrawer
    {
        private sealed class AsyncSearchItemsIMGUI
        {
            public bool Started = true;
            public bool Finished = true;
            public IEnumerator<int> SourceGenerator;
            public string SearchText = "";
            public double DebounceSearchTime;
            public List<int> HitTargetIndexes = new List<int>();
            public List<int> CachedHitTargetIndexes = new List<int>();
            public readonly List<int> VisibleIndexes = new List<int>();
            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;
        }

        private sealed class ElementContext
        {
            public SerializedProperty RootProperty;
            public SerializedProperty WrapProp;
            public FieldInfo WrapField;
            public object WrapParent;
            public Type WrapType;
            public WrapType ValueWrapType;
            public bool HasSerializeReference;
            public IReadOnlyList<Attribute> CellAttributes;
            public FieldInfo Info;
            public object Parent;
            public string LabelText;
            public bool InHorizontalLayout;
        }

        private sealed class InfoIMGUI
        {
            public readonly AsyncSearchItemsIMGUI AsyncSearchItems = new AsyncSearchItemsIMGUI();
            public readonly IMGUILoading Loading = new IMGUILoading();
            public readonly Dictionary<int, float> ElementHeights = new Dictionary<int, float>();
            public ReorderableList ReorderableList;
            public ElementContext Context;
            public bool NeedsHeightRefresh;
            public bool HeightRefreshScheduled;
        }

        private sealed class UnsetGuiStyleFixedHeight : IDisposable
        {
            private readonly GUIStyle _guiStyle;
            private readonly float _oldValue;

            public UnsetGuiStyleFixedHeight(GUIStyle guiStyle)
            {
                _guiStyle = guiStyle;
                _oldValue = guiStyle.fixedHeight;
                _guiStyle.fixedHeight = 0;
            }

            public void Dispose()
            {
                _guiStyle.fixedHeight = _oldValue;
            }
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private const float SearchGap = 5f;
        private const float HeaderFoldWidth = 16f;
        private const float HeaderSizeWidth = 50f;
        private const float HeaderGap = 4f;
        private const float PagerInputWidth = 30f;
        private const float PagerItemsLabelWidth = 65f;
        private const float PagerButtonWidth = 19f;
        private const float PagerPageLabelWidth = 30f;
        private const float PagerSepWidth = 8f;

        private static Texture2D _iconLeft;
        private static Texture2D _iconRight;

        protected override bool UseCreateFieldIMGUI => true;

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                infoCache.AsyncSearchItems.SourceGenerator?.Dispose();
                infoCache.AsyncSearchItems.SourceGenerator = null;
                InfoCacheIMGUI.Remove(key);
            });
            return infoCache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width, int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth,
            object parent)
        {
            EnsureSerializedVersion(property);

            SaintsHashSetAttribute saintsHashSetAttribute =
                saintsAttribute as SaintsHashSetAttribute ?? new SaintsHashSetAttribute();
            (string contextError, ElementContext context) = TryBuildElementContext(property, label, info, parent);
            if (contextError != "")
            {
                return GetContextErrorHeight(width, contextError);
            }

            InfoIMGUI cache = EnsureKey(property);
            cache.Context = context;

            EnsureSearchState(cache, context.WrapProp, saintsHashSetAttribute);
            TickAsyncSearch(cache, context.WrapProp);
            EnsureReorderableList(cache, saintsHashSetAttribute);

            if (!property.isExpanded)
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            try
            {
                return SaintsPropertyDrawer.SingleLineHeight +
                       (cache.ReorderableList?.GetHeight() ?? SaintsPropertyDrawer.SingleLineHeight);
            }
            catch (ObjectDisposedException)
            {
                cache.ReorderableList = null;
            }
            catch (NullReferenceException)
            {
                cache.ReorderableList = null;
            }

            EnsureReorderableList(cache, saintsHashSetAttribute);
            return SaintsPropertyDrawer.SingleLineHeight +
                   (cache.ReorderableList?.GetHeight() ?? SaintsPropertyDrawer.SingleLineHeight);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            EnsureSerializedVersion(property);

            SaintsHashSetAttribute saintsHashSetAttribute =
                saintsAttribute as SaintsHashSetAttribute ?? new SaintsHashSetAttribute();
            (string contextError, ElementContext context) = TryBuildElementContext(property, label, info, parent);
            if (contextError != "")
            {
                DrawContextError(position, property, label, contextError);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            InfoIMGUI cache = EnsureKey(property);
            cache.Context = context;

            EnsureSearchState(cache, context.WrapProp, saintsHashSetAttribute);
            TickAsyncSearch(cache, context.WrapProp);

            Rect usePosition = new Rect(position)
            {
                y = position.y + 1f,
                height = Mathf.Max(0f, position.height - 2f),
            };

            Rect headerRect = new Rect(usePosition)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            DrawHeader(headerRect, cache);

            if (!property.isExpanded)
            {
                return;
            }

            EnsureReorderableList(cache, saintsHashSetAttribute);
            Rect listRect = new Rect(usePosition)
            {
                y = headerRect.yMax,
                height = Mathf.Max(0f, usePosition.yMax - headerRect.yMax),
            };

            using (new UnsetGuiStyleFixedHeight("RL Header"))
            {
                try
                {
                    cache.ReorderableList?.DoList(listRect);
                }
                catch (ObjectDisposedException)
                {
                    cache.ReorderableList = null;
                }
                catch (NullReferenceException)
                {
                    cache.ReorderableList = null;
                }
            }
        }

        private void EnsureReorderableList(InfoIMGUI cache, SaintsHashSetAttribute saintsHashSetAttribute)
        {
            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null)
            {
                return;
            }

            if (cache.NeedsHeightRefresh)
            {
                cache.NeedsHeightRefresh = false;
                cache.ReorderableList = null;
            }

            bool hasHeaderControls = HasHeaderControls(saintsHashSetAttribute);
            float expectedHeaderHeight = hasHeaderControls
                ? SaintsPropertyDrawer.SingleLineHeight
                : 0f;

            if (cache.ReorderableList != null)
            {
                cache.ReorderableList.headerHeight = expectedHeaderHeight;
                return;
            }

            cache.ReorderableList = new ReorderableList(context.WrapProp.serializedObject, context.WrapProp, true,
                hasHeaderControls, true, true)
            {
                headerHeight = expectedHeaderHeight,
            };
            cache.ReorderableList.drawHeaderCallback += rect => DrawHeaderControls(rect, cache, saintsHashSetAttribute);
            cache.ReorderableList.elementHeightCallback += itemIndex => DrawElementHeight(cache, itemIndex);
            cache.ReorderableList.drawElementCallback += (rect, itemIndex, _, _) => DrawElement(rect, cache, itemIndex);
            cache.ReorderableList.onAddCallback += _ =>
            {
                ElementContext currentContext = cache.Context;
                if (currentContext?.WrapProp == null)
                {
                    return;
                }

                IncreaseArraySize(currentContext.WrapProp.arraySize + 1, currentContext.WrapProp);
                ApplyAndTrigger(currentContext.RootProperty, currentContext.Info, currentContext.Parent);
                RestartSearch(cache, currentContext.WrapProp, cache.AsyncSearchItems.SearchText, false);
            };
            cache.ReorderableList.onRemoveCallback += list =>
            {
                ElementContext currentContext = cache.Context;
                if (currentContext?.WrapProp == null)
                {
                    return;
                }

                int removeIndex = list.index >= 0 ? list.index : currentContext.WrapProp.arraySize - 1;
                if (removeIndex < 0)
                {
                    return;
                }

                DecreaseArraySize(new[] { removeIndex }, currentContext.WrapProp);
                ApplyAndTrigger(currentContext.RootProperty, currentContext.Info, currentContext.Parent);
                RestartSearch(cache, currentContext.WrapProp, cache.AsyncSearchItems.SearchText, false);
            };
            cache.ReorderableList.onReorderCallbackWithDetails += (_, _, _) =>
            {
                ElementContext currentContext = cache.Context;
                if (currentContext?.WrapProp == null)
                {
                    return;
                }

                ApplyAndTrigger(currentContext.RootProperty, currentContext.Info, currentContext.Parent);
                RestartSearch(cache, currentContext.WrapProp, cache.AsyncSearchItems.SearchText, false);
            };
        }

        private float DrawElementHeight(InfoIMGUI cache, int itemIndex)
        {
            float height = GetElementHeight(cache, itemIndex);
            cache.ElementHeights[itemIndex] = height;
            return height;
        }

        private float GetElementHeight(InfoIMGUI cache, int itemIndex)
        {
            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null || itemIndex >= context.WrapProp.arraySize ||
                !cache.AsyncSearchItems.VisibleIndexes.Contains(itemIndex))
            {
                return 0f;
            }

            SerializedProperty elementProp = context.WrapProp.GetArrayElementAtIndex(itemIndex);
            elementProp.isExpanded = true;

            SaintsWrapUtils.CellInfoIMGUI cellInfo = SaintsWrapUtils.GetCellInfoIMGUI(context.ValueWrapType,
                context.WrapField, context.WrapType, elementProp, context.CellAttributes, context.HasSerializeReference,
                context.InHorizontalLayout, $"Element {itemIndex}");
            if (!cellInfo.IsValid)
            {
                return EditorGUI.GetPropertyHeight(elementProp, true);
            }

            cellInfo.Property.isExpanded = true;
            string labelText = $"Element {itemIndex}";
            return IMGUIRawDraw.GetPropertyHeight(cellInfo.Drawer, new GUIContent(labelText), cellInfo.Property,
                cellInfo.Attributes, cellInfo.RawType, cellInfo.Info, context.InHorizontalLayout);
        }

        private void DrawElement(Rect rect, InfoIMGUI cache, int itemIndex)
        {
            if (rect.height <= 0f)
            {
                return;
            }

            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null || itemIndex >= context.WrapProp.arraySize ||
                !cache.AsyncSearchItems.VisibleIndexes.Contains(itemIndex))
            {
                return;
            }

            if (IsConflicted(context, itemIndex))
            {
                EditorGUI.DrawRect(rect, new Color(WarningColor.r, WarningColor.g, WarningColor.b, 0.22f));
            }

            SerializedProperty elementProp = context.WrapProp.GetArrayElementAtIndex(itemIndex);
            elementProp.isExpanded = true;

            SaintsWrapUtils.CellInfoIMGUI cellInfo = SaintsWrapUtils.GetCellInfoIMGUI(context.ValueWrapType,
                context.WrapField, context.WrapType, elementProp, context.CellAttributes, context.HasSerializeReference,
                context.InHorizontalLayout, $"Element {itemIndex}");
            if (!cellInfo.IsValid)
            {
                EditorGUI.PropertyField(rect, elementProp, new GUIContent($"Element {itemIndex}"), true);
                return;
            }

            cellInfo.Property.isExpanded = true;
            string labelText = $"Element {itemIndex}";
            GUIContent guiContent = new GUIContent(labelText);
            float oldHeight = cache.ElementHeights.TryGetValue(itemIndex, out float cachedHeight)
                ? cachedHeight
                : rect.height;

            Rect useRect = cellInfo.ShouldIndent
                ? new Rect(rect)
                {
                    x = rect.x + 12f,
                    width = Mathf.Max(0f, rect.width - 12f),
                }
                : rect;

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                IMGUIRawDraw.OnGUI(cellInfo.Drawer, useRect, cellInfo.Property, cellInfo.Attributes, cellInfo.RawType,
                    guiContent, null, cellInfo.Info, context.InHorizontalLayout, false);
                if (changed.changed)
                {
                    ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                }
            }

            float newHeight = GetElementHeight(cache, itemIndex);
            if (Mathf.Abs(oldHeight - newHeight) > 0.5f)
            {
                cache.ElementHeights[itemIndex] = newHeight;
                RequestHeightRefresh(cache);
            }
        }

        private static void RequestHeightRefresh(InfoIMGUI cache)
        {
            cache.NeedsHeightRefresh = true;
            GUI.changed = true;
            if (cache.HeightRefreshScheduled)
            {
                return;
            }

            cache.HeightRefreshScheduled = true;
            EditorApplication.delayCall += () =>
            {
                cache.HeightRefreshScheduled = false;
                InternalEditorUtility.RepaintAllViews();
            };
        }

        private static bool IsConflicted(ElementContext context, int itemIndex)
        {
            if (context.WrapField == null || context.WrapParent == null)
            {
                return false;
            }

            IEnumerable allValues = context.WrapField.GetValue(context.WrapParent) as IEnumerable;
            if (allValues == null)
            {
                return false;
            }

            object[] values = allValues.Cast<object>().ToArray();
            if (itemIndex < 0 || itemIndex >= values.Length)
            {
                return false;
            }

            object thisValue = values[itemIndex];
            for (int compareIndex = 0; compareIndex < values.Length; compareIndex++)
            {
                if (compareIndex == itemIndex)
                {
                    continue;
                }

                if (Util.GetIsEqual(values[compareIndex], thisValue))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasHeaderControls(SaintsHashSetAttribute saintsHashSetAttribute) =>
            saintsHashSetAttribute.Searchable || saintsHashSetAttribute.NumberOfItemsPerPage > 0;

        private void DrawHeader(Rect rect, InfoIMGUI cache)
        {
            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null)
            {
                return;
            }

            EnsureHeaderIcons();

            Rect titleRect = new Rect(rect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            Rect sizeRect = new Rect(titleRect)
            {
                x = titleRect.xMax - HeaderSizeWidth,
                width = HeaderSizeWidth,
            };
            Rect titleAreaRect = new Rect(titleRect)
            {
                width = Mathf.Max(0f, titleRect.width - HeaderSizeWidth - HeaderGap),
            };
            Rect titleFoldRect = new Rect(titleAreaRect)
            {
                width = HeaderFoldWidth,
            };
            Rect titleButtonRect = new Rect(titleAreaRect)
            {
                x = titleFoldRect.xMax,
                width = Mathf.Max(0f, titleAreaRect.width - HeaderFoldWidth),
            };

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool expanded = EditorGUI.Foldout(titleAreaRect, context.RootProperty.isExpanded,
                    context.LabelText, true);
                if (changed.changed)
                {
                    context.RootProperty.isExpanded = expanded;
                    return;
                }
            }

            DrawOverrideRichText(titleRect, new GUIContent(context.LabelText), overrideRichTextChunks);

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, GUIContent.none, context.WrapProp.arraySize);
                if (changed.changed)
                {
                    SetArraySize(cache, context.WrapProp, Mathf.Max(newSize, 0));
                    ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                    return;
                }
            }
        }

        private void DrawHeaderControls(Rect rect, InfoIMGUI cache, SaintsHashSetAttribute saintsHashSetAttribute)
        {
            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null)
            {
                return;
            }

            EnsureHeaderIcons();

            bool hasSearch = saintsHashSetAttribute.Searchable;
            bool hasPaging = saintsHashSetAttribute.NumberOfItemsPerPage > 0;
            if (!hasSearch && !hasPaging)
            {
                return;
            }

            Rect controlRect = new Rect(rect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            float searchWidth = controlRect.width - PagerInputWidth * 2f - PagerItemsLabelWidth - PagerSepWidth -
                                PagerButtonWidth * 2f - PagerPageLabelWidth;
            Rect searchRect = hasPaging
                ? new Rect(controlRect)
                {
                    width = Mathf.Max(0f, searchWidth),
                }
                : controlRect;
            Rect pagingRect = new Rect(controlRect)
            {
                x = searchRect.xMax,
                width = Mathf.Max(0f, controlRect.xMax - searchRect.xMax),
            };

            if (hasSearch)
            {
                string searchControlName = $"SaintsHashSetSearch_{context.RootProperty.propertyPath}";
                string oldSearch = cache.AsyncSearchItems.SearchText;
                Rect searchFieldRect = new Rect(searchRect)
                {
                    width = Mathf.Max(0f, searchRect.width - SearchGap),
                };

                if (cache.AsyncSearchItems.Started && !cache.AsyncSearchItems.Finished)
                {
                    Rect loadingRect = new Rect(searchFieldRect)
                    {
                        x = searchFieldRect.xMax - 14f,
                        width = 12f,
                    };
                    cache.Loading.Draw(loadingRect);
                    searchFieldRect.xMax -= 16f;
                }

                GUI.SetNextControlName(searchControlName);
                cache.AsyncSearchItems.SearchText =
                    EditorGUI.TextField(searchFieldRect, GUIContent.none, cache.AsyncSearchItems.SearchText);

                if (oldSearch != cache.AsyncSearchItems.SearchText)
                {
                    RestartSearch(cache, context.WrapProp, cache.AsyncSearchItems.SearchText, true);
                }

                if (Event.current.type == EventType.KeyDown
                    && Event.current.keyCode == KeyCode.Return
                    && GUI.GetNameOfFocusedControl() == searchControlName
                    && !cache.AsyncSearchItems.Started
                    && cache.AsyncSearchItems.SourceGenerator != null
                    && cache.AsyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                {
                    cache.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1d;
                }

                if (string.IsNullOrEmpty(cache.AsyncSearchItems.SearchText))
                {
                    EditorGUI.LabelField(new Rect(searchRect)
                    {
                        width = Mathf.Max(0f, searchRect.width - 6f),
                    }, "Search", new GUIStyle("label")
                    {
                        alignment = TextAnchor.MiddleRight,
                        normal = { textColor = Color.gray },
                        fontStyle = FontStyle.Italic,
                    });
                }
            }

            if (hasPaging)
            {
                Rect numberOfItemsPerPageRect = new Rect(pagingRect)
                {
                    width = PagerInputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newNumberOfItemsPerPage = EditorGUI.DelayedIntField(numberOfItemsPerPageRect,
                        GUIContent.none, cache.AsyncSearchItems.NumberOfItemsPerPage);
                    if (changed.changed)
                    {
                        cache.AsyncSearchItems.NumberOfItemsPerPage = Mathf.Max(newNumberOfItemsPerPage, 0);
                        cache.AsyncSearchItems.PageIndex = 0;
                        RefreshVisibleIndexes(cache);
                    }
                }

                Rect numberOfItemsSepRect = new Rect(numberOfItemsPerPageRect)
                {
                    x = numberOfItemsPerPageRect.xMax,
                    width = PagerSepWidth,
                };
                EditorGUI.LabelField(numberOfItemsSepRect, "/");

                Rect numberOfItemsTotalRect = new Rect(numberOfItemsSepRect)
                {
                    x = numberOfItemsSepRect.xMax,
                    width = PagerItemsLabelWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newCount = EditorGUI.DelayedIntField(numberOfItemsTotalRect, GUIContent.none,
                        context.WrapProp.arraySize);
                    if (changed.changed)
                    {
                        SetArraySize(cache, context.WrapProp, Mathf.Max(newCount, 0));
                        ApplyAndTrigger(context.RootProperty, context.Info, context.Parent);
                        return;
                    }
                }
                EditorGUI.LabelField(numberOfItemsTotalRect, "Items", new GUIStyle("label")
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = Color.gray },
                    fontStyle = FontStyle.Italic,
                });

                Rect prePageRect = new Rect(numberOfItemsTotalRect)
                {
                    x = numberOfItemsTotalRect.xMax,
                    width = PagerButtonWidth,
                };
                using (new EditorGUI.DisabledScope(cache.AsyncSearchItems.PageIndex <= 0))
                {
                    if (GUI.Button(prePageRect, _iconLeft, EditorStyles.miniButtonLeft))
                    {
                        cache.AsyncSearchItems.PageIndex = Mathf.Max(0, cache.AsyncSearchItems.PageIndex - 1);
                        RefreshVisibleIndexes(cache);
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
                        RefreshVisibleIndexes(cache);
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
                using (new EditorGUI.DisabledScope(cache.AsyncSearchItems.PageIndex >=
                                                   cache.AsyncSearchItems.TotalPage - 1))
                {
                    if (GUI.Button(nextPageRect, _iconRight, EditorStyles.miniButtonRight))
                    {
                        cache.AsyncSearchItems.PageIndex =
                            Mathf.Min(cache.AsyncSearchItems.PageIndex + 1, cache.AsyncSearchItems.TotalPage - 1);
                        RefreshVisibleIndexes(cache);
                    }
                }
            }
        }

        private static void EnsureHeaderIcons()
        {
            _iconLeft ??= Util.LoadResource<Texture2D>("classic-dropdown-left.png");
            _iconRight ??= Util.LoadResource<Texture2D>("classic-dropdown-right.png");
        }

        private static void EnsureSearchState(InfoIMGUI cache, SerializedProperty wrapProp,
            SaintsHashSetAttribute saintsHashSetAttribute)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            int numberOfItemsPerPage = saintsHashSetAttribute.NumberOfItemsPerPage;
            if (asyncSearchItems.Size == 0 && asyncSearchItems.HitTargetIndexes.Count == 0 &&
                asyncSearchItems.CachedHitTargetIndexes.Count == 0 && wrapProp.arraySize >= 0 &&
                string.IsNullOrEmpty(asyncSearchItems.SearchText))
            {
                asyncSearchItems.NumberOfItemsPerPage = numberOfItemsPerPage;
                SetFullResults(cache, wrapProp.arraySize);
                return;
            }

            if (asyncSearchItems.NumberOfItemsPerPage != numberOfItemsPerPage)
            {
                asyncSearchItems.NumberOfItemsPerPage = numberOfItemsPerPage;
                RefreshVisibleIndexes(cache);
            }

            if (asyncSearchItems.Size != wrapProp.arraySize)
            {
                RestartSearch(cache, wrapProp, asyncSearchItems.SearchText, false);
            }
        }

        private static void RestartSearch(InfoIMGUI cache, SerializedProperty wrapProp, string searchText,
            bool resetPage)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            string safeSearchText = searchText ?? "";
            if (resetPage)
            {
                asyncSearchItems.PageIndex = 0;
            }

            asyncSearchItems.Size = wrapProp.arraySize;
            asyncSearchItems.SourceGenerator?.Dispose();
            asyncSearchItems.SourceGenerator = null;

            if (string.IsNullOrEmpty(safeSearchText))
            {
                asyncSearchItems.SearchText = "";
                SetFullResults(cache, wrapProp.arraySize);
                return;
            }

            IReadOnlyList<int> currentResults = GetCurrentResults(asyncSearchItems);
            asyncSearchItems.CachedHitTargetIndexes.Clear();
            asyncSearchItems.CachedHitTargetIndexes.AddRange(currentResults);
            asyncSearchItems.HitTargetIndexes.Clear();
            asyncSearchItems.SearchText = safeSearchText;
            asyncSearchItems.Started = false;
            asyncSearchItems.Finished = false;
            asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
            asyncSearchItems.SourceGenerator = Search(wrapProp, safeSearchText).GetEnumerator();
            RefreshVisibleIndexes(cache);
        }

        private static void SetFullResults(InfoIMGUI cache, int size)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            asyncSearchItems.Started = true;
            asyncSearchItems.Finished = true;
            asyncSearchItems.Size = size;
            asyncSearchItems.SourceGenerator?.Dispose();
            asyncSearchItems.SourceGenerator = null;
            asyncSearchItems.HitTargetIndexes.Clear();
            asyncSearchItems.HitTargetIndexes.AddRange(Enumerable.Range(0, size));
            asyncSearchItems.CachedHitTargetIndexes.Clear();
            asyncSearchItems.CachedHitTargetIndexes.AddRange(asyncSearchItems.HitTargetIndexes);
            RefreshVisibleIndexes(cache);
        }

        private static void TickAsyncSearch(InfoIMGUI cache, SerializedProperty wrapProp)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!asyncSearchItems.Started && asyncSearchItems.SourceGenerator != null &&
                EditorApplication.timeSinceStartup > asyncSearchItems.DebounceSearchTime)
            {
                asyncSearchItems.Started = true;
                RefreshVisibleIndexes(cache);
            }

            if (!asyncSearchItems.Started || asyncSearchItems.Finished || asyncSearchItems.SourceGenerator == null)
            {
                return;
            }

            bool needRefresh = false;
            for (int searchTick = 0; searchTick < 50; searchTick++)
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
                asyncSearchItems.Size = wrapProp.arraySize;
                RefreshVisibleIndexes(cache);
            }
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

        private static void RefreshVisibleIndexes(InfoIMGUI cache)
        {
            UpdateVisibleIndexes(cache.AsyncSearchItems);
            cache.ElementHeights.Clear();
            cache.ReorderableList = null;
        }

        private void SetArraySize(InfoIMGUI cache, SerializedProperty wrapProp, int newSize)
        {
            if (newSize >= wrapProp.arraySize)
            {
                IncreaseArraySize(newSize, wrapProp);
            }
            else
            {
                IReadOnlyList<int> deleteIndexes = Enumerable.Range(newSize, wrapProp.arraySize - newSize)
                    .Reverse()
                    .ToList();
                DecreaseArraySize(deleteIndexes, wrapProp);
            }

            RestartSearch(cache, wrapProp, cache.AsyncSearchItems.SearchText, false);
        }

        private void ApplyAndTrigger(SerializedProperty property, FieldInfo info, object parent)
        {
            property.serializedObject.ApplyModifiedProperties();
            (string error, int _, object value) = Util.GetValue(property, info, parent);
            if (error == "")
            {
                TriggerChangedIMGUI(property, value);
            }
        }

        private static float GetContextErrorHeight(float width, string contextError) =>
            SingleLineHeight + ImGuiHelpBox.GetHeight(contextError, width, MessageType.Error);

        private void DrawContextError(Rect position, SerializedProperty property, GUIContent label, string contextError)
        {
            Rect labelRect = new Rect(position)
            {
                height = SingleLineHeight,
            };
            GUIContent useLabel = label;
            if (useLabel == null || string.IsNullOrEmpty(useLabel.text))
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

        private (string error, ElementContext context) TryBuildElementContext(SerializedProperty property,
            GUIContent label, FieldInfo info, object parent)
        {
            (string sharedError, HashSetFieldContext sharedContext) = TryGetHashSetContext(property, info, parent);
            if (sharedError != "")
            {
                return (sharedError, null);
            }

            bool insideArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;
            GetImGuiInjectAttributes(info, insideArray, UsesReferenceWrap(sharedContext.RawType),
                out bool hasSerializeReference, out IReadOnlyList<Attribute> injectedAttributes);

            WrapType valueWrapType =
                SaintsWrapUtils.EnsureWrapType(property.FindPropertyRelative("_wrapType"), sharedContext.WrapField,
                    hasSerializeReference);

            string labelText = string.IsNullOrEmpty(label?.text)
                ? GetPreferredLabel(property)
                : label.text;
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = "Value";
            }

            return ("", new ElementContext
            {
                RootProperty = property,
                WrapProp = sharedContext.WrapProp,
                WrapField = sharedContext.WrapField,
                WrapParent = sharedContext.WrapParent,
                WrapType = sharedContext.WrapType,
                ValueWrapType = valueWrapType,
                HasSerializeReference = hasSerializeReference,
                CellAttributes = ReflectCache.GetCustomAttributes<Attribute>(info)
                    .Where(each => each is not SaintsHashSetAttribute && each is not InjectAttributeBase)
                    .Concat(injectedAttributes)
                    .ToArray(),
                Info = info,
                Parent = parent,
                LabelText = labelText,
                InHorizontalLayout = InHorizontalLayout,
            });
        }

        private static void GetImGuiInjectAttributes(FieldInfo info, bool insideArray, bool defaultSerializeReference,
            out bool hasSerializeReference, out IReadOnlyList<Attribute> injectCreatedAttributes)
        {
            hasSerializeReference = defaultSerializeReference;
            List<Attribute> createdAttributes = new List<Attribute>();
            int insideArrayOffset = insideArray ? 1 : 0;

            foreach (InjectAttributeBase injectAttribute in ReflectCache.GetCustomAttributes<InjectAttributeBase>(info))
            {
                if (injectAttribute.Decorator == typeof(SerializeReference))
                {
                    hasSerializeReference = true;
                    continue;
                }

                ValueAttributeAttribute less2DepthInject = new ValueAttributeAttribute(
                    injectAttribute.Depth - 2 - insideArrayOffset, injectAttribute.Decorator,
                    injectAttribute.Parameters);

                if (less2DepthInject.Depth > 0)
                {
                    continue;
                }

                Attribute injectedAttribute = SaintsWrapUtils.CreateInjectedAttribute(less2DepthInject);
                if (injectedAttribute != null)
                {
                    createdAttributes.Add(injectedAttribute);
                }
            }

            injectCreatedAttributes = createdAttributes;
        }
    }
}
