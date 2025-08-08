#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
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
            };

            // int numberOfItemsPerPage = 0;
            int curPageIndex = 0;
            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, property.arraySize).ToList();

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
                if(index >= itemIndexToPropertyIndex.Count)
                {
                    return;
                }

                int propIndex = itemIndexToPropertyIndex[index];
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

                itemIndexToPropertyIndex.Clear();
                itemIndexToPropertyIndex.AddRange(pagingInfo.IndexesCurPage);

                curPageIndex = pagingInfo.CurPageIndex;

                pageLabel.text = $" / {pagingInfo.PageCount}";
                pageField.SetValueWithoutNotify(curPageIndex + 1);

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
                UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
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
                UpdatePage(curPageIndex - 1, numberOfItemsPerPageField.value);
            };
            pageNextButton.clicked += () =>
            {
                UpdatePage(curPageIndex + 1, numberOfItemsPerPageField.value);
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
                    UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
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
                UpdatePage(curPageIndex, newValueClamp);
            }

            numberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            listViewAddButton.clickable = new Clickable(() =>
            {
                property.arraySize += 1;
                property.serializedObject.ApplyModifiedProperties();
                int totalVisiblePage = Mathf.CeilToInt((float)itemIndexToPropertyIndex.Count / numberOfItemsPerPageField.value);
                UpdatePage(totalVisiblePage - 1, numberOfItemsPerPageField.value);
                // numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";
                numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
            });

            listView.itemsRemoved += objects =>
            {
                // int[] sources = listView.itemsSource.Cast<int>().ToArray();
                List<int> curRemoveObjects = objects.ToList();

                foreach (int index in curRemoveObjects.Select(removeIndex => itemIndexToPropertyIndex[removeIndex]).OrderByDescending(each => each))
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

                listView.schedule.Execute(() => UpdatePage(curPageIndex, numberOfItemsPerPageField.value));
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
                UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
            });
            #endregion

            listView.itemIndexChanged += (first, second) =>
            {
                int fromPropIndex = itemIndexToPropertyIndex[first];
                int toPropIndex = itemIndexToPropertyIndex[second];
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

                    int propIndex = itemIndexToPropertyIndex[selectedIndex];
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

                    int propIndex = itemIndexToPropertyIndex[selectedIndex];
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
                        UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
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
                            UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
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
    }
}
#endif
