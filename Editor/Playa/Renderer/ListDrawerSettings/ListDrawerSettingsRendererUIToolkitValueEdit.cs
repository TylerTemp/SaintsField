#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
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
            public readonly CollectionFoldout Foldout;
            public readonly IntegerField ArraySizeField;
            public readonly ListView ListView;
            public readonly ToolbarSearchField SearchField;
            public readonly VisualElement LoadingImage;
            public readonly ListViewPagerElement Pager;
            public readonly ListViewFooterButtonsElement FooterButtons;

            public ListViewWrapper(string label, ListView listView)
            {
                Add(Foldout = new CollectionFoldout(label));
                VisualElement foldoutContent = Foldout.contentContainer;
                foldoutContent.style.marginLeft = 0;

                ArraySizeField = Foldout.ArraySizeField;

                SearchField = new ToolbarSearchField
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                    },
                };
                TextField searchTextField = SearchField.Q<TextField>();
                searchTextField.style.position = Position.Relative;
                LoadingImage = new Image
                {
                    image = Util.LoadResource<Texture2D>("refresh.png"),
                    pickingMode = PickingMode.Ignore,
                    tintColor = EColor.Gray.GetColor(),
                    style =
                    {
                        position = Position.Absolute,
                        right = 0,
                        top = 1,
                        width = 12,
                        height = 12,
                        visibility = Visibility.Hidden,
                    },
                };
                searchTextField.Add(LoadingImage);
                UIToolkitUtils.SetKeepRotate(LoadingImage);
                LoadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(LoadingImage));
                foldoutContent.Add(SearchField);

                VisualElement textInputElement = ArraySizeField.Q<VisualElement>(name: "unity-text-input");
                if (textInputElement != null)
                {
                    textInputElement.style.borderTopLeftRadius = textInputElement.style.borderTopRightRadius = 0;
                    textInputElement.style.marginLeft = 0;
                }

                foldoutContent.Add(ListView = listView);

                VisualElement footer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexEnd,
                    },
                };
                footer.Add(Pager = new ListViewPagerElement());
                footer.Add(FooterButtons = new ListViewFooterButtonsElement());
                foldoutContent.Add(footer);
            }
        }

        public static ListViewWrapper UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object rawListValue,
            object[] listValue, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor,
            bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
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
                listViewWrapper.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(oldPayload.RawValues.Count);
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

            bool defaultSearch = true;
            bool objectNestedSearch = true;
            (MethodInfo methodInfo, ParamType paramType) extraSearchMethod = default;
            (MethodInfo methodInfo, ParamType paramType) overrideSearchMethod = default;

            if (!string.IsNullOrEmpty(listDrawerSettingsAttribute.ExtraSearch))
            {
                extraSearchMethod = GetSearchMethodInfo(targets[0].GetType(), elementType, listDrawerSettingsAttribute.ExtraSearch);
            }
            bool extraSearch = extraSearchMethod.methodInfo != null;

            // if (!string.IsNullOrEmpty(listDrawerSettingsAttribute.OverrideSearch))
            // {
            //     overrideSearchMethod = GetSearchMethodInfo(targets[0].GetType(), elementType, listDrawerSettingsAttribute.OverrideSearch);
            // }

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

                if (extraSearch && extraSearchMethod.methodInfo != null)
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
                                    if (!Util.SearchObject(item, token.Token, searchedObject, objectNestedSearch))
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
                                    if (!Util.SearchObject(itemProp, token.Token, searchedObjects, objectNestedSearch))
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

                if(defaultSearch)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# use default search method");
#endif
                    foreach (IReadOnlyList<int> batch in DefaultSearchCallbackWithObject(values, search,
                                 objectNestedSearch))
                    {
                        yield return batch;
                    }
                }
            }

            #endregion

            listViewWrapper = new ListViewWrapper(label, new ListView
            {
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                headerTitle = label,
                showAddRemoveFooter = false,
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

            // Size & Page Items Total
            listViewWrapper.ArraySizeField.SetValueWithoutNotify(payload.RawValues.Count);
            listViewWrapper.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(payload.RawValues.Count);
            listViewWrapper.ArraySizeField.RegisterValueChangedCallback(OnSizeInput);
            listViewWrapper.Pager.NumberOfItemsTotalField.RegisterValueChangedCallback(OnSizeInput);

            // Search
            listViewWrapper.SearchField.RegisterValueChangedCallback(_ =>
                UpdatePage(0, listViewWrapper.Pager.NumberOfItemsPerPageField.value));
            listViewWrapper.SearchField.RegisterCallback<KeyDownEvent>(evt =>
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
            listViewWrapper.Pager.NumberOfItemsPerPageField.SetValueWithoutNotify(listDrawerSettingsAttribute.NumberOfItemsPerPage);
            void UpdateNumberOfItemsPerPage(int newValue)
            {
                int newValueClamp = Mathf.Max(newValue, 0);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"update number of items per page {newValueClamp}");
#endif
                UpdatePage(payload.AsyncSearchItems.CurPageIndex, newValueClamp);
            }
            listViewWrapper.Pager.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            // Pre Button
            listViewWrapper.Pager.PagePreButton.clicked += () =>
                UpdatePage(payload.AsyncSearchItems.CurPageIndex - 1,
                    listViewWrapper.Pager.NumberOfItemsPerPageField.value);
            // Next Button
            listViewWrapper.Pager.PageNextButton.clicked += () =>
                UpdatePage(payload.AsyncSearchItems.CurPageIndex + 1,
                    listViewWrapper.Pager.NumberOfItemsPerPageField.value);
            // Input Page Number
            listViewWrapper.Pager.PageField.RegisterValueChangedCallback(evt =>
                UpdatePage(evt.newValue - 1, listViewWrapper.Pager.NumberOfItemsPerPageField.value));

            void UpdatePage(int newPageIndex, int numberOfItemsPerPage)
            {
                string searchText = listViewWrapper.SearchField.value;
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

                listViewWrapper.Pager.PagePreButton.SetEnabled(pagingInfo.CurPageIndex > 0);
                listViewWrapper.Pager.PageNextButton.SetEnabled(pagingInfo.CurPageIndex < pagingInfo.PageCount - 1);

                payload.AsyncSearchItems.ItemIndexToPropertyIndex.Clear();
                payload.AsyncSearchItems.ItemIndexToPropertyIndex.AddRange(pagingInfo.IndexesCurPage);

                payload.AsyncSearchItems.CurPageIndex = pagingInfo.CurPageIndex;

                listViewWrapper.Pager.PageLabel.text = $" / {pagingInfo.PageCount}";
                listViewWrapper.Pager.PageField.SetValueWithoutNotify(payload.AsyncSearchItems.CurPageIndex + 1);

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

            void RefreshSearchingStatus()
            {
                string searchText = listViewWrapper.SearchField.value;
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    UpdatePage(0, listViewWrapper.Pager.NumberOfItemsPerPageField.value);
                    return;
                }

                payload.AsyncSearchItems.DebounceSearchTime = 0;
                payload.AsyncSearchItems.Started = false;
                payload.AsyncSearchItems.Finished = false;
                payload.AsyncSearchItems.FullSources.Clear();
                payload.AsyncSearchItems.SourceGenerator?.Dispose();
                payload.AsyncSearchItems.SourceGenerator = SearchCallback(payload.RawValues, searchText).GetEnumerator();
                payload.AsyncSearchItems.SearchText = searchText;
                UpdatePage(0, listViewWrapper.Pager.NumberOfItemsPerPageField.value);
            }

            listViewWrapper.Foldout.MenuButton.clicked += () =>
            {
                GenericDropdownMenu menu = new GenericDropdownMenu();

                if (setterOrNull == null)
                {
                    menu.AddDisabledItem("Set To Null", false);
                }
                else
                {
                    menu.AddItem("Set To Null", false, () =>
                    {
                        beforeSet?.Invoke(payload.RawListValue);
                        setterOrNull(null);
                    });
                }
                menu.AddSeparator("");

                bool curPaging = listViewWrapper.Pager.style.display != DisplayStyle.None;
                menu.AddItem("Paging", curPaging, () =>
                {
                    if (curPaging)
                    {
                        listViewWrapper.Pager.style.display = DisplayStyle.None;
                        listViewWrapper.Pager.NumberOfItemsPerPageField.value = 0;
                    }
                    else
                    {
                        int configuredItemsPerPage = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                        int itemsPerPage = configuredItemsPerPage > 0
                            ? configuredItemsPerPage
                            : Mathf.Max(5, payload.RawValues.Count / 2);
                        listViewWrapper.Pager.style.display = DisplayStyle.Flex;
                        listViewWrapper.Pager.NumberOfItemsPerPageField.value = itemsPerPage;
                    }
                });

                bool curSearch = listViewWrapper.SearchField.style.display != DisplayStyle.None;
                menu.AddItem("Search", curSearch, () =>
                {
                    listViewWrapper.SearchField.style.display = curSearch ? DisplayStyle.None : DisplayStyle.Flex;
                    if (curSearch)
                    {
                        listViewWrapper.SearchField.SetValueWithoutNotify("");
                    }
                    RefreshSearchingStatus();
                });

                if (curSearch && extraSearchMethod.methodInfo != null)
                {
                    menu.AddItem("Default Search", defaultSearch, () =>
                    {
                        defaultSearch = !defaultSearch;
                        RefreshSearchingStatus();
                    });
                }
                else
                {
                    menu.AddDisabledItem("Default Search", defaultSearch);
                }

                if (curSearch)
                {
                    menu.AddItem("Object Search", objectNestedSearch, () =>
                    {
                        objectNestedSearch = !objectNestedSearch;
                        RefreshSearchingStatus();
                    });
                }
                else
                {
                    menu.AddDisabledItem("Object Search", objectNestedSearch);
                }

                if (extraSearchMethod.methodInfo != null)
                {
                    if (curSearch)
                    {
                        menu.AddItem("Extra Search", extraSearch, () =>
                        {
                            extraSearch = !extraSearch;
                            RefreshSearchingStatus();
                        });
                    }
                    else
                    {
                        menu.AddDisabledItem("Extra Search", extraSearch);
                    }
                }

                Rect menuBound = listViewWrapper.Foldout.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
                menuBound.xMin = menuBound.xMax - Mathf.Max(menuBound.width, 120f);
#endif
                menu.DropDown(menuBound, listViewWrapper.Foldout.MenuButton,
#if UNITY_6000_3_OR_NEWER
                    DropdownMenuSizeMode.Auto
#else
                    true
#endif
                );
            };

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
                    richTextTagProvider,
                    $"{foldoutViewKey}.[${actualIndex}]"
                    ).result;
                if (item != null)
                {
                    visualElement.Clear();
                    visualElement.Add(item);
                }
            }

            listViewWrapper.ListView.bindItem = BindItem;

            listViewWrapper.FooterButtons.AddButton.clicked += () => AddCount(1);
            listViewWrapper.FooterButtons.RemoveButton.clicked += () =>
            {
                List<int> removeIndexInRaw = listViewWrapper.ListView.selectedIndices
                    .Select(removeIndex => payload.AsyncSearchItems.ItemIndexToPropertyIndex[removeIndex])
                    .OrderByDescending(each => each)
                    .ToList();
                RemoveIndicesBackwards(removeIndexInRaw);
            };

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
                    if (listViewWrapper.LoadingImage.style.visibility != Visibility.Visible)
                    {
                        listViewWrapper.LoadingImage.style.visibility = Visibility.Visible;
                        UpdatePage(payload.AsyncSearchItems.CurPageIndex, listViewWrapper.Pager.NumberOfItemsPerPageField.value);
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
                            UpdatePage(payload.AsyncSearchItems.CurPageIndex, listViewWrapper.Pager.NumberOfItemsPerPageField.value);
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

                if (payload.AsyncSearchItems.Finished && listViewWrapper.LoadingImage.style.visibility != Visibility.Hidden)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# disable loader image {_asyncSearchItems.SearchText}");
#endif
                    listViewWrapper.LoadingImage.style.visibility = Visibility.Hidden;
                }
            }).Every(1);

            UpdatePage(0, listViewWrapper.Pager.NumberOfItemsPerPageField.value);

            bool noSearch = !listDrawerSettingsAttribute.Searchable;
            if (noSearch)
            {
                listViewWrapper.SearchField.style.display = DisplayStyle.None;
            }

            bool noPaging = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0;
            if (noPaging)
            {
                listViewWrapper.Pager.style.display = DisplayStyle.None;
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

        private static IEnumerable<IReadOnlyList<int>> DefaultSearchCallbackWithObject(List<object> payloadRawValues,
            string search, bool objectNestedSearch)
        {
            const int batchLimit = 10;

            List<int> batchResults = new List<int>();
            int batchCount = 0;
            foreach (int i in Util.SearchArrayObjects(payloadRawValues, search, objectNestedSearch))
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
#endif
