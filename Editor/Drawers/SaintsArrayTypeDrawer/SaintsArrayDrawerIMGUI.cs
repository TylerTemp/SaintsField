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

namespace SaintsField.Editor.Drawers.SaintsArrayTypeDrawer
{
    public partial class SaintsArrayDrawer
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
            public ReorderableList ReorderableList;
            public ElementContext Context;
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
            SaintsArrayAttribute saintsArrayAttribute =
                saintsAttribute as SaintsArrayAttribute ?? new SaintsArrayAttribute(searchable: false, numberOfItemsPerPage: 0);
            if (!TryBuildElementContext(property, label, info, parent, out ElementContext context))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            InfoIMGUI cache = EnsureKey(property);
            cache.Context = context;

            EnsureSearchState(cache, context.WrapProp, saintsArrayAttribute);
            TickAsyncSearch(cache, context.WrapProp);
            EnsureReorderableList(cache, saintsArrayAttribute);

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

            EnsureReorderableList(cache, saintsArrayAttribute);
            return SaintsPropertyDrawer.SingleLineHeight +
                   (cache.ReorderableList?.GetHeight() ?? SaintsPropertyDrawer.SingleLineHeight);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SaintsArrayAttribute saintsArrayAttribute =
                saintsAttribute as SaintsArrayAttribute ?? new SaintsArrayAttribute(searchable: false, numberOfItemsPerPage: 0);
            if (!TryBuildElementContext(property, label, info, parent, out ElementContext context))
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            InfoIMGUI cache = EnsureKey(property);
            cache.Context = context;

            EnsureSearchState(cache, context.WrapProp, saintsArrayAttribute);
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

            EnsureReorderableList(cache, saintsArrayAttribute);
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

        private void EnsureReorderableList(InfoIMGUI cache, SaintsArrayAttribute saintsArrayAttribute)
        {
            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null)
            {
                return;
            }

            bool hasHeaderControls = HasHeaderControls(saintsArrayAttribute);
            float expectedHeaderHeight = hasHeaderControls
                ? SaintsPropertyDrawer.SingleLineHeight
                : 0f;

            if (cache.ReorderableList != null)
            {
                cache.ReorderableList.headerHeight = expectedHeaderHeight;
                return;
            }

            cache.ReorderableList = new ReorderableList(context.WrapProp.serializedObject, context.WrapProp, true, hasHeaderControls, true, true)
            {
                headerHeight = expectedHeaderHeight,
            };
            cache.ReorderableList.drawHeaderCallback += rect => DrawHeaderControls(rect, cache, saintsArrayAttribute);
            cache.ReorderableList.elementHeightCallback += itemIndex => DrawElementHeight(cache, itemIndex);
            cache.ReorderableList.drawElementCallback += (rect, itemIndex, _, _) => DrawElement(rect, cache, itemIndex);
            cache.ReorderableList.onAddCallback += _ =>
            {
                ElementContext currentContext = cache.Context;
                if (currentContext?.WrapProp == null)
                {
                    return;
                }

                IncreaseArraySizeImGui(currentContext.WrapProp.arraySize + 1, currentContext.WrapProp);
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

                DecreaseArraySizeImGui(new[] { removeIndex }, currentContext.WrapProp);
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
        }

        private static bool HasHeaderControls(SaintsArrayAttribute saintsArrayAttribute) =>
            saintsArrayAttribute.Searchable || saintsArrayAttribute.NumberOfItemsPerPage > 0;

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

        private void DrawHeaderControls(Rect rect, InfoIMGUI cache, SaintsArrayAttribute saintsArrayAttribute)
        {
            ElementContext context = cache.Context;
            if (context == null || context.WrapProp == null)
            {
                return;
            }

            EnsureHeaderIcons();

            bool hasSearch = saintsArrayAttribute.Searchable;
            bool hasPaging = saintsArrayAttribute.NumberOfItemsPerPage > 0;
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
                string searchControlName = $"SaintsArraySearch_{context.RootProperty.propertyPath}";
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
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
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
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
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
                        UpdateVisibleIndexes(cache.AsyncSearchItems);
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
            SaintsArrayAttribute saintsArrayAttribute)
        {
            AsyncSearchItemsIMGUI asyncSearchItems = cache.AsyncSearchItems;
            int numberOfItemsPerPage = saintsArrayAttribute.NumberOfItemsPerPage;
            if (asyncSearchItems.Size == 0 && asyncSearchItems.HitTargetIndexes.Count == 0 &&
                asyncSearchItems.CachedHitTargetIndexes.Count == 0 && wrapProp.arraySize >= 0 &&
                string.IsNullOrEmpty(asyncSearchItems.SearchText))
            {
                asyncSearchItems.NumberOfItemsPerPage = numberOfItemsPerPage;
                SetFullResults(asyncSearchItems, wrapProp.arraySize);
                return;
            }

            if (asyncSearchItems.NumberOfItemsPerPage != numberOfItemsPerPage)
            {
                asyncSearchItems.NumberOfItemsPerPage = numberOfItemsPerPage;
                UpdateVisibleIndexes(asyncSearchItems);
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
                SetFullResults(asyncSearchItems, wrapProp.arraySize);
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
            asyncSearchItems.SourceGenerator = SearchImGui(wrapProp, safeSearchText).GetEnumerator();
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
                UpdateVisibleIndexes(asyncSearchItems);
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
                UpdateVisibleIndexes(asyncSearchItems);
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

        private void SetArraySize(InfoIMGUI cache, SerializedProperty wrapProp, int newSize)
        {
            if (newSize >= wrapProp.arraySize)
            {
                IncreaseArraySizeImGui(newSize, wrapProp);
            }
            else
            {
                IReadOnlyList<int> deleteIndexes = Enumerable.Range(newSize, wrapProp.arraySize - newSize)
                    .Reverse()
                    .ToList();
                DecreaseArraySizeImGui(deleteIndexes, wrapProp);
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

        private bool TryBuildElementContext(SerializedProperty property, GUIContent label, FieldInfo info, object parent,
            out ElementContext context)
        {
            context = null;
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;

            Type rawType = insideArray ? ReflectUtils.GetElementType(info.FieldType) : info.FieldType;
            if (rawType == null)
            {
                return false;
            }

            string propNameCompact = GetPropNameImGui(rawType);
            SerializedProperty wrapProp = FindPropertyCompactImGui(property, propNameCompact);
            if (wrapProp == null)
            {
                return false;
            }

            object fieldValue = info.GetValue(parent);
            if (insideArray && fieldValue is IEnumerable enumerable)
            {
                fieldValue = enumerable.Cast<object>().ElementAt(arrayIndex);
            }

            (FieldInfo wrapField, object _) = GetTargetInfoImGui(propNameCompact, rawType, fieldValue);
            if (wrapField == null)
            {
                return false;
            }

            Type wrapType = ReflectUtils.GetElementType(wrapField.FieldType);
            if (wrapType == null)
            {
                return false;
            }

            GetImGuiInjectAttributes(info, insideArray, out bool hasSerializeReference,
                out IReadOnlyList<Attribute> injectCreatedAttributes2);
            WrapType valueWrapType =
                SaintsWrapUtils.EnsureWrapType(property.FindPropertyRelative("_wrapType"), wrapField,
                    hasSerializeReference);

            string labelText = string.IsNullOrEmpty(label?.text)
                ? GetPreferredLabel(property)
                : label.text;
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = "Value";
            }

            context = new ElementContext
            {
                RootProperty = property,
                WrapProp = wrapProp,
                WrapField = wrapField,
                WrapType = wrapType,
                ValueWrapType = valueWrapType,
                HasSerializeReference = hasSerializeReference,
                CellAttributes = ReflectCache.GetCustomAttributes<Attribute>(info)
                    .Where(each => each is not InjectAttributeBase)
                    .Concat(injectCreatedAttributes2)
                    .ToArray(),
                Info = info,
                Parent = parent,
                LabelText = labelText,
                InHorizontalLayout = InHorizontalLayout,
            };
            return true;
        }

        private static void GetImGuiInjectAttributes(FieldInfo info, bool insideArray,
            out bool hasSerializeReference, out IReadOnlyList<Attribute> injectCreatedAttributes2)
        {
            hasSerializeReference = false;
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
                    injectAttribute.Depth - 2 - insideArrayOffset, injectAttribute.Decorator, injectAttribute.Parameters);

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

            injectCreatedAttributes2 = createdAttributes;
        }

        private static (FieldInfo targetInfo, object targetParent) GetTargetInfoImGui(string propNameCompact, Type type,
            object saintsSerValue)
        {
            object keysIterTarget = saintsSerValue;
            List<object> keysParents = new List<object>(3) { saintsSerValue };
            Type keysParentType = type;
            FieldInfo keysField = null;
            foreach (string propKeysName in propNameCompact.Split('.'))
            {
                foreach (Type each in ReflectUtils.GetSelfAndBaseTypesFromType(keysParentType))
                {
                    FieldInfo field = each.GetField(propKeysName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy);
                    if (field == null)
                    {
                        continue;
                    }

                    keysField = field;
                    keysParentType = keysField.FieldType;
                    keysIterTarget = keysField.GetValue(keysIterTarget);
                    keysParents.Add(keysIterTarget);
                    break;
                }

                Debug.Assert(keysField != null, $"Failed to get key {propKeysName} from {keysIterTarget}");
            }

            int keysParentsCount = keysParents.Count;
            object keysParent = keysParentsCount >= 2 ? keysParents[keysParentsCount - 2] : keysParents[0];
            return (keysField, keysParent);
        }

        private static string GetPropNameImGui(Type rawType)
        {
            string propName = ReflectUtils.GetIWrapPropName(rawType);
            Debug.Assert(propName != null,
                $"Failed to find property name for {rawType}. Do you forget to define a `static string EditorPropertyName` (nameof(YourPropList))?");
            return propName;
        }

        private static SerializedProperty FindPropertyCompactImGui(SerializedProperty property, string propNameCompact)
        {
            SerializedProperty prop = property.FindPropertyRelative(propNameCompact);
            if (prop != null)
            {
                return prop;
            }

            SerializedProperty accProp = property;
            foreach (string propSegName in propNameCompact.Split('.'))
            {
                SerializedProperty findProp = accProp.FindPropertyRelative(propSegName) ??
                                              SerializedUtils.FindPropertyByAutoPropertyName(accProp, propSegName);
                Debug.Assert(findProp != null, $"Failed to find prop {propSegName} in {accProp.propertyPath}");
                accProp = findProp;
            }

            return accProp;
        }

        private static bool IncreaseArraySizeImGui(int newValue, SerializedProperty prop)
        {
            if (prop.arraySize == newValue)
            {
                return false;
            }

            prop.arraySize = newValue;
            return true;
        }

        private static void DecreaseArraySizeImGui(IReadOnlyList<int> indexReversed, SerializedProperty prop)
        {
            int curSize = prop.arraySize;
            foreach (int deleteIndex in indexReversed.Where(each => each < curSize))
            {
                prop.DeleteArrayElementAtIndex(deleteIndex);
            }
        }

        private static IEnumerable<int> SearchImGui(SerializedProperty wrapProp, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                for (int index = 0; index < wrapProp.arraySize; index++)
                {
                    yield return index;
                }

                yield break;
            }

            foreach (int result in SerializedUtils.SearchArrayProperty(wrapProp, searchText))
            {
                yield return result;
            }
        }
    }
}
