#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class SerializedListElement : VisualElement
    {
        public SerializedProperty Property { get; }
        public CollectionFoldout Foldout { get; }
        public ListView ListView { get; }
        public ListViewFooterButtonsElement FooterButtons { get; }
        private readonly Func<(int min, int max)> _getSizeLimits;
        private ListView _listView;
        private Action _refresh;

        public SerializedListElement(SerializedProperty property, Type elementType, string label,
            ListDrawerSettingsAttribute listDrawerSettingsAttribute,
            Func<SerializedProperty, int, VisualElement> createCell,
            Func<int, IReadOnlyList<ListSearchToken>, bool> customSearch = null,
            Func<(int min, int max)> getSizeLimits = null)
        {
            Property = property.Copy();
            property = Property;
            _getSizeLimits = getSizeLimits;
            style.flexGrow = 1;
            AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);
            AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
            bool defaultSearch = true;
            bool objectNestedSearch = true;
            bool extraSearch = customSearch != null;

            IEnumerable<IReadOnlyList<int>> SearchCallbackWithCustom(SerializedProperty arrayProperty, string search)
            {
                const int batchLimit = 10;
                List<int> batch = new List<int>();
                int count = 0;
                if (extraSearch && customSearch != null)
                {
                    IReadOnlyList<ListSearchToken> tokens = SerializedUtils.ParseSearch(search).ToList();
                    for (int index = 0; index < arrayProperty.arraySize; index++)
                    {
                        SerializedProperty item = arrayProperty.GetArrayElementAtIndex(index);
                        if (customSearch(index, tokens) || (defaultSearch && tokens.All(token =>
                                SerializedUtils.SearchProp(item, token.Token, objectNestedSearch, new HashSet<object>()))))
                        {
                            batch.Add(index);
                        }
                        if (++count % batchLimit == 0)
                        {
                            yield return batch.ToArray();
                            batch.Clear();
                        }
                    }
                }
                else if (defaultSearch)
                {
                    foreach (int index in SerializedUtils.SearchArrayProperty(arrayProperty, search, objectNestedSearch))
                    {
                        if (index >= 0)
                        {
                            batch.Add(index);
                        }
                        if (++count % batchLimit == 0)
                        {
                            yield return batch.ToArray();
                            batch.Clear();
                        }
                    }
                }
                if (batch.Count > 0)
                {
                    yield return batch;
                }
            }

            CollectionFoldout root = new CollectionFoldout(label)
            {
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
                viewDataKey = SerializedUtils.GetUniqueId(property),
            };
            Foldout = root;
            Add(root);
            root.contentContainer.style.marginLeft = 0;

            root.hierarchy.Add(new EmptyPrefabOverrideElement(property)
            {
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    height = 18,
                },
                pickingMode = PickingMode.Ignore,
            });

            List<int> fullList = Enumerable.Range(0, property.arraySize).ToList();
            _asyncSearchItems = new AsyncSearchItems
            {
                Started = true,
                Finished = true,
                SourceGenerator = Enumerable.Empty<IReadOnlyList<int>>().GetEnumerator(),
                FullSources = fullList,
                CachedFullSources = new List<int>(fullList),
                SearchText = "",
                DebounceSearchTime = double.MaxValue,

                ItemIndexToPropertyIndex = Enumerable.Range(0, property.arraySize).ToList(),
                CurPageIndex = 0,
            };

            VisualElement MakeItem()
            {
                return new VisualElement();
            }

            void BindItem(VisualElement element, int index)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log(($"bind: {index}, propIndex: {_asyncSearchItems.ItemIndexToPropertyIndex[index]}, itemIndexes={string.Join(", ", _asyncSearchItems.ItemIndexToPropertyIndex)}"));
#endif
                if(index >= _asyncSearchItems.ItemIndexToPropertyIndex.Count)
                {
                    return;
                }

                int propIndex = _asyncSearchItems.ItemIndexToPropertyIndex[index];
                if(propIndex >= property.arraySize)
                {
                    return;
                }

                MarkElementTreeRowIndex(_listView, element, index);

                SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);
                VisualElement resultField = createCell(prop, propIndex);
                UIToolkitUtils.Unbind(element);
                element.Clear();
                if(resultField != null)
                {
                    element.Add(resultField);
                    resultField.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        evt.menu.AppendAction("Copy Element Property Path",
                            _ => EditorGUIUtility.systemCopyBuffer = prop.propertyPath);

                        if (ClipboardHelper.CanCopySerializedProperty(prop.propertyType))
                        {
                            evt.menu.AppendAction("Copy Element", _ => ClipboardHelper.DoCopySerializedProperty(prop));
                        }

                        (bool hasReflectionPaste, bool hasValuePaste) =
                            ClipboardHelper.CanPasteSerializedProperty(prop.propertyType);

                        if (hasReflectionPaste)
                        {
                            evt.menu.AppendAction("Paste Element", _ =>
                            {
                                ClipboardHelper.DoPasteSerializedProperty(prop);
                                property.serializedObject.ApplyModifiedProperties();
                            }, hasValuePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        }

                        evt.menu.AppendAction("Delete Element", _ =>
                        {
                            if (CanRemove())
                            {
                                DeleteElement(propIndex);
                            }
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }));
                }
            }

            _listView = new ListView(Enumerable.Range(0, property.arraySize).ToList())
            {
                makeItem = MakeItem,
                unbindItem = (element, _) =>
                {
                    UIToolkitUtils.Unbind(element);
                    element.Clear();
                },
                bindItem = BindItem,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                headerTitle = property.displayName,
                showAddRemoveFooter = false,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
                viewDataKey = property.propertyPath,
            };
            _listView.selectedIndicesChanged += _ => ClearNestedSelectionAndFocusOutsideSelection(_listView);
            ListView = _listView;

            UIToolkitUtils.AddContextualMenuManipulator(root, property, () => {});
            Toggle toggle = root.Q<Toggle>();
            if (toggle != null && toggle.style.marginLeft != -12)
            {
                toggle.style.marginLeft = -12;
            }

            VisualElement listContent = new VisualElement();
            root.Add(listContent);
            listContent.AddToClassList("unity-collection-view--with-border");
            listContent.AddToClassList("unity-list-view__scroll-view--with-footer");

            #region Search

            ToolbarSearchField searchField = new ToolbarSearchField
            {
                style =
                {
                    display = listDrawerSettingsAttribute.Searchable ? DisplayStyle.Flex : DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                    width = StyleKeyword.Auto,
                },
            };

            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    if (!_asyncSearchItems.Started && _asyncSearchItems.SourceGenerator != null &&
                        _asyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                    {
                        _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1;
                    }
                }
            }, TrickleDown.TrickleDown);

            TextField searchTextField = searchField.Q<TextField>();
            searchTextField.style.position = Position.Relative;
            Image loadingImage = new Image
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
            searchTextField.Add(loadingImage);
            UIToolkitUtils.SetKeepRotate(loadingImage);
            loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingImage));

            listContent.Add(searchField);

            #endregion

            #region Paging

            ListViewPagerElement pager = new ListViewPagerElement
            {
                style =
                {
                    display = listDrawerSettingsAttribute.NumberOfItemsPerPage > 0
                        ? DisplayStyle.Flex
                        : DisplayStyle.None,
                },
            };

            IntegerField numberOfItemsPerPageField = pager.NumberOfItemsPerPageField;
            IntegerField numberOfItemsTotalField = pager.NumberOfItemsTotalField;
            numberOfItemsPerPageField.SetValueWithoutNotify(Mathf.Max(0, listDrawerSettingsAttribute.NumberOfItemsPerPage));
            numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
            IntegerField numberOfItemsTopRightField = root.ArraySizeField;
            numberOfItemsTopRightField.SetValueWithoutNotify(property.arraySize);

            Button pagePreButton = pager.PagePreButton;
            IntegerField pageField = pager.PageField;
            Label pageLabel = pager.PageLabel;
            Button pageNextButton = pager.PageNextButton;

            VisualElement footer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                },
            };
            footer.Add(pager);

            ListViewFooterButtonsElement listViewFooterButtons = new ListViewFooterButtonsElement();
            FooterButtons = listViewFooterButtons;
            footer.Add(listViewFooterButtons);
            root.Add(footer);

            void LocalUpdatePage(int newPageIndex, int numberOfItemsPerPage)
            {
                if (!SerializedUtils.IsOk(property))
                {
                    return;
                }
                string searchText = searchField.value;
                List<int> resultIndexes;
                if (string.IsNullOrWhiteSpace(searchText))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Empty search");
#endif
                    resultIndexes = Enumerable.Range(0, property.arraySize).ToList();
                    _asyncSearchItems.Started = true;
                    _asyncSearchItems.Finished = true;
                    _asyncSearchItems.CachedFullSources = new List<int>(resultIndexes);
                    _asyncSearchItems.FullSources = new List<int>(resultIndexes);
                    _asyncSearchItems.SearchText = "";
                    _asyncSearchItems.SourceGenerator?.Dispose();
                    _asyncSearchItems.SourceGenerator = null;
                }
                else if (_asyncSearchItems.SearchText == searchText)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Cached search {_asyncSearchItems.SearchText}, started={_asyncSearchItems.Started}, finished={_asyncSearchItems.Finished}");
#endif
                    resultIndexes = _asyncSearchItems.FullSources;
                }
                else
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# New search {searchText}");
#endif
                    _asyncSearchItems.SearchText = searchText;
                    _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + 0.6f;
                    _asyncSearchItems.Started = false;
                    _asyncSearchItems.Finished = false;
                    _asyncSearchItems.FullSources.Clear();
                    if (_asyncSearchItems.SourceGenerator != null)
                    {
                        _asyncSearchItems.SourceGenerator.Dispose();
                        _asyncSearchItems.SourceGenerator = null;
                    }
                    _asyncSearchItems.SourceGenerator = SearchCallbackWithCustom(property, searchText).GetEnumerator();

                    resultIndexes = _asyncSearchItems.CachedFullSources;
                }

                PagingInfo pagingInfo = GetPagingInfo(newPageIndex, resultIndexes, numberOfItemsPerPage);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"index search={searchField.value} result: {string.Join(",", pagingInfo.IndexesAfterSearch)}; numberOfItemsPerPage={numberOfItemsPerPage}");
#endif

                pagePreButton.SetEnabled(pagingInfo.CurPageIndex > 0);
                pageNextButton.SetEnabled(pagingInfo.CurPageIndex < pagingInfo.PageCount - 1);

                _asyncSearchItems.ItemIndexToPropertyIndex.Clear();
                _asyncSearchItems.ItemIndexToPropertyIndex.AddRange(pagingInfo.IndexesCurPage);

                _asyncSearchItems.CurPageIndex = pagingInfo.CurPageIndex;

                pageLabel.text = $" / {pagingInfo.PageCount}";
                pageField.SetValueWithoutNotify(_asyncSearchItems.CurPageIndex + 1);

                List<int> curPageItems = pagingInfo.IndexesCurPage;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"set items: {string.Join(", ", curPageItems)}, itemIndexToPropertyIndex={string.Join(",", _asyncSearchItems.ItemIndexToPropertyIndex)}");
#endif
                if(!_listView.itemsSource.Cast<int>().SequenceEqual(curPageItems))
                {
                    _listView.itemsSource = curPageItems;
                    _listView.Rebuild();
                }

            }

            void RefreshSearchingStatus()
            {
                string searchText = searchField.value;
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                    return;
                }

                _asyncSearchItems.DebounceSearchTime = 0;
                _asyncSearchItems.Started = false;
                _asyncSearchItems.Finished = false;
                _asyncSearchItems.FullSources.Clear();
                _asyncSearchItems.SourceGenerator?.Dispose();
                _asyncSearchItems.SourceGenerator = SearchCallbackWithCustom(property, searchText).GetEnumerator();
                _asyncSearchItems.SearchText = searchText;
                LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
            }

            int arraySize = property.arraySize;

            void CheckArraySizeChange()
            {
                int newSize;
                try
                {
                    newSize = property.arraySize;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (NullReferenceException)
                {
                    return;
                }

                if (newSize == arraySize)
                {
                    return;
                }

                arraySize = newSize;
                NotifyChanged();
                RefreshSearchingStatus();
                numberOfItemsTotalField.SetValueWithoutNotify(arraySize);
                numberOfItemsTopRightField.SetValueWithoutNotify(arraySize);
                LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
            }

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                _listView.TrackPropertyValue(property, _ =>
                {
                    CheckArraySizeChange();
                    RefreshSearchingStatus();
                });
                RefreshSearchingStatus();
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                UIToolkitUtils.Unbind(_listView);
                StopSearch();
            });
            schedule.Execute(() =>
            {
                if (SerializedUtils.IsOk(property))
                {
                    CheckArraySizeChange();
                    UpdateSizeLimits();
                }
            }).Every(100);

            searchField.RegisterValueChangedCallback(_ =>
            {
                LocalUpdatePage(0, numberOfItemsPerPageField.value);
            });

            pagePreButton.clicked += () =>
            {
                LocalUpdatePage(_asyncSearchItems.CurPageIndex - 1, numberOfItemsPerPageField.value);
            };
            pageNextButton.clicked += () =>
            {
                LocalUpdatePage(_asyncSearchItems.CurPageIndex + 1, numberOfItemsPerPageField.value);
            };
            pageField.RegisterValueChangedCallback(evt => LocalUpdatePage(evt.newValue - 1, numberOfItemsPerPageField.value));

            void NumberOfItemsTotalChangedCallback(ChangeEvent<int> e)
            {
                (int min, int max) = GetSizeLimits();

                int newSize = Mathf.Max(0, e.newValue);

                if(min >= 0 && newSize < min)
                {
                    newSize = min;
                }
                else if(max >= 0 && newSize > max)
                {
                    newSize = max;
                }

                if(property.arraySize != newSize)
                {
                    Resize(newSize);
                    LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                }
                else
                {
                    numberOfItemsTopRightField.SetValueWithoutNotify(property.arraySize);
                    numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
                }
            }

            numberOfItemsTopRightField.RegisterValueChangedCallback(NumberOfItemsTotalChangedCallback);
            numberOfItemsTotalField.RegisterValueChangedCallback(NumberOfItemsTotalChangedCallback);

            void UpdateNumberOfItemsPerPage(int newValue)
            {
                int newValueClamp = Mathf.Max(newValue, 0);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"update number of items per page {newValueClamp}");
#endif
                LocalUpdatePage(_asyncSearchItems.CurPageIndex, newValueClamp);
            }

            numberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            listViewFooterButtons.AddButton.clicked += () =>
            {
                if (!CanAdd())
                {
                    return;
                }
                Resize(property.arraySize + 1);
                int totalVisiblePage = numberOfItemsPerPageField.value > 0
                    ? Mathf.CeilToInt((float)property.arraySize / numberOfItemsPerPageField.value)
                    : 1;
                LocalUpdatePage(totalVisiblePage - 1, numberOfItemsPerPageField.value);
                numberOfItemsTopRightField.SetValueWithoutNotify(property.arraySize);
                numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
            };

            listViewFooterButtons.RemoveButton.clicked += () =>
            {
                List<int> curRemoveObjects = _listView.selectedIndices.ToList();
                if (curRemoveObjects.Count == 0)
                {
                    return;
                }

                int removable = GetSizeLimits().min < 0 ? property.arraySize : Mathf.Max(0, property.arraySize - GetSizeLimits().min);
                foreach (int index in curRemoveObjects.Select(removeIndex => _asyncSearchItems.ItemIndexToPropertyIndex[removeIndex]).OrderByDescending(each => each).Take(removable))
                {
                    DeleteElement(index);
                }

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                numberOfItemsTopRightField.SetValueWithoutNotify(property.arraySize);
                numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"removed update page to {_asyncSearchItems.CurPageIndex}");
#endif

                _listView.schedule.Execute(() => LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value));
            };

            #endregion

            #region Menu

            root.MenuButton.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                bool curPaging = pager.style.display != DisplayStyle.None;
                genericDropdownMenu.AddItem("Paging", curPaging, () =>
                {
                    if (curPaging)
                    {
                        pager.style.display = DisplayStyle.None;
                        numberOfItemsPerPageField.value = -1;
                    }
                    else
                    {
                        int configuredItemsPerPage = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                        int itemsPerPage = configuredItemsPerPage > 0
                            ? configuredItemsPerPage
                            : Mathf.Max(5, property.arraySize / 2);
                        pager.style.display = DisplayStyle.Flex;
                        numberOfItemsPerPageField.value = itemsPerPage;
                    }
                });

                bool curSearch = searchField.style.display != DisplayStyle.None;
                genericDropdownMenu.AddItem("Search", curSearch, () =>
                {
                    searchField.style.display = curSearch ? DisplayStyle.None : DisplayStyle.Flex;
                    if (curSearch)
                    {
                        searchField.SetValueWithoutNotify("");
                    }
                    RefreshSearchingStatus();
                });
                if(curSearch)
                {
                    if (customSearch != null)
                    {
                        genericDropdownMenu.AddItem("Default Search", defaultSearch,
                            () =>
                            {
                                defaultSearch = !defaultSearch;
                                RefreshSearchingStatus();
                            });
                    }
                    else
                    {
                        genericDropdownMenu.AddDisabledItem("Default Search", defaultSearch);
                    }
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem("Default Search", defaultSearch);
                }

                if(curSearch)
                {
                    genericDropdownMenu.AddItem("Object Search", objectNestedSearch,
                        () =>
                        {
                            objectNestedSearch = !objectNestedSearch;
                            RefreshSearchingStatus();
                        });
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem("Object Search", objectNestedSearch);
                }

                if (customSearch != null)
                {
                    if(curSearch)
                    {
                        genericDropdownMenu.AddItem("Extra Search", extraSearch, () =>
                        {
                            extraSearch = !extraSearch;
                            RefreshSearchingStatus();
                        });
                    }
                    else
                    {
                        genericDropdownMenu.AddDisabledItem("Extra Search", extraSearch);
                    }
                }

                Rect menuBound = root.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
                menuBound.xMin = menuBound.xMax - Mathf.Max(menuBound.width, 120f);
#endif
                genericDropdownMenu.DropDown(menuBound, root.MenuButton,
#if UNITY_6000_3_OR_NEWER
                    DropdownMenuSizeMode.Auto
#else
                    true
#endif
                );
            };

            #endregion

            #region Drag

            Toggle foldoutToggle = root.Q<Toggle>();
            foldoutToggle.RegisterCallback<DragEnterEvent>(_ =>
            {
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            foldoutToggle.RegisterCallback<DragLeaveEvent>(_ =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.None;
            });
            foldoutToggle.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            foldoutToggle.RegisterCallback<DragPerformEvent>(_ =>
            {
                if (!Drop(elementType))
                {
                    return;
                }

                property.serializedObject.ApplyModifiedProperties();
                LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
            });
            #endregion

            _listView.itemIndexChanged += (first, second) =>
            {
                int fromPropIndex = _asyncSearchItems.ItemIndexToPropertyIndex[first];
                int toPropIndex = _asyncSearchItems.ItemIndexToPropertyIndex[second];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"drag {fromPropIndex}({first}) -> {toPropIndex}({second})");
#endif

                property.MoveArrayElement(fromPropIndex, toPropIndex);
                property.serializedObject.ApplyModifiedProperties();
            };

            _listView.RegisterCallback<KeyDownEvent>(evt =>
            {
                bool ctrl = evt.modifiers == EventModifiers.Control ||
                            evt.modifiers == EventModifiers.Command;

                bool copyCommand = ctrl && evt.keyCode == KeyCode.C;
                if (copyCommand)
                {
                    int selectedIndex = _listView.selectedIndices
                        .DefaultIfEmpty(-1)
                        .First();

                    if (selectedIndex == -1)
                    {
                        return;
                    }

                    int propIndex = _asyncSearchItems.ItemIndexToPropertyIndex[selectedIndex];
                    if(propIndex >= property.arraySize)
                    {
                        return;
                    }
                    SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);

                    if (ClipboardHelper.CanCopySerializedProperty(prop.propertyType))
                    {
                        ClipboardHelper.DoCopySerializedProperty(prop);
                    }
                }

                bool pasteCommand = ctrl && evt.keyCode == KeyCode.V;
                if (pasteCommand)
                {
                    int selectedIndex = _listView.selectedIndices
                        .DefaultIfEmpty(-1)
                        .First();

                    if (selectedIndex == -1)
                    {
                        return;
                    }

                    int propIndex = _asyncSearchItems.ItemIndexToPropertyIndex[selectedIndex];
                    if(propIndex >= property.arraySize)
                    {
                        return;
                    }
                    SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);

                    (bool pasteHasReflection, bool pasteHasValue) = ClipboardHelper.CanPasteSerializedProperty(prop.propertyType);
                    if (pasteHasReflection && pasteHasValue)
                    {
                        ClipboardHelper.DoPasteSerializedProperty(prop);
                        prop.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            _listView.schedule.Execute(() =>
            {
                if (!SerializedUtils.IsOk(property))
                {
                    StopSearch();
                    return;
                }
                if(!_asyncSearchItems.Started && EditorApplication.timeSinceStartup > _asyncSearchItems.DebounceSearchTime)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Debounce reached, start {_asyncSearchItems.SearchText}");
#endif
                    _asyncSearchItems.Started = true;
                    Debug.Assert(_asyncSearchItems.SourceGenerator != null);
                }

                if (_asyncSearchItems.Started && !_asyncSearchItems.Finished)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# Continue search {_asyncSearchItems.SearchText}");
#endif
                    if (loadingImage.style.visibility != Visibility.Visible)
                    {
                        loadingImage.style.visibility = Visibility.Visible;
                        LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                    }

                    if (_asyncSearchItems.SourceGenerator.MoveNext())
                    {
                        IReadOnlyList<int> currentValue = _asyncSearchItems.SourceGenerator.Current;

                        if(currentValue != null && currentValue.Count > 0)
                        {
                            _asyncSearchItems.FullSources.AddRange(currentValue);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"#Search# add search results {string.Join(", ", currentValue)}");
#endif
                            LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                        }
                    }
                    else
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                        Debug.Log($"#Search# end search {_asyncSearchItems.SearchText}");
#endif
                        _asyncSearchItems.Finished = true;
                        _asyncSearchItems.SourceGenerator.Dispose();
                        _asyncSearchItems.SourceGenerator = null;
                    }
                }

                if (_asyncSearchItems.Finished && loadingImage.style.visibility != Visibility.Hidden)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"#Search# disable loader image {_asyncSearchItems.SearchText}");
#endif
                    loadingImage.style.visibility = Visibility.Hidden;
                }
            }).Every(1);

            listContent.Add(_listView);

            _refresh = () =>
            {
                CheckArraySizeChange();
                LocalUpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                UpdateSizeLimits();
            };
            root.SetValueWithoutNotify(property.isExpanded);
            root.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);
            Refresh(label);
        }

        public bool Matches(SerializedProperty property)
        {
            return SerializedUtils.IsOk(Property) && Property.serializedObject == property.serializedObject &&
                   Property.propertyPath == property.propertyPath;
        }

        public void Refresh(string label)
        {
            if (!SerializedUtils.IsOk(Property))
            {
                return;
            }
            Foldout.Q<Foldout>().text = string.IsNullOrEmpty(label) ? Property.displayName : label;
            Foldout.SetValueWithoutNotify(Property.isExpanded);
            _refresh?.Invoke();
        }

        public void UpdateSizeLimits()
        {
            if (FooterButtons != null && SerializedUtils.IsOk(Property))
            {
                FooterButtons.AddButton.SetEnabled(CanAdd());
                FooterButtons.RemoveButton.SetEnabled(CanRemove());
            }
        }

        private (int min, int max) GetSizeLimits() => _getSizeLimits?.Invoke() ?? (-1, -1);
        private bool CanAdd() => GetSizeLimits().max < 0 || Property.arraySize < GetSizeLimits().max;
        private bool CanRemove() => Property.arraySize > Mathf.Max(0, GetSizeLimits().min);

        private void Resize(int newSize)
        {
            int oldSize = Property.arraySize;
            Property.arraySize = newSize;
            Property.serializedObject.ApplyModifiedProperties();
            if (newSize > oldSize)
            {
                for (int index = oldSize; index < newSize; index++)
                {
                    UIToolkitUtils.UnlinkAddedArrayElementManagedReferences(Property, index, new HashSet<int> { index });
                }
            }
            UpdateSizeLimits();
        }

        private void DeleteElement(int index)
        {
            int oldSize = Property.arraySize;
            Property.DeleteArrayElementAtIndex(index);
            if (Property.arraySize == oldSize)
            {
                Property.DeleteArrayElementAtIndex(index);
            }
        }

        private void NotifyChanged()
        {
            using (SerializedPropertyChangeEvent evt = SerializedPropertyChangeEvent.GetPooled(Property))
            {
                evt.target = this;
                SendEvent(evt);
            }
            SaintsEditorApplicationChanged.OnSaintsFieldChangedEvent.Invoke();
        }

        public void StopSearch()
        {
            _asyncSearchItems.SourceGenerator?.Dispose();
            _asyncSearchItems.SourceGenerator = null;
            _asyncSearchItems.Started = true;
            _asyncSearchItems.Finished = true;
        }

        private static IEnumerable<UnityEngine.Object> CanDrop(IEnumerable<UnityEngine.Object> objects, Type elementType)
            => objects.Where(each => Util.GetTypeFromObj(each, elementType));

        private bool Drop(Type elementType)
        {
            int max = GetSizeLimits().max;
            int available = max < 0 ? int.MaxValue : Mathf.Max(0, max - Property.arraySize);
            UnityEngine.Object[] objects = CanDrop(DragAndDrop.objectReferences, elementType).Take(available).ToArray();
            if (objects.Length == 0)
            {
                return false;
            }
            DragAndDrop.AcceptDrag();
            int start = Property.arraySize;
            Property.arraySize += objects.Length;
            for (int index = 0; index < objects.Length; index++)
            {
                Property.GetArrayElementAtIndex(start + index).objectReferenceValue = objects[index];
            }
            return true;
        }

        private class AsyncSearchItems
        {
            public bool Started;
            public bool Finished;
            public IEnumerator<IReadOnlyList<int>> SourceGenerator;
            public List<int> FullSources;
            public string SearchText;
            public double DebounceSearchTime;
            public List<int> CachedFullSources;

            public List<int> ItemIndexToPropertyIndex;
            public int CurPageIndex;
        }

        private AsyncSearchItems _asyncSearchItems;

        private struct PagingInfo
        {
            public IReadOnlyList<int> IndexesAfterSearch;
            public List<int> IndexesCurPage;
            public int CurPageIndex;
            public int PageCount;
        }

        private static PagingInfo GetPagingInfo(int newPageIndex, IReadOnlyList<int> fullIndexResults, int numberOfItemsPerPage)
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
                pageCount = Mathf.Max(1, Mathf.CeilToInt((float)fullIndexResults.Count / numberOfItemsPerPage));
                curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                skipStart = curPageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            List<int> curPageItemIndexes = fullIndexResults.Skip(skipStart).Take(itemCount).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"set items: {string.Join(", ", curPageItemIndexes)}, itemIndexToPropertyIndex={string.Join(",", fullIndexResults)}");
#endif

            return new PagingInfo
            {
                IndexesAfterSearch = fullIndexResults,
                IndexesCurPage = curPageItemIndexes,
                CurPageIndex = curPageIndex,
                PageCount = pageCount,
            };
        }

        private static void MarkElementTreeRowIndex(ListView owner, VisualElement element, int rowIndex)
        {
            element.userData = new RowFocusData(owner, rowIndex);
        }

        private static void ClearNestedSelectionAndFocusOutsideSelection(ListView listView)
        {
            foreach (ListView nestedListView in listView.Query<ListView>().Build().Where(each => !ReferenceEquals(each, listView)))
            {
                if (!TryGetRowIndex(listView, nestedListView, out int nestedRowIndex))
                {
                    continue;
                }

                if (listView.selectedIndices.Contains(nestedRowIndex))
                {
                    continue;
                }

                nestedListView.ClearSelection();
            }

            Focusable focused = listView.panel?.focusController?.focusedElement;
            if (!(focused is VisualElement focusedElement) || !listView.Contains(focusedElement))
            {
                return;
            }

            if (!TryGetRowIndex(listView, focusedElement, out int focusedRowIndex))
            {
                return;
            }

            if (listView.selectedIndices.Contains(focusedRowIndex))
            {
                return;
            }

            focusedElement.Blur();
            listView.Focus();
        }

        private static bool TryGetRowIndex(ListView owner, VisualElement element, out int rowIndex)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.userData is RowFocusData rowData && ReferenceEquals(rowData.Owner, owner))
                {
                    rowIndex = rowData.RowIndex;
                    return true;
                }
            }

            rowIndex = -1;
            return false;
        }

        private readonly struct RowFocusData
        {
            public readonly ListView Owner;
            public readonly int RowIndex;

            public RowFocusData(ListView owner, int rowIndex)
            {
                Owner = owner;
                RowIndex = rowIndex;
            }
        }

    }
}
#endif
