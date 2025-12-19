using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using Saintsfield.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer
    {
        private class ListViewPayload
        {
            public List<object> RawValues;
            // public List<int> ItemIndexToOriginIndex;
            public object RawListValue;
            public AsyncSearchItems AsyncSearchItems;
        }

        public class ListViewWrapper : VisualElement
        {
            public readonly Foldout Foldout;
            public readonly IntegerField ArraySizeField;
            public readonly Button NullButton;
            public readonly ListView ListView;
            public readonly SearchPager SearchPager;

            public ListViewWrapper(string label, bool nullable, ListView listView)
            {
                VisualElement header = new VisualElement();
                Add(header);

                // Debug.Log(label);

                header.Add(Foldout = new Foldout
                {
                    text = label,
                });
                VisualElement foldoutContent = Foldout.contentContainer;
                foldoutContent.style.marginLeft = 0;

                VisualElement rightAlign = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignSelf = Align.FlexEnd,
                        marginTop = -16,
                    },
                };
                header.Add(rightAlign);

                rightAlign.Add(NullButton = new Button
                {
                    tooltip = "Set to Null",
                    // text = "x",
                    style =
                    {
                        display = nullable? DisplayStyle.Flex: DisplayStyle.None,
                        width = EditorGUIUtility.singleLineHeight,
                        // height = EditorGUIUtility.singleLineHeight,
                        borderBottomRightRadius = 0,
                        borderTopRightRadius = 0,
                        borderRightWidth = 0,
                        marginRight = 0,

                        backgroundImage = Util.LoadResource<Texture2D>("close.png"),
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
#else
                        unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
                });
                rightAlign.Add(ArraySizeField = new IntegerField
                {
                    isDelayed = true,
                    style =
                    {
                        width = 50,
                        marginLeft = 0,
                    },
                });

                VisualElement textInputElement = ArraySizeField.Q<VisualElement>(name: "unity-text-input");
                if (textInputElement != null)
                {
                    textInputElement.style.borderTopLeftRadius = textInputElement.style.borderTopRightRadius = 0;
                    textInputElement.style.marginLeft = 0;
                }

                Add(SearchPager = new SearchPager());

                Add(ListView = listView);

                RefreshToggleDisplay();
                Foldout.RegisterValueChangedCallback(_ => RefreshToggleDisplay());
            }

            private void RefreshToggleDisplay()
            {
                ListView.style.display = Foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public static ListViewWrapper UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object rawListValue,
            object[] listValue, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor,
            bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
            Debug.Log($"render list start {listValue.Length}/{label}/{valueType}");
#endif
            if (oldElement is ListViewWrapper listViewWrapper)
            {
                ListViewPayload oldPayload = (ListViewPayload)listViewWrapper.ListView.userData;
                oldPayload.RawValues = listValue.ToList();
                oldPayload.RawListValue = rawListValue;

                // Debug.Log($"Refresh count={listValue.Length}");
                oldPayload.AsyncSearchItems.ItemIndexToPropertyIndex = oldPayload.RawValues.Select((_, index) => index).ToList();
                listViewWrapper.ListView.itemsSource = oldPayload.AsyncSearchItems.ItemIndexToPropertyIndex.ToList();
                listViewWrapper.ArraySizeField.SetValueWithoutNotify(oldPayload.RawValues.Count);
                listViewWrapper.SearchPager.NumberOfItemsTotalField.SetValueWithoutNotify(oldPayload.RawValues.Count);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"ItemIndexToOriginIndex={string.Join(",", oldPayload.ItemIndexToOriginIndex)}");
#endif

                return null;
            }

            ListDrawerSettingsAttribute listDrawerSettingsAttribute =
                allAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault() ?? new ListDrawerSettingsAttribute(searchable: false, numberOfItemsPerPage: 0);

            Type elementType = null;
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(valueType))
            {
                Type tryGetElementType = ReflectUtils.GetElementType(eachType);
                // Debug.Log($"{eachType}({eachType.IsGenericType}) -> {tryGetElementType}");
                // ReSharper disable once InvertIf
                if (tryGetElementType != eachType)
                {
                    elementType = tryGetElementType;
                    break;
                }
            }

            if (elementType == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError($"Failed to find element type in {valueType}");
#endif
                elementType = typeof(object);
            }

            List<int> originIndices = Enumerable.Range(0, listValue.Length).ToList();
            ListViewPayload payload = new ListViewPayload
            {
                RawValues = listValue.ToList(),
                // ItemIndexToOriginIndex = new List<int>(originIndices),
                RawListValue = rawListValue,
                // ElementType = elementType,
                AsyncSearchItems = new AsyncSearchItems
                {
                    Started = true,
                    Finished = true,
                    SourceGenerator = Enumerable.Empty<IReadOnlyList<int>>().GetEnumerator(),
                    FullSources = new List<int>(originIndices),
                    CachedFullSources = new List<int>(originIndices),
                    SearchText = "",
                    DebounceSearchTime = double.MaxValue,
                    ItemIndexToPropertyIndex = new List<int>(originIndices),
                },
            };

            #region Search Callback

            (MethodInfo methodInfo, ParamType paramType) extraSearchMethod = default;
            (MethodInfo methodInfo, ParamType paramType) overrideSearchMethod = default;

            if (!string.IsNullOrEmpty(listDrawerSettingsAttribute.ExtraSearch))
            {
                extraSearchMethod = GetSearchMethodInfo(targets[0].GetType(), elementType, listDrawerSettingsAttribute.ExtraSearch);
            }

            if (!string.IsNullOrEmpty(listDrawerSettingsAttribute.OverrideSearch))
            {
                overrideSearchMethod = GetSearchMethodInfo(targets[0].GetType(), elementType, listDrawerSettingsAttribute.OverrideSearch);
            }

            IEnumerable<IReadOnlyList<int>> SearchCallback(List<object> values, string search)
            {
                const int batchLimit = 10;

                IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(search).ToList();

                if (overrideSearchMethod.methodInfo != null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# use override search method");
#endif
                    if (overrideSearchMethod.paramType == ParamType.Index)
                    {
                        List<int> batchResults = new List<int>();
                        int batchCount = 0;
                        foreach (int fullIndex in Enumerable.Range(0, values.Count))
                        {
                            if ((bool)overrideSearchMethod.methodInfo.Invoke(targets[0],
                                    new object[] { fullIndex, searchTokens }))
                            {
                                // yield return fullIndex;
                                batchResults.Add(fullIndex);
                            }

                            batchCount++;

                            // ReSharper disable once InvertIf
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

                    {
                        int curIndex = 0;

                        List<int> batchResults = new List<int>();
                        int batchCount = 0;

                        foreach (object rawValue in values)
                        {
                            object[] methodParams = overrideSearchMethod.paramType == ParamType.Target
                                ? new[] { rawValue, searchTokens }
                                : new[] { rawValue, curIndex, searchTokens };

                            if ((bool)overrideSearchMethod.methodInfo.Invoke(targets[0], methodParams))
                            {
                                batchResults.Add(curIndex);
                            }

                            curIndex++;

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
                }

                if (extraSearchMethod.methodInfo != null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# use extra search method");
#endif
                    if (extraSearchMethod.paramType == ParamType.Index)
                    {
                        List<int> batchResults = new List<int>();
                        int batchCount = 0;

                        foreach (int fullIndex in Enumerable.Range(0, values.Count))
                        {
                            if ((bool)extraSearchMethod.methodInfo.Invoke(targets[0],
                                    new object[] { fullIndex, searchTokens }))
                            {
                                // yield return fullIndex;
                                batchResults.Add(fullIndex);
                            }
                            else
                            {
                                var item = values[fullIndex];
                                HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                                    .Select(_ => new HashSet<object>())
                                    .ToArray();
                                bool all = true;
                                for (int index = 0; index < searchTokens.Count; index++)
                                {
                                    ListSearchToken token = searchTokens[index];
                                    HashSet<object> searchedObject = searchedObjectsArray[index];
                                    // ReSharper disable once InvertIf
                                    if (!Util.SearchObject(item, token.Token, searchedObject))
                                    {
                                        all = false;
                                        break;
                                    }
                                }

                                if (all)
                                {
                                    // yield return fullIndex;
                                    batchResults.Add(fullIndex);
                                }
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

                    {
                        int curIndex = 0;

                        List<int> batchResults = new List<int>();
                        int batchCount = 0;
                        foreach (object rawValue in values)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"#Search# pass rawValue {rawValue}/{curIndex}");
#endif
                            object[] methodParams = extraSearchMethod.paramType == ParamType.Target
                                ? new[] { rawValue, searchTokens }
                                : new[] { rawValue, curIndex, searchTokens };

                            if ((bool)extraSearchMethod.methodInfo.Invoke(targets[0], methodParams))
                            {
                                // Debug.Log($"yield {curIndex}/{rawValue} in extra search");
                                // yield return curIndex;
                                batchResults.Add(curIndex);
                            }
                            else
                            {
                                object itemProp = values[curIndex];
                                HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                                    .Select(_ => new HashSet<object>())
                                    .ToArray();

                                bool all = true;
                                for (int index = 0; index < searchTokens.Count; index++)
                                {
                                    ListSearchToken token = searchTokens[index];
                                    HashSet<object> searchedObjects = searchedObjectsArray[index];
                                    if (!Util.SearchObject(itemProp, token.Token, searchedObjects))
                                    {
                                        all = false;
                                        break;
                                    }
                                }

                                if (all)
                                {
                                    // yield return curIndex;
                                    batchResults.Add(curIndex);
                                }
                            }

                            curIndex++;

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
                }

                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# use default search method");
#endif
                    foreach (IReadOnlyList<int> batch in DefaultSearchCallbackWithObject(values, search))
                    {
                        yield return batch;
                    }
                }
            }

            #endregion

            listViewWrapper = new ListViewWrapper(label, setterOrNull != null, new ListView
            {
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                headerTitle = label,
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
                itemsSource = listValue.Select((_, index) => index).ToList(),
                makeItem = () => new VisualElement(),

                userData = payload,
            });
            if (labelGrayColor)
            {
                listViewWrapper.Foldout.style.color = EColor.EditorSeparator.GetColor();
            }

            // null button
            if (setterOrNull != null)
            {
                // nullable
                listViewWrapper.NullButton.style.display = DisplayStyle.Flex;
                listViewWrapper.NullButton.clicked += () =>
                {
                    beforeSet?.Invoke(rawListValue);
                    setterOrNull(null);
                };
            }

            // Size & Page Items Total
            listViewWrapper.ArraySizeField.SetValueWithoutNotify(payload.RawValues.Count);
            listViewWrapper.SearchPager.NumberOfItemsTotalField.SetValueWithoutNotify(payload.RawValues.Count);
            listViewWrapper.ArraySizeField.RegisterValueChangedCallback(OnSizeInput);
            listViewWrapper.SearchPager.NumberOfItemsTotalField.RegisterValueChangedCallback(OnSizeInput);

            // Search
            listViewWrapper.SearchPager.ToolbarSearchField.RegisterValueChangedCallback(_ =>
                UpdatePage(0, listViewWrapper.SearchPager.NumberOfItemsTotalField.value));
            listViewWrapper.SearchPager.ToolbarSearchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                // ReSharper disable once InvertIf
                if (evt.keyCode == KeyCode.Return)
                {
                    if (!payload.AsyncSearchItems.Started && payload.AsyncSearchItems.SourceGenerator != null &&
                        payload.AsyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                    {
                        payload.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1;
                    }
                }
            }, TrickleDown.TrickleDown);

            // Page Items
            listViewWrapper.SearchPager.NumberOfItemsPerPageField.SetValueWithoutNotify(listDrawerSettingsAttribute.NumberOfItemsPerPage);
            void UpdateNumberOfItemsPerPage(int newValue)
            {
                int newValueClamp = Mathf.Max(newValue, 0);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"update number of items per page {newValueClamp}");
#endif
                UpdatePage(payload.AsyncSearchItems.CurPageIndex, newValueClamp);
            }
            listViewWrapper.SearchPager.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            // Pre Button
            listViewWrapper.SearchPager.PagePreButton.clicked += () =>
                UpdatePage(payload.AsyncSearchItems.CurPageIndex - 1,
                    listViewWrapper.SearchPager.NumberOfItemsPerPageField.value);
            // Next Button
            listViewWrapper.SearchPager.PageNextButton.clicked += () =>
                UpdatePage(payload.AsyncSearchItems.CurPageIndex + 1,
                    listViewWrapper.SearchPager.NumberOfItemsPerPageField.value);
            // Input Page Number
            listViewWrapper.SearchPager.PageField.RegisterValueChangedCallback(evt =>
                UpdatePage(evt.newValue - 1, listViewWrapper.SearchPager.NumberOfItemsPerPageField.value));

            void UpdatePage(int newPageIndex, int numberOfItemsPerPage)
            {
                string searchText = listViewWrapper.SearchPager.ToolbarSearchField.value;
                List<int> resultIndexes;
                if (string.IsNullOrWhiteSpace(searchText))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Empty search");
#endif
                    resultIndexes = Enumerable.Range(0, payload.RawValues.Count).ToList();
                    payload.AsyncSearchItems.Started = true;
                    payload.AsyncSearchItems.Finished = true;
                    payload.AsyncSearchItems.CachedFullSources = new List<int>(resultIndexes);
                    payload.AsyncSearchItems.FullSources = new List<int>(resultIndexes);
                    payload.AsyncSearchItems.SearchText = "";
                    payload.AsyncSearchItems.SourceGenerator = null;
                }
                else if (payload.AsyncSearchItems.SearchText == searchText)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Cached search {_asyncSearchItems.SearchText}, started={_asyncSearchItems.Started}, finished={_asyncSearchItems.Finished}");
#endif
                    resultIndexes = payload.AsyncSearchItems.FullSources;
                }
                else
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# New search {searchText}");
#endif
                    payload.AsyncSearchItems.SearchText = searchText;
                    payload.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + 0.6f;
                    payload.AsyncSearchItems.Started = false;
                    payload.AsyncSearchItems.Finished = false;
                    payload.AsyncSearchItems.FullSources.Clear();
                    if (payload.AsyncSearchItems.SourceGenerator != null)
                    {
                        payload.AsyncSearchItems.SourceGenerator.Dispose();
                        payload.AsyncSearchItems.SourceGenerator = null;
                    }
                    payload.AsyncSearchItems.SourceGenerator = SearchCallback(payload.RawValues, searchText).GetEnumerator();

                    resultIndexes = payload.AsyncSearchItems.CachedFullSources;
                }

                PagingInfo pagingInfo = GetPagingInfo(newPageIndex, resultIndexes, numberOfItemsPerPage);

                // Debug.Log($"index search={searchText} result: {string.Join(",", pagingInfo.IndexesAfterSearch)}; numberOfItemsPerPage={numberOfItemsPerPage}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"index search={searchField.value} result: {string.Join(",", pagingInfo.IndexesAfterSearch)}; numberOfItemsPerPage={numberOfItemsPerPage}");
#endif

                listViewWrapper.SearchPager.PagePreButton.SetEnabled(pagingInfo.CurPageIndex > 0);
                listViewWrapper.SearchPager.PageNextButton.SetEnabled(pagingInfo.CurPageIndex < pagingInfo.PageCount - 1);

                payload.AsyncSearchItems.ItemIndexToPropertyIndex.Clear();
                payload.AsyncSearchItems.ItemIndexToPropertyIndex.AddRange(pagingInfo.IndexesCurPage);

                payload.AsyncSearchItems.CurPageIndex = pagingInfo.CurPageIndex;

                listViewWrapper.SearchPager.PageLabel.text = $" / {pagingInfo.PageCount}";
                listViewWrapper.SearchPager.PageField.SetValueWithoutNotify(payload.AsyncSearchItems.CurPageIndex + 1);

                List<int> curPageItems = pagingInfo.IndexesCurPage;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"set items: {string.Join(", ", curPageItems)}, itemIndexToPropertyIndex={string.Join(",", itemIndexToPropertyIndex)}");
#endif
                if(!listViewWrapper.ListView.itemsSource.Cast<int>().SequenceEqual(curPageItems))
                {
                    listViewWrapper.ListView.itemsSource = curPageItems;
                    listViewWrapper.ListView.Rebuild();
                }
            }

            void BindItem(VisualElement visualElement, int index)
            {
                // int actualIndex = (int)listView.itemsSource[index];
                // Debug.Log($"{index} -> {actualIndex}");
                // Debug.Log($"index={index}, ItemIndexToOriginIndex={string.Join(",", payload.ItemIndexToOriginIndex)}");

                VisualElement firstChild = visualElement.Children().FirstOrDefault();

                int actualIndex = payload.AsyncSearchItems.ItemIndexToPropertyIndex[index];
                object actualValue = payload.RawValues[actualIndex];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                    Debug.Log($"list index={index}, elementType={elementType}, actualValue={actualValue}, rawValues={string.Join(",", payload.RawValues)}");
#endif
                VisualElement item = UIToolkitEdit.UIToolkitValueEdit(
                    firstChild,
                    $"Element {actualIndex}",
                    elementType,
                    actualValue,
                    null,
                    newItemValue =>
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                            Debug.Log($"List {actualIndex} set newValue {newItemValue}");
#endif
                        IList rawListValueArray = (IList)payload.RawListValue;
                        rawListValueArray[actualIndex] = newItemValue;
                        payload.RawValues[actualIndex] = newItemValue;
                        setterOrNull?.Invoke(rawListValueArray);
                    },
                    false,
                    inHorizontalLayout,
                    allAttributes,
                    targets,
                    richTextTagProvider).result;
                if (item != null)
                {
                    visualElement.Clear();
                    visualElement.Add(item);
                }
            }

            listViewWrapper.ListView.bindItem = BindItem;

            Button listViewAddButton = listViewWrapper.ListView.Q<Button>("unity-list-view__add-button");

            if (listViewAddButton != null)
            {
                listViewAddButton.clickable = new Clickable(() => AddCount(1));
            }

            listViewWrapper.ListView.itemsRemoved += objects =>
            {
                List<int> removeIndexInRaw = objects
                    .Select(removeIndex => payload.AsyncSearchItems.ItemIndexToPropertyIndex[removeIndex])
                    .OrderByDescending(each => each)
                    .ToList();
                RemoveIndicesBackwards(removeIndexInRaw);
            };

            listViewWrapper.ListView.itemIndexChanged += (first, second) =>
            {
                int fromPropIndex = payload.AsyncSearchItems.ItemIndexToPropertyIndex[first];
                int toPropIndex = payload.AsyncSearchItems.ItemIndexToPropertyIndex[second];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                    Debug.Log($"drag {fromPropIndex}({first}) -> {toPropIndex}({second}); ItemIndexToOriginIndex={string.Join(",", payload.ItemIndexToOriginIndex)}");
#endif

                IList lis = (IList)payload.RawListValue;
                MoveArrayElement(lis, fromPropIndex, toPropIndex);
            };

            listViewWrapper.schedule.Execute(() =>
            {
                if(!payload.AsyncSearchItems.Started && EditorApplication.timeSinceStartup > payload.AsyncSearchItems.DebounceSearchTime)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Debounce reached, start {_asyncSearchItems.SearchText}");
#endif
                    payload.AsyncSearchItems.Started = true;
                    Debug.Assert(payload.AsyncSearchItems.SourceGenerator != null);
                }

                if (payload.AsyncSearchItems.Started && !payload.AsyncSearchItems.Finished)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Continue search {_asyncSearchItems.SearchText}");
#endif
                    if (listViewWrapper.SearchPager.LoadingImage.style.visibility != Visibility.Visible)
                    {
                        listViewWrapper.SearchPager.LoadingImage.style.visibility = Visibility.Visible;
                        UpdatePage(payload.AsyncSearchItems.CurPageIndex, listViewWrapper.SearchPager.NumberOfItemsTotalField.value);
                    }

                    if (payload.AsyncSearchItems.SourceGenerator.MoveNext())
                    {
                        IReadOnlyList<int> currentValue = payload.AsyncSearchItems.SourceGenerator.Current;

                        // ReSharper disable once MergeIntoPattern
                        if(currentValue != null && currentValue.Count > 0)
                        {
                            payload.AsyncSearchItems.FullSources.AddRange(currentValue);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"#Search# add search results {string.Join(", ", currentValue)}");
#endif
                            UpdatePage(payload.AsyncSearchItems.CurPageIndex, listViewWrapper.SearchPager.NumberOfItemsPerPageField.value);
                        }
                    }
                    else
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                        Debug.Log($"#Search# end search {_asyncSearchItems.SearchText}");
#endif
                        payload.AsyncSearchItems.Finished = true;
                        payload.AsyncSearchItems.SourceGenerator.Dispose();
                        payload.AsyncSearchItems.SourceGenerator = null;
                    }
                }

                if (payload.AsyncSearchItems.Finished && listViewWrapper.SearchPager.LoadingImage.style.visibility != Visibility.Hidden)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# disable loader image {_asyncSearchItems.SearchText}");
#endif
                    listViewWrapper.SearchPager.LoadingImage.style.visibility = Visibility.Hidden;
                }
            }).Every(1);

            UpdatePage(0, listViewWrapper.SearchPager.NumberOfItemsPerPageField.value);

            bool noSearch = !listDrawerSettingsAttribute.Searchable;
            if (noSearch)
            {
                listViewWrapper.SearchPager.SearchContainer.style.visibility = Visibility.Hidden;
                // listViewWrapper.SearchPager.SearchContainer.style.display = DisplayStyle.None;
                // listViewWrapper.SearchPager.PagingContainer.style.flexGrow = 1;
            }

            bool noPaging = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0;
            if (noPaging)
            {
                listViewWrapper.SearchPager.PagingContainer.style.display = DisplayStyle.None;
            }

            if (noSearch && noPaging)
            {
                listViewWrapper.SearchPager.style.display = DisplayStyle.None;
            }

            return listViewWrapper;

            void OnSizeInput(ChangeEvent<int> evt)
            {
                int newCount = evt.newValue;
                int oldCount = payload.RawValues.Count;

                int delta = newCount - oldCount;
                switch (delta)
                {
                    case 0:
                        return;
                    case > 0:
                        AddCount(delta);
                        return;
                    case < 0:
                        RemoveIndicesBackwards(Enumerable.Range(newCount, -delta).Reverse().ToArray());
                        return;
                }
            }

            void RemoveIndicesBackwards(IReadOnlyList<int> backwardIndices)
            {
                if (valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                {
                    beforeSet?.Invoke(rawListValue);
                    Array newArray =
                        Array.CreateInstance(elementType, payload.RawValues.Count - backwardIndices.Count);
                    Array rawArray = (Array)payload.RawListValue;
                    int copyIndex = 0;
                    foreach ((object rawValue, int rawIndex) in rawArray.Cast<object>().WithIndex())
                    {
                        if (backwardIndices.Contains(rawIndex))
                        {
                            continue;
                        }

                        newArray.SetValue(rawValue, copyIndex);
                        copyIndex++;
                    }

                    // payload.RawValues.Add(addItem);
                    // Array.Copy(payload.RawValues.ToArray(), newArray, oldSize);
                    payload.RawListValue = newArray;
                    setterOrNull?.Invoke(newArray);
                }
                else
                {
                    IList rawListValueArray = (IList)payload.RawListValue;
                    foreach (int removeIndex in backwardIndices)
                    {
                        rawListValueArray.RemoveAt(removeIndex);
                    }
                }
            }

            void AddCount(int count)
            {
                int oldSize = payload.RawValues.Count;
                int newSize = oldSize + count;
                object addItem = elementType.IsValueType
                    ? Activator.CreateInstance(elementType)
                    : null;

                if (valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                {
                    beforeSet?.Invoke(rawListValue);
                    Array newArray = Array.CreateInstance(elementType, newSize);
                    payload.RawValues.AddRange(Enumerable.Range(0, count).Select(_ => addItem));
                    Array.Copy(payload.RawValues.ToArray(), newArray, oldSize);
                    payload.RawListValue = newArray;
                    setterOrNull?.Invoke(newArray);
                }
                else
                {
                    IList rawListValueArray = (IList)payload.RawListValue;
                    for (int _ = 0; _ < count; _++)
                    {
                        rawListValueArray.Add(addItem);
                        payload.RawValues.Add(addItem);
                    }

                    payload.AsyncSearchItems.ItemIndexToPropertyIndex = payload.RawValues.Select((_, index) => index).ToList();
                    listViewWrapper.ListView.itemsSource = payload.AsyncSearchItems.ItemIndexToPropertyIndex.ToList();
                }
            }
        }

        private static IEnumerable<IReadOnlyList<int>> DefaultSearchCallbackWithObject(List<object> payloadRawValues, string search)
        {
            const int batchLimit = 10;

            List<int> batchResults = new List<int>();
            int batchCount = 0;
            foreach (int i in Util.SearchArrayObjects(payloadRawValues, search))
            {
                if(i != -1)
                {
                    batchResults.Add(i);
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
        }

        private static void MoveArrayElement(IList list, int fromIndex, int toIndex)
        {
            if (list == null)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentNullException(nameof(list));
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }
            if (fromIndex < 0 || fromIndex >= list.Count)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }
            if (toIndex < 0 || toIndex >= list.Count)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentOutOfRangeException(nameof(toIndex));
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }

            if (fromIndex == toIndex)
            {
                return;
            }

            // shifting
            object item = list[fromIndex];

            if (fromIndex < toIndex)
            {
                for (int i = fromIndex; i < toIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }

            list[toIndex] = item;
        }
    }
}
