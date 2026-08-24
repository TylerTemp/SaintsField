using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    internal static class IMGUIEditList
    {
        private const float VerticalPadding = 1f;
        private const float SizeWidth = 48f;
        private const float MenuWidth = 18f;
        private const float ControlGap = 4f;
        private const float FooterButtonsWidth = 58f;

        private static readonly ListDrawerSettingsAttribute DefaultListDrawerSettingsAttribute =
            new ListDrawerSettingsAttribute(searchable: false, numberOfItemsPerPage: 0);

        private enum SearchParamType
        {
            TargetAndIndex,
            Target,
            Index,
        }

        private struct PagingInfo
        {
            public IReadOnlyList<int> IndexesAfterSearch;
            public List<int> IndexesCurPage;
            public int CurPageIndex;
            public int PageCount;
        }

        private sealed class AsyncSearchItems
        {
            public bool Started;
            public bool Finished;
            public IEnumerator<IReadOnlyList<int>> SourceGenerator;
            public List<int> FullSources;
            public string SearchText;
            public double DebounceSearchTime;
            public List<int> CachedFullSources;
        }

        private sealed class ListContext
        {
            public string Key;
            public string Label;
            public Type ValueType;
            public Type ElementType;
            public object RawValue;
            public List<object> Items;
            public Action<object> BeforeSet;
            public Action<object> SetterOrNull;
            public bool LabelGrayColor;
            public bool InHorizontalLayout;
            public IReadOnlyList<Attribute> AllAttributes;
            public IReadOnlyList<object> Targets;
            public IRichTextTagProvider RichTextTagProvider;
            public ReorderableList ReorderableList;
            public ListDrawerSettingsAttribute Attribute;
            public bool StateInitialized;
            public bool SearchEnabled;
            public bool PagingEnabled;
            public bool DefaultSearch = true;
            public bool ObjectNestedSearch = true;
            public bool ExtraSearch;
            public string SearchText = "";
            public int PageIndex;
            public int NumberOfItemsPerPage;
            public PagingInfo PagingInfo;
            public AsyncSearchItems AsyncSearchItems;
            public (MethodInfo methodInfo, SearchParamType paramType) ExtraSearchMethod;
            public readonly IMGUILoading Loading = new IMGUILoading();
            public Texture2D IconLeft;
            public Texture2D IconRight;
        }

        private static readonly Dictionary<string, ListContext> ListContexts = new Dictionary<string, ListContext>();

        public static (bool ok, float height) GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isEnumerable, Type elementType) = GetEnumerableElementType(valueType, value);
            if (!isEnumerable)
            {
                return (false, 0f);
            }

            ListContext context = EnsureListContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                elementType);
            return (true, GetListHeight(context));
        }

        public static bool TryOnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isEnumerable, Type elementType) = GetEnumerableElementType(valueType, value);
            if (!isEnumerable)
            {
                return false;
            }

            ListContext context = EnsureListContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                elementType);
            DrawList(position, context);
            return true;
        }

        private static (bool ok, Type elementType) GetEnumerableElementType(Type valueType, object value)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null || type == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return (false, null);
            }

            if (type.IsArray)
            {
                return (true, type.GetElementType());
            }

            if (!typeof(IEnumerable).IsAssignableFrom(type))
            {
                return (false, null);
            }

            Type elementType = ReflectUtils.GetElementType(type);
            if (elementType == type)
            {
                elementType = typeof(object);
            }

            return (true, elementType);
        }

        private static bool IsExpanded(string key) =>
            IMGUIEdit.ViewKey.ContainsKey(key) && IMGUIEdit.ViewKey[key];

        private static void SetExpanded(string key, bool expanded) => IMGUIEdit.ViewKey[key] = expanded;

        private static ListContext EnsureListContext(string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey, Type elementType)
        {
            string key = $"{foldoutViewKey}.list";
            if (!ListContexts.ContainsKey(key))
            {
                ListContexts[key] = new ListContext
                {
                    Key = key,
                };
            }

            ListContext context = ListContexts[key];
            context.Label = label;
            context.ValueType = valueType;
            context.ElementType = elementType;
            context.RawValue = value;
            context.Items = ToObjectList(value);
            context.BeforeSet = beforeSet;
            context.SetterOrNull = setterOrNull;
            context.LabelGrayColor = labelGrayColor;
            context.InHorizontalLayout = inHorizontalLayout;
            context.AllAttributes = allAttributes;
            context.Targets = targets;
            context.RichTextTagProvider = richTextTagProvider;
            context.Attribute = allAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault()
                                ?? DefaultListDrawerSettingsAttribute;
            context.ExtraSearchMethod = !string.IsNullOrEmpty(context.Attribute.ExtraSearch) && targets.Count > 0
                ? GetSearchMethodInfo(targets[0].GetType(), elementType, context.Attribute.ExtraSearch)
                : default;

            if (!context.StateInitialized)
            {
                List<int> fullList = Enumerable.Range(0, context.Items.Count).ToList();
                context.SearchEnabled = context.Attribute.Searchable;
                context.PagingEnabled = context.Attribute.NumberOfItemsPerPage > 0;
                context.ExtraSearch = context.ExtraSearchMethod.methodInfo != null;
                context.NumberOfItemsPerPage = context.Attribute.NumberOfItemsPerPage;
                context.PagingInfo = GetPagingInfo(0, fullList, context.NumberOfItemsPerPage);
                context.AsyncSearchItems = new AsyncSearchItems
                {
                    Started = true,
                    Finished = true,
                    SourceGenerator = Enumerable.Empty<IReadOnlyList<int>>().GetEnumerator(),
                    FullSources = fullList,
                    CachedFullSources = new List<int>(fullList),
                    SearchText = "",
                    DebounceSearchTime = double.MaxValue,
                };
                context.StateInitialized = true;
                SetExpanded(context.Key, true);
            }

            if (context.ReorderableList == null)
            {
                context.ReorderableList = CreateReorderableList(context);
            }
            else
            {
                context.ReorderableList.list = context.Items;
            }

            context.ReorderableList.draggable = setterOrNull != null;
            context.ReorderableList.displayAdd = setterOrNull != null;
            context.ReorderableList.displayRemove = setterOrNull != null;
            context.ReorderableList.headerHeight = 0f;
            context.ReorderableList.footerHeight = EditorGUIUtility.singleLineHeight;
            UpdatePage(context, context.PageIndex, context.NumberOfItemsPerPage);
            return context;
        }

        private static ReorderableList CreateReorderableList(ListContext context)
        {
            ReorderableList reorderableList = new ReorderableList(context.Items, context.ElementType, true, false,
                true, true);
            reorderableList.elementHeightCallback = index => GetListElementHeight(context, index);
            reorderableList.drawElementCallback = (rect, index, _, _) => DrawListElement(rect, index, context);
            reorderableList.drawFooterCallback = rect =>
            {
                ReorderableList.defaultBehaviours.DrawFooter(rect, reorderableList);
                DrawPagingFooter(rect, context);
            };
            reorderableList.onAddCallback = _ =>
            {
                context.Items.Add(CreateDefaultValue(context.ElementType));
                ApplyListValue(context);
            };
            reorderableList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < context.Items.Count)
                {
                    context.Items.RemoveAt(list.index);
                    ApplyListValue(context);
                }
            };
            reorderableList.onReorderCallback = _ => ApplyListValue(context);
            return reorderableList;
        }

        private static List<object> ToObjectList(object value)
        {
            if (RuntimeUtil.IsNull(value))
            {
                return new List<object>();
            }

            return ((IEnumerable)value).Cast<object>().ToList();
        }

        private static float GetListHeight(ListContext context)
        {
            TickAsyncSearch(context);
            UpdatePage(context, context.PageIndex, context.NumberOfItemsPerPage);

            if (!IsExpanded(context.Key))
            {
                return EditorGUIUtility.singleLineHeight + VerticalPadding * 2;
            }

            float searchHeight = context.SearchEnabled ? EditorGUIUtility.singleLineHeight : 0f;
            return EditorGUIUtility.singleLineHeight + searchHeight + context.ReorderableList.GetHeight()
                   + VerticalPadding * 2;
        }

        private static void DrawList(Rect position, ListContext context)
        {
            Rect contentRect = new Rect(position)
            {
                y = position.y + VerticalPadding,
                height = Mathf.Max(0f, position.height - VerticalPadding * 2),
            };

            Rect headerRect = new Rect(contentRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            DrawListHeader(headerRect, context);

            if (!IsExpanded(context.Key))
            {
                return;
            }

            TickAsyncSearch(context);
            UpdatePage(context, context.PageIndex, context.NumberOfItemsPerPage);

            Rect listRect = new Rect(contentRect)
            {
                y = headerRect.yMax,
                height = Mathf.Max(0f, contentRect.yMax - headerRect.yMax),
            };

            if (context.SearchEnabled)
            {
                (Rect searchRect, Rect afterSearchRect) =
                    RectUtils.SplitHeightRect(listRect, EditorGUIUtility.singleLineHeight);
                DrawSearchField(searchRect, context);
                listRect = afterSearchRect;
            }

            context.ReorderableList.DoList(listRect);
        }

        private static void DrawListHeader(Rect rect, ListContext context)
        {
            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - SizeWidth,
                width = SizeWidth,
            };
            Rect menuRect = new Rect(rect)
            {
                x = sizeRect.x - MenuWidth - ControlGap,
                width = MenuWidth,
            };
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, menuRect.x - rect.x - ControlGap),
            };

            bool expanded = EditorGUI.Foldout(foldoutRect, IsExpanded(context.Key),
                new GUIContent($"{context.Label}"), true);
            SetExpanded(context.Key, expanded);

            if (GUI.Button(menuRect, "...", EditorStyles.miniButton))
            {
                ShowMenu(menuRect, context);
            }

            using (new EditorGUI.DisabledScope(context.SetterOrNull == null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, context.Items.Count);
                if (changed.changed)
                {
                    ResizeList(context, Math.Max(0, newSize));
                    ApplyListValue(context);
                }
            }
        }

        private static void ShowMenu(Rect rect, ListContext context)
        {
            GenericMenu menu = new GenericMenu();
            if (context.SetterOrNull == null)
            {
                menu.AddDisabledItem(new GUIContent("Set To Null"));
            }
            else
            {
                menu.AddItem(new GUIContent("Set To Null"), false, () =>
                {
                    context.BeforeSet?.Invoke(context.RawValue);
                    context.SetterOrNull(null);
                });
            }

            menu.AddSeparator("");

            bool pagingEnabled = context.PagingEnabled;
            menu.AddItem(new GUIContent("Paging"), pagingEnabled, () =>
            {
                context.PagingEnabled = !pagingEnabled;
                context.NumberOfItemsPerPage = pagingEnabled
                    ? 0
                    : context.Attribute.NumberOfItemsPerPage > 0
                        ? context.Attribute.NumberOfItemsPerPage
                        : Mathf.Max(5, context.Items.Count / 2);
                UpdatePage(context, 0, context.NumberOfItemsPerPage);
                GUI.changed = true;
                EditorWindow.focusedWindow?.Repaint();
            });

            bool searchEnabled = context.SearchEnabled;
            menu.AddItem(new GUIContent("Search"), searchEnabled, () =>
            {
                context.SearchEnabled = !searchEnabled;
                if (searchEnabled)
                {
                    context.SearchText = "";
                }
                RefreshSearchingStatus(context);
            });

            bool hasExtraSearch = context.ExtraSearchMethod.methodInfo != null;
            if (searchEnabled && hasExtraSearch)
            {
                menu.AddItem(new GUIContent("Default Search"), context.DefaultSearch, () =>
                {
                    context.DefaultSearch = !context.DefaultSearch;
                    RefreshSearchingStatus(context);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Default Search"), context.DefaultSearch);
            }

            if (searchEnabled)
            {
                menu.AddItem(new GUIContent("Object Search"), context.ObjectNestedSearch, () =>
                {
                    context.ObjectNestedSearch = !context.ObjectNestedSearch;
                    RefreshSearchingStatus(context);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Object Search"), context.ObjectNestedSearch);
            }

            if (hasExtraSearch)
            {
                if (searchEnabled)
                {
                    menu.AddItem(new GUIContent("Extra Search"), context.ExtraSearch, () =>
                    {
                        context.ExtraSearch = !context.ExtraSearch;
                        RefreshSearchingStatus(context);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Extra Search"), context.ExtraSearch);
                }
            }

            menu.DropDown(rect);
        }

        private static void DrawSearchField(Rect rect, ListContext context)
        {
            string controlName = $"IMGUIEditListSearch_{context.Key}";
            string oldSearchText = context.SearchText;
            Rect searchFieldRect = new Rect(rect);
            if (context.AsyncSearchItems.Started && !context.AsyncSearchItems.Finished)
            {
                Rect loadingRect = new Rect(searchFieldRect)
                {
                    x = searchFieldRect.xMax - 14f,
                    width = 12f,
                };
                context.Loading.Draw(loadingRect);
                searchFieldRect.xMax -= 16f;
            }

            GUI.SetNextControlName(controlName);
            context.SearchText = EditorGUI.TextField(searchFieldRect, GUIContent.none, context.SearchText);
            if (oldSearchText != context.SearchText)
            {
                UpdatePage(context, 0, context.NumberOfItemsPerPage);
            }

            if (Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl() == controlName
                && !context.AsyncSearchItems.Started
                && context.AsyncSearchItems.SourceGenerator != null
                && context.AsyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
            {
                context.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1d;
            }

            if (string.IsNullOrEmpty(context.SearchText))
            {
                EditorGUI.LabelField(new Rect(rect)
                {
                    width = rect.width - 6f,
                }, "Search", new GUIStyle("label")
                {
                    alignment = TextAnchor.MiddleRight,
                    normal =
                    {
                        textColor = Color.gray,
                    },
                    fontStyle = FontStyle.Italic,
                });
            }
        }

        private static void DrawPagingFooter(Rect rect, ListContext context)
        {
            if (!context.PagingEnabled)
            {
                return;
            }

            const float inputWidth = 30f;
            const float itemsLabelWidth = 65f;
            const float buttonWidth = 19f;
            const float pagingLabelWidth = 30f;
            const float pagingSeparatorWidth = 8f;

            Rect pagingRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - FooterButtonsWidth),
            };
            Rect numberOfItemsPerPageRect = new Rect(pagingRect)
            {
                width = inputWidth,
            };
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newNumberOfItemsPerPage = EditorGUI.DelayedIntField(numberOfItemsPerPageRect,
                    GUIContent.none, context.NumberOfItemsPerPage);
                if (changed.changed)
                {
                    UpdatePage(context, context.PageIndex, Mathf.Max(newNumberOfItemsPerPage, 0));
                }
            }

            Rect numberOfItemsSeparatorRect = new Rect(numberOfItemsPerPageRect)
            {
                x = numberOfItemsPerPageRect.xMax,
                width = pagingSeparatorWidth,
            };
            EditorGUI.LabelField(numberOfItemsSeparatorRect, "/");

            Rect numberOfItemsTotalRect = new Rect(numberOfItemsSeparatorRect)
            {
                x = numberOfItemsSeparatorRect.xMax,
                width = itemsLabelWidth,
            };
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newCount = EditorGUI.DelayedIntField(numberOfItemsTotalRect, GUIContent.none,
                    context.Items.Count);
                if (changed.changed)
                {
                    ResizeList(context, Mathf.Max(newCount, 0));
                    ApplyListValue(context);
                    return;
                }
            }
            EditorGUI.LabelField(numberOfItemsTotalRect, "Items", new GUIStyle("label")
            {
                alignment = TextAnchor.MiddleRight,
                normal =
                {
                    textColor = Color.gray,
                },
                fontStyle = FontStyle.Italic,
            });

            Rect previousPageRect = new Rect(numberOfItemsTotalRect)
            {
                x = numberOfItemsTotalRect.xMax,
                width = buttonWidth,
            };
            using (new EditorGUI.DisabledScope(context.PagingInfo.CurPageIndex <= 0))
            {
                if (!context.IconLeft)
                {
                    context.IconLeft = Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                }
                if (GUI.Button(previousPageRect, context.IconLeft, EditorStyles.miniButtonLeft))
                {
                    UpdatePage(context, context.PagingInfo.CurPageIndex - 1, context.NumberOfItemsPerPage);
                }
            }

            Rect pageRect = new Rect(previousPageRect)
            {
                x = previousPageRect.xMax,
                width = inputWidth,
            };
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newPageIndex = EditorGUI.DelayedIntField(pageRect, GUIContent.none,
                    context.PageIndex + 1) - 1;
                if (changed.changed)
                {
                    UpdatePage(context, newPageIndex, context.NumberOfItemsPerPage);
                }
            }

            Rect totalPageRect = new Rect(pageRect)
            {
                x = pageRect.xMax,
                width = pagingLabelWidth,
            };
            EditorGUI.LabelField(totalPageRect, $"/ {context.PagingInfo.PageCount}");

            Rect nextPageRect = new Rect(totalPageRect)
            {
                x = totalPageRect.xMax,
                width = buttonWidth,
            };
            using (new EditorGUI.DisabledScope(context.PagingInfo.CurPageIndex >=
                                                context.PagingInfo.PageCount - 1))
            {
                if (!context.IconRight)
                {
                    context.IconRight = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                }
                if (GUI.Button(nextPageRect, context.IconRight, EditorStyles.miniButtonRight))
                {
                    UpdatePage(context, context.PagingInfo.CurPageIndex + 1,
                        context.NumberOfItemsPerPage);
                }
            }
        }

        private static void RefreshSearchingStatus(ListContext context)
        {
            if (string.IsNullOrWhiteSpace(context.SearchText))
            {
                UpdatePage(context, 0, context.NumberOfItemsPerPage);
            }
            else
            {
                context.AsyncSearchItems.DebounceSearchTime = 0d;
                context.AsyncSearchItems.Started = false;
                context.AsyncSearchItems.Finished = false;
                context.AsyncSearchItems.FullSources.Clear();
                context.AsyncSearchItems.SourceGenerator?.Dispose();
                context.AsyncSearchItems.SourceGenerator = SearchCallback(context, context.SearchText).GetEnumerator();
                context.AsyncSearchItems.SearchText = context.SearchText;
                UpdatePage(context, 0, context.NumberOfItemsPerPage);
            }

            GUI.changed = true;
            EditorWindow.focusedWindow?.Repaint();
        }

        private static void UpdatePage(ListContext context, int newPageIndex, int numberOfItemsPerPage)
        {
            List<int> resultIndexes;
            if (string.IsNullOrWhiteSpace(context.SearchText))
            {
                resultIndexes = Enumerable.Range(0, context.Items.Count).ToList();
                context.AsyncSearchItems.Started = true;
                context.AsyncSearchItems.Finished = true;
                context.AsyncSearchItems.CachedFullSources = new List<int>(resultIndexes);
                context.AsyncSearchItems.FullSources = new List<int>(resultIndexes);
                context.AsyncSearchItems.SearchText = "";
                context.AsyncSearchItems.SourceGenerator?.Dispose();
                context.AsyncSearchItems.SourceGenerator = null;
            }
            else if (context.AsyncSearchItems.SearchText == context.SearchText)
            {
                resultIndexes = !context.AsyncSearchItems.Started && !context.AsyncSearchItems.Finished
                    ? context.AsyncSearchItems.CachedFullSources
                    : context.AsyncSearchItems.FullSources;
            }
            else
            {
                context.AsyncSearchItems.SearchText = context.SearchText;
                context.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + 0.6d;
                context.AsyncSearchItems.Started = false;
                context.AsyncSearchItems.Finished = false;
                context.AsyncSearchItems.FullSources.Clear();
                context.AsyncSearchItems.SourceGenerator?.Dispose();
                context.AsyncSearchItems.SourceGenerator = SearchCallback(context, context.SearchText).GetEnumerator();
                resultIndexes = context.AsyncSearchItems.CachedFullSources;
            }

            resultIndexes = resultIndexes
                .Where(each => each >= 0 && each < context.Items.Count)
                .ToList();
            context.PagingInfo = GetPagingInfo(newPageIndex, resultIndexes, numberOfItemsPerPage);
            context.PageIndex = context.PagingInfo.CurPageIndex;
            context.NumberOfItemsPerPage = Mathf.Max(numberOfItemsPerPage, 0);
        }

        private static void TickAsyncSearch(ListContext context)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            AsyncSearchItems searchItems = context.AsyncSearchItems;
            if (!searchItems.Started && searchItems.SourceGenerator != null
                                     && EditorApplication.timeSinceStartup > searchItems.DebounceSearchTime)
            {
                searchItems.Started = true;
                UpdatePage(context, context.PageIndex, context.NumberOfItemsPerPage);
            }

            if (searchItems.Started && !searchItems.Finished && searchItems.SourceGenerator != null)
            {
                if (searchItems.SourceGenerator.MoveNext())
                {
                    IReadOnlyList<int> currentValue = searchItems.SourceGenerator.Current;
                    if (currentValue != null && currentValue.Count > 0)
                    {
                        searchItems.FullSources.AddRange(currentValue);
                        UpdatePage(context, context.PageIndex, context.NumberOfItemsPerPage);
                    }
                    EditorWindow.focusedWindow?.Repaint();
                }
                else
                {
                    searchItems.Finished = true;
                    searchItems.SourceGenerator.Dispose();
                    searchItems.SourceGenerator = null;
                }
            }
        }

        private static PagingInfo GetPagingInfo(int newPageIndex, IReadOnlyList<int> fullIndexResults,
            int numberOfItemsPerPage)
        {
            int curPageIndex;
            int pageCount;
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
                pageCount = Mathf.CeilToInt((float)fullIndexResults.Count / numberOfItemsPerPage);
                curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                skipStart = curPageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            return new PagingInfo
            {
                IndexesAfterSearch = fullIndexResults,
                IndexesCurPage = fullIndexResults.Skip(skipStart).Take(itemCount).ToList(),
                CurPageIndex = curPageIndex,
                PageCount = pageCount,
            };
        }

        private static IEnumerable<IReadOnlyList<int>> SearchCallback(ListContext context, string search)
        {
            const int batchLimit = 10;
            IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(search).ToList();

            if (context.ExtraSearch && context.ExtraSearchMethod.methodInfo != null)
            {
                List<int> batchResults = new List<int>();
                int batchCount = 0;
                for (int index = 0; index < context.Items.Count; index++)
                {
                    object item = context.Items[index];
                    object[] methodParams = context.ExtraSearchMethod.paramType switch
                    {
                        SearchParamType.Index => new object[] { index, searchTokens },
                        SearchParamType.Target => new[] { item, searchTokens },
                        _ => new[] { item, index, searchTokens },
                    };

                    bool matched = (bool)context.ExtraSearchMethod.methodInfo.Invoke(context.Targets[0], methodParams);
                    if (!matched)
                    {
                        matched = MatchesDefaultSearch(item, searchTokens, context.ObjectNestedSearch);
                    }
                    if (matched)
                    {
                        batchResults.Add(index);
                    }

                    batchCount++;
                    if (batchCount / batchLimit >= 1)
                    {
                        yield return batchResults.ToArray();
                        batchCount = 0;
                        batchResults.Clear();
                    }
                }

                if (batchResults.Count > 0)
                {
                    yield return batchResults;
                }
                yield break;
            }

            if (!context.DefaultSearch)
            {
                yield break;
            }

            List<int> defaultBatchResults = new List<int>();
            int defaultBatchCount = 0;
            foreach (int index in Util.SearchArrayObjects(context.Items, search, context.ObjectNestedSearch))
            {
                if (index != -1)
                {
                    defaultBatchResults.Add(index);
                }

                defaultBatchCount++;
                if (defaultBatchCount / batchLimit >= 1)
                {
                    yield return defaultBatchResults.ToArray();
                    defaultBatchCount = 0;
                    defaultBatchResults.Clear();
                }
            }

            if (defaultBatchResults.Count > 0)
            {
                yield return defaultBatchResults;
            }
        }

        private static bool MatchesDefaultSearch(object item, IReadOnlyList<ListSearchToken> searchTokens,
            bool objectNestedSearch)
        {
            foreach (ListSearchToken token in searchTokens)
            {
                if (!Util.SearchObject(item, token.Token, new HashSet<object>(), objectNestedSearch))
                {
                    return false;
                }
            }
            return true;
        }

        private static (MethodInfo methodInfo, SearchParamType paramType) GetSearchMethodInfo(Type targetType,
            Type elementType, string methodName)
        {
            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(targetType))
            {
                foreach (MethodInfo methodInfo in eachType.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (methodInfo.Name != methodName || methodInfo.ReturnParameter?.ParameterType != typeof(bool))
                    {
                        continue;
                    }

                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    if (methodParams.Length == 0
                        || !typeof(IEnumerable<ListSearchToken>).IsAssignableFrom(
                            methodParams[methodParams.Length - 1].ParameterType))
                    {
                        continue;
                    }

                    if (methodParams.Length == 3
                        && elementType.IsAssignableFrom(methodParams[0].ParameterType)
                        && typeof(int).IsAssignableFrom(methodParams[1].ParameterType))
                    {
                        return (methodInfo, SearchParamType.TargetAndIndex);
                    }

                    if (methodParams.Length == 2 && elementType.IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, SearchParamType.Target);
                    }

                    if (methodParams.Length == 2 && typeof(int).IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, SearchParamType.Index);
                    }
                }
            }

            return (null, default);
        }

        private static void ResizeList(ListContext context, int newSize)
        {
            while (context.Items.Count < newSize)
            {
                context.Items.Add(CreateDefaultValue(context.ElementType));
            }

            while (context.Items.Count > newSize)
            {
                context.Items.RemoveAt(context.Items.Count - 1);
            }
        }

        private static float GetListElementHeight(ListContext context, int index)
        {
            if (index < 0 || index >= context.Items.Count
                          || !context.PagingInfo.IndexesCurPage.Contains(index))
            {
                return 0f;
            }

            return IMGUIEdit.GetPropertyHeight(
                $"Element {index}",
                context.ElementType,
                context.Items[index],
                null,
                context.SetterOrNull == null ? null : newValue =>
                {
                    context.Items[index] = newValue;
                    ApplyListValue(context);
                },
                context.LabelGrayColor,
                context.InHorizontalLayout,
                context.AllAttributes,
                context.Targets,
                context.RichTextTagProvider,
                $"{context.Key}.[{index}]") + 2f;
        }

        private static void DrawListElement(Rect rect, int index, ListContext context)
        {
            if (index < 0 || index >= context.Items.Count
                          || !context.PagingInfo.IndexesCurPage.Contains(index)
                          || rect.height <= 0f)
            {
                return;
            }

            Rect useRect = new Rect(rect)
            {
                y = rect.y + 1f,
                height = Mathf.Max(0f, rect.height - 2f),
            };
            IMGUIEdit.OnGUI(
                useRect,
                $"Element {index}",
                context.ElementType,
                context.Items[index],
                null,
                context.SetterOrNull == null ? null : newValue =>
                {
                    context.Items[index] = newValue;
                    ApplyListValue(context);
                },
                context.LabelGrayColor,
                context.InHorizontalLayout,
                context.AllAttributes,
                context.Targets,
                context.RichTextTagProvider,
                $"{context.Key}.[{index}]");
        }

        private static void ApplyListValue(ListContext context)
        {
            if (context.SetterOrNull == null)
            {
                return;
            }

            object newValue = MakeCollectionValue(context.ValueType, context.ElementType, context.RawValue,
                context.Items);
            context.BeforeSet?.Invoke(context.RawValue);
            context.SetterOrNull(newValue);
            context.RawValue = newValue;
            RefreshSearchingStatus(context);
        }

        private static object MakeCollectionValue(Type valueType, Type elementType, object rawValue,
            List<object> items)
        {
            if (valueType?.IsArray == true)
            {
                Array array = Array.CreateInstance(elementType, items.Count);
                for (int index = 0; index < items.Count; index++)
                {
                    array.SetValue(items[index], index);
                }

                return array;
            }

            if (rawValue is IList existingList && !existingList.IsReadOnly && !existingList.IsFixedSize)
            {
                existingList.Clear();
                foreach (object item in items)
                {
                    existingList.Add(item);
                }

                return existingList;
            }

            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList list = (IList)Activator.CreateInstance(listType);
            foreach (object item in items)
            {
                list.Add(item);
            }

            if (valueType == null || valueType.IsAssignableFrom(listType) || valueType.IsInterface)
            {
                return list;
            }

            if (typeof(IList).IsAssignableFrom(valueType) && valueType.GetConstructor(Type.EmptyTypes) != null)
            {
                IList concrete = (IList)Activator.CreateInstance(valueType);
                foreach (object item in items)
                {
                    concrete.Add(item);
                }

                return concrete;
            }

            return list;
        }

        private static object CreateDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }

            if (type == typeof(Guid))
            {
                return Guid.NewGuid();
            }

            if (type?.IsEnum == true)
            {
                Array values = Enum.GetValues(type);
                return values.Length == 0 ? Activator.CreateInstance(type) : values.GetValue(0);
            }

            return type?.IsValueType == true ? Activator.CreateInstance(type) : null;
        }
    }
}
