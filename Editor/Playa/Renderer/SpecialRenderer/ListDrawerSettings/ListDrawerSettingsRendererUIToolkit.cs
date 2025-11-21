#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer
    {
        // private PropertyField _result;
        // private VisualElement _fieldElement;

        private Button _addButton;
        private Button _removeButton;

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            Debug.Assert(listDrawerSettingsAttribute != null, $"{FieldWithInfo.SerializedProperty.propertyPath}");
            // ArraySizeAttribute arraySizeAttribute =
            //     FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();

            (VisualElement root, Button addButton, Button removeButton) = MakeListDrawerSettingsField(listDrawerSettingsAttribute, FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault());
            _addButton = addButton;
            _removeButton = removeButton;

            return (root, false);
        }

        private enum ParamType
        {
            TargetAndIndex,
            Target,
            Index,
        }

        private static (MethodInfo, ParamType) GetSearchMethodInfo(Type targetType, Type elementType, string methodName)
        {
            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(targetType))
            {
                foreach (MethodInfo methodInfo in eachType.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (methodInfo.Name != methodName)
                    {
                        continue;
                    }

                    if (methodInfo.ReturnParameter?.ParameterType != typeof(bool))
                    {
                        continue;
                    }

                    ParameterInfo[] methodParams = methodInfo.GetParameters();

                    // ReSharper disable once UseIndexFromEndExpression
                    bool lastMatch = typeof(IEnumerable<ListSearchToken>).IsAssignableFrom(methodParams[methodParams.Length - 1].ParameterType);
                    if (!lastMatch)
                    {
                        continue;
                    }

                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (methodParams.Length == 3
                        && elementType.IsAssignableFrom(methodParams[0].ParameterType)
                        && typeof(int).IsAssignableFrom(methodParams[1].ParameterType))
                    {
                        return (methodInfo, ParamType.TargetAndIndex);
                    }

                    if (methodParams.Length == 2 && elementType.IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, ParamType.Target);
                    }

                    if (methodParams.Length == 2 && typeof(int).IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, ParamType.Index);
                    }
                }
            }

            return (null, default);
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

        private (VisualElement root, Button addButton, Button removeButton) MakeListDrawerSettingsField(ListDrawerSettingsAttribute listDrawerSettingsAttribute, ArraySizeAttribute arraySizeAttribute)
        {


            Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
                                                           FieldWithInfo.PropertyInfo.PropertyType);

            // search functions
            string extraSearchCallback = listDrawerSettingsAttribute.ExtraSearch;
            string overrideSearchCallback = listDrawerSettingsAttribute.OverrideSearch;

            (MethodInfo methodInfo, ParamType paramType) extraSearchMethod = default;
            (MethodInfo methodInfo, ParamType paramType) overrideSearchMethod = default;

            if (!string.IsNullOrEmpty(extraSearchCallback))
            {
                extraSearchMethod = GetSearchMethodInfo(FieldWithInfo.Targets[0].GetType(), elementType, extraSearchCallback);
            }

            if (!string.IsNullOrEmpty(overrideSearchCallback))
            {
                overrideSearchMethod = GetSearchMethodInfo(FieldWithInfo.Targets[0].GetType(), elementType, overrideSearchCallback);
            }

            IEnumerable<IReadOnlyList<int>> SearchCallback(SerializedProperty arrayProperty, string search)
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
                        foreach (int fullIndex in Enumerable.Range(0, arrayProperty.arraySize))
                        {
                            if ((bool)overrideSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0],
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
                        IEnumerable rawValueList = (IEnumerable)FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Targets[0]);

                        int curIndex = 0;

                        List<int> batchResults = new List<int>();
                        int batchCount = 0;

                        foreach (object rawValue in rawValueList)
                        {
                            object[] methodParams = overrideSearchMethod.paramType == ParamType.Target
                                ? new[] { rawValue, searchTokens }
                                : new[] { rawValue, curIndex, searchTokens };

                            if ((bool)overrideSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0], methodParams))
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

                        foreach (int fullIndex in Enumerable.Range(0, arrayProperty.arraySize))
                        {
                            if ((bool)extraSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0],
                                    new object[] { fullIndex, searchTokens }))
                            {
                                // yield return fullIndex;
                                batchResults.Add(fullIndex);
                            }
                            else
                            {
                                SerializedProperty itemProp = arrayProperty.GetArrayElementAtIndex(fullIndex);
                                HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                                    .Select(_ => new HashSet<object>())
                                    .ToArray();
                                bool all = true;
                                for (int index = 0; index < searchTokens.Count; index++)
                                {
                                    ListSearchToken token = searchTokens[index];
                                    HashSet<object> searchedObject = searchedObjectsArray[index];
                                    // ReSharper disable once InvertIf
                                    if (!SerializedUtils.SearchProp(itemProp, token.Token, searchedObject))
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
                        IEnumerable rawValueList = (IEnumerable)FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Targets[0]);

                        int curIndex = 0;

                        List<int> batchResults = new List<int>();
                        int batchCount = 0;
                        foreach (object rawValue in rawValueList)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"#Search# pass rawValue {rawValue}/{curIndex}");
#endif
                            object[] methodParams = extraSearchMethod.paramType == ParamType.Target
                                ? new[] { rawValue, searchTokens }
                                : new[] { rawValue, curIndex, searchTokens };

                            if ((bool)extraSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0], methodParams))
                            {
                                // Debug.Log($"yield {curIndex}/{rawValue} in extra search");
                                // yield return curIndex;
                                batchResults.Add(curIndex);
                            }
                            else
                            {
                                SerializedProperty itemProp = arrayProperty.GetArrayElementAtIndex(curIndex);
                                HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                                    .Select(_ => new HashSet<object>())
                                    .ToArray();

                                bool all = true;
                                for (int index = 0; index < searchTokens.Count; index++)
                                {
                                    ListSearchToken token = searchTokens[index];
                                    HashSet<object> searchedObjects = searchedObjectsArray[index];
                                    if (!SerializedUtils.SearchProp(itemProp, token.Token, searchedObjects))
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
                    List<int> batchResults = new List<int>();
                    int batchCount = 0;
                    foreach (int i in SerializedUtils.SearchArrayProperty(arrayProperty, search))
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
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
            };

            SerializedProperty property = FieldWithInfo.SerializedProperty;

            root.Add(new EmptyPrefabOverrideElement(property)
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

            // int numberOfItemsPerPage = 0;

            VisualElement MakeItem()
            {
                // PropertyField propertyField = new PropertyField();
                return new VisualElement();
            }

            PropertyAttribute[] allAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo);

            void BindItem(VisualElement element, int index)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log(($"bind: {index}, propIndex: {itemIndexToPropertyIndex[index]}, itemIndexes={string.Join(", ", itemIndexToPropertyIndex)}"));
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

                SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);
                VisualElement resultField = UIToolkitUtils.CreateOrUpdateFieldProperty(
                    prop,
                    allAttributes,
                    ReflectUtils.GetElementType(FieldWithInfo.FieldInfo.FieldType),
                    $"Element {index}",
                    FieldWithInfo.FieldInfo,
                    InAnyHorizontalLayout,
                    this,
                    this,
                    null,
                    false,
                    FieldWithInfo.Targets[0]
                );
                // Debug.Log($"draw list item {prop.propertyPath}: {resultField}");
                // PropertyField propertyField = (PropertyField)propertyFieldRaw;
                // propertyField.BindProperty(prop);
                // Debug.Log(prop.propertyPath);
                element.Clear();
                // ReSharper disable once InvertIf
                if(resultField != null)
                {
                    element.Add(resultField);
                    // we can not clear the original context menu which will incorrectly copy the whole property, rather than an element
                    resultField.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        evt.menu.AppendAction("Copy Element Property Path",
                            _ => EditorGUIUtility.systemCopyBuffer = prop.propertyPath);

                        // bool spearator = false;
                        if (ClipboardHelper.CanCopySerializedProperty(prop.propertyType))
                        {
                            // spearator = true;
                            // evt.menu.AppendSeparator();
                            evt.menu.AppendAction("Copy Element", _ => ClipboardHelper.DoCopySerializedProperty(prop));
                        }

                        (bool hasReflectionPaste, bool hasValuePaste) =
                            ClipboardHelper.CanPasteSerializedProperty(prop.propertyType);

                        // ReSharper disable once InvertIf
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
                            property.DeleteArrayElementAtIndex(propIndex);
                            property.serializedObject.ApplyModifiedProperties();
                        });

                        evt.menu.AppendSeparator();
                    }));
                }
            }

            ListView listView = new ListView(Enumerable.Range(0, property.arraySize).ToList())
            {
                makeItem = MakeItem,
                bindItem = BindItem,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                showBoundCollectionSize = false,
                showFoldoutHeader = true,
                headerTitle = property.displayName,
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
                viewDataKey = property.propertyPath,
                // bindingPath = property.propertyPath,
            };

            Foldout foldoutElement = listView.Q<Foldout>();

            UIToolkitUtils.AddContextualMenuManipulator(foldoutElement, property, () => {});
            Toggle toggle = foldoutElement.Q<Toggle>();
            if (toggle != null && toggle.style.marginLeft != -12)
            {
                toggle.style.marginLeft = -12;
            }

            VisualElement foldoutContent = foldoutElement.Q<VisualElement>(className: "unity-foldout__content");

            VisualElement preContent = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    display = (listDrawerSettingsAttribute.Searchable || listDrawerSettingsAttribute.NumberOfItemsPerPage > 0)
                        ? DisplayStyle.Flex
                        :DisplayStyle.None,
                },
            };

            #region Search

            ToolbarSearchField searchField = new ToolbarSearchField
            {
                style =
                {
                    visibility = listDrawerSettingsAttribute.Searchable? Visibility.Visible :Visibility.Hidden,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                // ReSharper disable once InvertIf
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
            UIToolkitUtils.KeepRotate(loadingImage);
            loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingImage));

            preContent.Add(searchField);

            #endregion

            #region Paging

            VisualElement pagingContainer = new VisualElement
            {
                style =
                {
                    // visibility = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0? Visibility.Hidden: Visibility.Visible,
                    display = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0? DisplayStyle.None: DisplayStyle.Flex,
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };

            IntegerField numberOfItemsPerPageField = new IntegerField
            {
                isDelayed = true,
                style =
                {
                    minWidth = 30,
                },
            };
            TextElement numberOfItemsPerPageFieldTextElement = numberOfItemsPerPageField.Q<TextElement>();
            if(numberOfItemsPerPageFieldTextElement != null)
            {
                numberOfItemsPerPageFieldTextElement.style.unityTextAlign = TextAnchor.MiddleRight;
            }
            Label numberOfItemsSep = new Label("/")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };

            IntegerField numberOfItemsTotalField = new IntegerField
            {
                isDelayed = true,
                style =
                {
                    minWidth = 30,
                },
                value = property.arraySize,
            };

            Label numberOfItemsDesc = new Label("Items")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };


            Button pagePreButton = new Button
            {
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown-left.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            IntegerField pageField = new IntegerField
            {
                isDelayed = true,
                value = 1,
                style =
                {
                    minWidth = 30,
                },
            };
            TextElement pageFieldTextElement = pageField.Q<TextElement>();
            if(pageFieldTextElement != null)
            {
                pageFieldTextElement.style.unityTextAlign = TextAnchor.MiddleRight;
            }
            Label pageLabel = new Label(" / 1")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };
            Button pageNextButton = new Button
            {
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown-right.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
                // text = ">",
            };

            Button listViewAddButton = listView.Q<Button>("unity-list-view__add-button");
            Button listViewRemoveButton = listView.Q<Button>("unity-list-view__remove-button");

            void UpdatePage(int newPageIndex, int numberOfItemsPerPage)
            {
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
                    _asyncSearchItems.SourceGenerator = SearchCallback(property, searchText).GetEnumerator();

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
                Debug.Log($"set items: {string.Join(", ", curPageItems)}, itemIndexToPropertyIndex={string.Join(",", itemIndexToPropertyIndex)}");
#endif
                if(!listView.itemsSource.Cast<int>().SequenceEqual(curPageItems))
                {
                    listView.itemsSource = curPageItems;
                    listView.Rebuild();
                }
                // Debug.Log("rebuild listView");

                // UpdateAddRemoveButtons();
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

                // Debug.Log($"size check {arraySize}/{newSize}");

                if (newSize == arraySize)
                {
                    return;
                }

                arraySize = newSize;
                numberOfItemsTotalField.SetValueWithoutNotify(arraySize);
                UpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
            }

            // result.TrackPropertyValue(property, p =>
            // listView.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
            listView.TrackPropertyValue(property, _ =>
            {
                CheckArraySizeChange();
            });

            listView.schedule.Execute(CheckArraySizeChange).StartingIn(500);

            searchField.RegisterValueChangedCallback(_ =>
            {
                UpdatePage(0, numberOfItemsPerPageField.value);
            });

            pagePreButton.clicked += () =>
            {
                UpdatePage(_asyncSearchItems.CurPageIndex - 1, numberOfItemsPerPageField.value);
            };
            pageNextButton.clicked += () =>
            {
                UpdatePage(_asyncSearchItems.CurPageIndex + 1, numberOfItemsPerPageField.value);
            };
            pageField.RegisterValueChangedCallback(evt => UpdatePage(evt.newValue - 1, numberOfItemsPerPageField.value));
            numberOfItemsTotalField.RegisterValueChangedCallback(e =>
            {
                int min = -1;
                int max = -1;
                if (arraySizeAttribute != null)
                {
                    (string error, bool dynamic, int min, int max) getArraySize = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, property, FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
                    if(getArraySize.error == "")
                    {
                        min = getArraySize.min;
                        max = getArraySize.max;
                    }
                }

                int newSize = e.newValue;

                if(min > 0 && newSize < min)
                {
                    newSize = min;
                }
                else if(max > 0 && newSize > max)
                {
                    newSize = max;
                }

                if(property.arraySize != newSize)
                {
                    property.arraySize = newSize;
                    property.serializedObject.ApplyModifiedProperties();
                    UpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                }
                else
                {
                    numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
                }
            });

            void UpdateNumberOfItemsPerPage(int newValue)
            {
                int newValueClamp = Mathf.Max(newValue, 0);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"update number of items per page {newValueClamp}");
#endif
                UpdatePage(_asyncSearchItems.CurPageIndex, newValueClamp);
            }

            numberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            listViewAddButton.clickable = new Clickable(() =>
            {
                property.arraySize += 1;
                property.serializedObject.ApplyModifiedProperties();
                int totalVisiblePage = Mathf.CeilToInt((float)_asyncSearchItems.ItemIndexToPropertyIndex.Count / numberOfItemsPerPageField.value);
                UpdatePage(totalVisiblePage - 1, numberOfItemsPerPageField.value);
                // numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";
                numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
            });

            listView.itemsRemoved += objects =>
            {
                // int[] sources = listView.itemsSource.Cast<int>().ToArray();
                List<int> curRemoveObjects = objects.ToList();

                foreach (int index in curRemoveObjects.Select(removeIndex => _asyncSearchItems.ItemIndexToPropertyIndex[removeIndex]).OrderByDescending(each => each))
                {
                    // Debug.Log(index);
                    property.DeleteArrayElementAtIndex(index);
                }

                // itemIndexToPropertyIndex.RemoveAll(each => curRemoveObjects.Contains(each));
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                // numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";
                numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"removed update page to {curPageIndex}");
#endif

                listView.schedule.Execute(() => UpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value));
            };

            if (listDrawerSettingsAttribute.NumberOfItemsPerPage != 0)
            {
                // preContent.style.display = DisplayStyle.Flex;
                // pagingContainer.style.visibility = Visibility.Visible;

                listView.RegisterCallback<AttachToPanelEvent>(_ =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"init update numberOfItemsPerPage={listDrawerSettingsAttribute.NumberOfItemsPerPage}");
#endif
                    numberOfItemsPerPageField.value = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                });
            }

            pagingContainer.Add(numberOfItemsPerPageField);
            pagingContainer.Add(numberOfItemsSep);
            if(listDrawerSettingsAttribute.NumberOfItemsPerPage > 0)
            {
                pagingContainer.Add(numberOfItemsTotalField);
            }
            else
            {
                numberOfItemsTotalField.style.position = Position.Absolute;
                numberOfItemsTotalField.style.right = 2;
                numberOfItemsTotalField.style.top = 1;
                numberOfItemsTotalField.style.minWidth = 50;
            }
            pagingContainer.Add(numberOfItemsDesc);

            pagingContainer.Add(pagePreButton);
            pagingContainer.Add(pageField);
            pagingContainer.Add(pageLabel);
            pagingContainer.Add(pageNextButton);

            preContent.Add(pagingContainer);

            #endregion

            #region Drag
            VisualElement foldoutInput = foldoutElement.Q<VisualElement>(classes: "unity-foldout__input");

            // Type elementType =
            //     ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
            //                                 FieldWithInfo.PropertyInfo.PropertyType);
            foldoutInput.RegisterCallback<DragEnterEvent>(_ =>
            {
                // Debug.Log($"Drag Enter {evt}");
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            foldoutInput.RegisterCallback<DragLeaveEvent>(_ =>
            {
                // Debug.Log($"Drag Leave {evt}");
                DragAndDrop.visualMode = DragAndDropVisualMode.None;
            });
            foldoutInput.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                // Debug.Log($"Drag Update {evt}");
                // DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            foldoutInput.RegisterCallback<DragPerformEvent>(_ =>
            {
                // Debug.Log($"Drag Perform {evt}");
                if (!DropUIToolkit(elementType, property))
                {
                    return;
                }

                property.serializedObject.ApplyModifiedProperties();
                UpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
            });
            #endregion

            listView.itemIndexChanged += (first, second) =>
            {
                int fromPropIndex = _asyncSearchItems.ItemIndexToPropertyIndex[first];
                int toPropIndex = _asyncSearchItems.ItemIndexToPropertyIndex[second];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"drag {fromPropIndex}({first}) -> {toPropIndex}({second})");
#endif

                property.MoveArrayElement(fromPropIndex, toPropIndex);
                property.serializedObject.ApplyModifiedProperties();
            };

            listView.RegisterCallback<KeyDownEvent>(evt =>
            {
                // ReSharper disable once MergeIntoLogicalPattern
                bool ctrl = evt.modifiers == EventModifiers.Control ||
                            evt.modifiers == EventModifiers.Command;

                bool copyCommand = ctrl && evt.keyCode == KeyCode.C;
                if (copyCommand)
                {
                    int selectedIndex = listView.selectedItems
                        .Cast<int>()
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
                    int selectedIndex = listView.selectedItems
                        .Cast<int>()
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
                    // Debug.Log($"{pasteHasReflection}, {pasteHasValue}");
                    if (pasteHasReflection && pasteHasValue)
                    {
                        ClipboardHelper.DoPasteSerializedProperty(prop);
                        prop.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            foldoutContent.Insert(0, preContent);

            listView.schedule.Execute(() =>
            {
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
                        UpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
                    }

                    if (_asyncSearchItems.SourceGenerator.MoveNext())
                    {
                        IReadOnlyList<int> currentValue = _asyncSearchItems.SourceGenerator.Current;

                        // ReSharper disable once MergeIntoPattern
                        if(currentValue != null && currentValue.Count > 0)
                        {
                            _asyncSearchItems.FullSources.AddRange(currentValue);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"#Search# add search results {string.Join(", ", currentValue)}");
#endif
                            UpdatePage(_asyncSearchItems.CurPageIndex, numberOfItemsPerPageField.value);
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

            // UpdateAddRemoveButtons();
            root.Add(listView);

            if (listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0)
            {
                root.Add(numberOfItemsTotalField);
            }

            OnSearchFieldUIToolkit.AddListener(Search);
            root.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));

            return (root, listViewAddButton, listViewRemoveButton);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(FieldWithInfo.SerializedProperty.displayName, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (root.style.display != display)
                {
                    root.style.display = display;
                }
            }
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);

            (int minSize, int maxSize) = result.ArraySize;

            int curSize = FieldWithInfo.SerializedProperty.arraySize;
            bool canNotAddMore = maxSize >= 0 && curSize >= maxSize;
            _addButton.SetEnabled(!canNotAddMore);
            bool canNotRemoveMore = minSize >= 0 && curSize <= minSize;
            _removeButton.SetEnabled(!canNotRemoveMore);

            return result;
        }

        #region ShowInInspector

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
            bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
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
                                    if (!SearchObject(item, token.Token, searchedObject))
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
                                    if (!SearchObject(itemProp, token.Token, searchedObjects))
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
                VisualElement item = UIToolkitValueEdit(
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
                    targets).result;
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

            if (!listDrawerSettingsAttribute.Searchable)
            {
                listViewWrapper.SearchPager.SearchContainer.style.display = DisplayStyle.None;
                listViewWrapper.SearchPager.PagingContainer.style.flexGrow = 1;
            }

            if (listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0)
            {
                listViewWrapper.SearchPager.PagingContainer.style.display = DisplayStyle.None;
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
            foreach (int i in SearchArrayObjects(payloadRawValues, search))
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

        // Note: This WILL contain -1 for the sake of async searching...
        private static IEnumerable<int> SearchArrayObjects(List<object> payloadRawValues, string searchFull)
        {
            IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(searchFull).ToArray();
            for (int arrayElementIndex = 0; arrayElementIndex < payloadRawValues.Count; arrayElementIndex++)
            {
                object childObject = payloadRawValues[arrayElementIndex];
                bool all = true;
                HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                    .Select(_ => new HashSet<object>())
                    .ToArray();
                for (int tokenIndex = 0; tokenIndex < searchTokens.Count; tokenIndex++)
                {
                    ListSearchToken search = searchTokens[tokenIndex];
                    HashSet<object> searchedObjects = searchedObjectsArray[tokenIndex];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SEARCH
                    Debug.Log($"#Search# searching token@{tokenIndex}={search.Token} of property={property.name}@{arrayElementIndex} with seachedObjects={string.Join(",", searchedObjects)}");
#endif
                    if (!SearchObject(childObject, search.Token, searchedObjects))
                    {
                        all = false;
                        break;
                    }
                }

                if (all)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"found: {childProperty.propertyPath}");
#endif
                    yield return arrayElementIndex;
                }
                else
                {
                    yield return -1;
                }
            }
        }

        private static bool SearchObject(object childObject, string rawToken, HashSet<object> searchedObjects)
        {
            if (RuntimeUtil.IsNull(childObject))
            {
                return false;
            }

            if(!searchedObjects.Add(childObject))
            {
                return false;
            }

            string token = rawToken.ToLower();

            Type childType = childObject.GetType();
            // treat primitive-like types as leaf nodes: match against their string representation
            if (childType.IsPrimitive || childObject is string || childType.IsEnum || childType == typeof(decimal))
            {
                return childObject.ToString().Contains(token);
            }

            if (childObject is UnityEngine.Object uObject)
            {
                if (uObject is GameObject go)
                {
                    return go.name.Contains(token);
                }
                return SerializedUtils.SearchUnityObjectProp(uObject, rawToken, searchedObjects);
            }

            if (childObject is IEnumerable ie)
            {
                return ie.Cast<object>().Any(each => SearchObject(each, rawToken, searchedObjects));
            }

            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            FieldInfo[] fieldTargets = childType.GetFields(bindAttrNormal);
            foreach (FieldInfo fieldInfo in fieldTargets)
            {
                if (SkipTypeDrawing(fieldInfo.FieldType))
                {
                    continue;
                }

                object fieldValue;
                try
                {
                    fieldValue = fieldInfo.GetValue(childObject);
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    continue;
                }

                if (SearchObject(fieldValue, rawToken, searchedObjects))
                {
                    return true;
                }
            }
            PropertyInfo[] propertyTargets = childType.GetProperties(bindAttrNormal);
            foreach (PropertyInfo propertyInfo in propertyTargets)
            {
                if (SkipTypeDrawing(propertyInfo.PropertyType))
                {
                    continue;
                }

                object propertyValue;
                try
                {
                    propertyValue = propertyInfo.GetValue(childObject);
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    continue;
                }

                if (SearchObject(propertyValue, rawToken, searchedObjects))
                {
                    return true;
                }
            }

            return false;
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

        #endregion
    }
}
#endif
