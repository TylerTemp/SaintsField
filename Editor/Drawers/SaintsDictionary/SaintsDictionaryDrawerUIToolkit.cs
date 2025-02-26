#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     return new TextElement()
        //     {
        //         text = "Hi"
        //     };
        // }

        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictionary_Foldout";
        private static string NameTotalCount(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_TotalCount";

        private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_ListView";
        private static string NameTotalCountBottom(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_TotalCountBottom";

        private static string NamePagePreButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictinoary_PagePreButton";

        private static string NamePage(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictinoary_Page";

        private static string NamePageLabel(SerializedProperty property) =>
            $"{property.propertyPath}____SaintsDictionary_PageLabel";
        private static string NamePageNextButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictinoary_PageNextButton";

        private static string NameAddButton(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_RemoveButton";

        private static string NameNumberOfItemsPerPage(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictionary_NameNumberOfItemsPerPage";

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsDictionaryAttribute saintsDictionaryAttribute = saintsAttribute as SaintsDictionaryAttribute;

            VisualElement root = new VisualElement
            {
                style =
                {
                    position = Position.Relative,
                },
            };
            VisualElement foldout = new Foldout
            {
                text = property.displayName,
                value = property.isExpanded,
                name = NameFoldout(property),
            };

            VisualElement content = foldout.Q<VisualElement>(className: "unity-foldout__content");
            // Debug.Log(content);
            if (content != null)
            {
                content.style.marginLeft = 0;
            }

            root.Add(foldout);

            // (string propKeysName, string propValuesName) = GetKeysValuesPropName(info.FieldType);

            // SerializedProperty keysProp = property.FindPropertyRelative(propKeysName);
            // SerializedProperty valuesProp = property.FindPropertyRelative(propValuesName);

            IntegerField totalCount = new IntegerField
            {
                // value = keysProp.arraySize,
                label = "",
                isDelayed = true,
                style =
                {
                    minWidth = 50,
                    position = Position.Absolute,
                    top = 1,
                    right = 1,
                },
                name = NameTotalCount(property),
            };
            root.Add(totalCount);

            MultiColumnListView multiColumnListView = new MultiColumnListView
            {
                // showBoundCollectionSize = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,

                name = NameListView(property),

                // this has some issue because we bind order with renderer. Sort is not possible
// #if UNITY_6000_0_OR_NEWER
//                 sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
// #endif
            };

            foldout.Add(multiColumnListView);

            // footer: add/remove
            VisualElement footerButtons = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginRight = 5,
                },
            };

            int numberOfItemsPerPage = saintsDictionaryAttribute?.NumberOfItemsPerPage ?? -1;

            IntegerField numberOfItemsPerPageField = new IntegerField
            {
                value = numberOfItemsPerPage,
                isDelayed = true,
                style =
                {
                    minWidth = 30,
                },
                name = NameNumberOfItemsPerPage(property),
            };
            if(numberOfItemsPerPage > 0)
            {
                TextElement numberOfItemsPerPageFieldTextElement = numberOfItemsPerPageField.Q<TextElement>();
                if (numberOfItemsPerPageFieldTextElement != null)
                {
                    numberOfItemsPerPageFieldTextElement.style.unityTextAlign = TextAnchor.MiddleRight;
                }
            }
            else
            {
                numberOfItemsPerPageField.style.display = DisplayStyle.None;
            }

            footerButtons.Add(numberOfItemsPerPageField);

            Label numberOfItemsSep = new Label("/")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            footerButtons.Add(numberOfItemsSep);
            IntegerField totalCountBottomField = new IntegerField
            {
                isDelayed = true,
                style =
                {
                    minWidth = 30,
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
                },
                name = NameTotalCountBottom(property),
                // value = property.arraySize,
            };
            footerButtons.Add(totalCountBottomField);

            Label numberOfItemsDesc = new Label("Items")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            footerButtons.Add(numberOfItemsDesc);

            Button pagePreButton = new Button
            {
                style =
                {
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
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
                name = NamePagePreButton(property),
            };
            footerButtons.Add(pagePreButton);

            IntegerField pageField = new IntegerField
            {
                isDelayed = true,
                value = 1,
                style =
                {
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
                    minWidth = 30,
                },
                name = NamePage(property),
            };
            TextElement pageFieldTextElement = pageField.Q<TextElement>();
            if(pageFieldTextElement != null)
            {
                pageFieldTextElement.style.unityTextAlign = TextAnchor.MiddleRight;
            }
            footerButtons.Add(pageField);
            Label pageLabel = new Label(" / 1")
            {
                style =
                {
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
                name = NamePageLabel(property),
            };
            footerButtons.Add(pageLabel);
            Button pageNextButton = new Button
            {
                style =
                {
                    display = numberOfItemsPerPage > 0? DisplayStyle.Flex: DisplayStyle.None,
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
                name = NamePageNextButton(property),
            };
            footerButtons.Add(pageNextButton);

            footerButtons.Add(new Button
            {
                text = "+",
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NameAddButton(property),
            });
            footerButtons.Add(new Button
            {
                text = "-",
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NameRemoveButton(property),
            });

            foldout.Add(footerButtons);

            // root.Add(multiColumnListView);
            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Foldout foldout = container.Q<Foldout>(name: NameFoldout(property));
            foldout.RegisterValueChangedCallback(newValue => property.isExpanded = newValue.newValue);

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            Type rawType = arrayIndex == -1 ? info.FieldType : info.FieldType.GetElementType();
            Debug.Assert(rawType != null, $"Failed to get element type from {property.propertyPath}");
            // Debug.Log(info.FieldType);
            (string propKeysName, string propValuesName) = GetKeysValuesPropName(rawType);

            // Debug.Log(propKeysName);

            SerializedProperty keysProp = property.FindPropertyRelative(propKeysName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propKeysName);
            SerializedProperty valuesProp = property.FindPropertyRelative(propValuesName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propValuesName);

            FieldInfo keysField = rawType.GetField(propKeysName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance  | BindingFlags.FlattenHierarchy);
            Debug.Assert(keysField != null, $"Failed to get keys field from {property.propertyPath}");

            IntegerField totalCountFieldTop = container.Q<IntegerField>(name: NameTotalCount(property));
            totalCountFieldTop.SetValueWithoutNotify(keysProp.arraySize);
            IntegerField totalCountBottomField = container.Q<IntegerField>(name: NameTotalCountBottom(property));
            totalCountBottomField.SetValueWithoutNotify(keysProp.arraySize);

            void ManuallySetSize(int size)
            {
                int newSize = Mathf.Max(size, 0);
                if (newSize >= keysProp.arraySize)
                {
                    if (IncreaseArraySize(newSize, keysProp, valuesProp))
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    IReadOnlyList<int> deleteIndexes = Enumerable.Range(newSize, keysProp.arraySize - newSize)
                        .Reverse()
                        .ToList();
                    DecreaseArraySize(deleteIndexes, keysProp, valuesProp);
                    property.serializedObject.ApplyModifiedProperties();
                }
                totalCountFieldTop.SetValueWithoutNotify(newSize);
                totalCountBottomField.SetValueWithoutNotify(newSize);
            }

            totalCountFieldTop.TrackPropertyValue(keysProp, _ =>
            {
                totalCountFieldTop.SetValueWithoutNotify(keysProp.arraySize);
            });
            totalCountFieldTop.RegisterValueChangedCallback(evt => ManuallySetSize(evt.newValue));

            totalCountBottomField.TrackPropertyValue(keysProp, _ =>
            {
                totalCountBottomField.SetValueWithoutNotify(keysProp.arraySize);
            });
            totalCountBottomField.RegisterValueChangedCallback(evt => ManuallySetSize(evt.newValue));

            Label pageLabel = container.Q<Label>(name: NamePageLabel(property));
            Button pagePreButton = container.Q<Button>(name: NamePagePreButton(property));
            IntegerField pageField = container.Q<IntegerField>(name: NamePage(property));
            Button pageNextButton = container.Q<Button>(name: NamePageNextButton(property));

            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, keysProp.arraySize).ToList();

            MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>(name: NameListView(property));

            #region Paging & Search

            string preKeySearch = "";
            string preValueSearch = "";
            int prePageIndex = 0;
            int preSize = 0;
            // int preNumberOfItemsPerPage = -1;
            int preTotalPage = 1;

            SaintsDictionaryAttribute saintsDictionaryAttribute = saintsAttribute as SaintsDictionaryAttribute;
            // int numberOfItemsPerPage = saintsDictionaryAttribute?.NumberOfItemsPerPage ?? -1;
            List<int> hitTargetIndexes = new List<int>(itemIndexToPropertyIndex);

            void RefreshList(string keySearch, string valueSearch, int curPageIndex, int numberOfItemsPerPage)
            {
                // bool needRebuild = false;
                int nowArraySize = keysProp.arraySize;

                List<int> fullList = Enumerable.Range(0, nowArraySize).ToList();
                // List<int> useIndexes = new List<int>(itemIndexToPropertyIndex);
                List<int> refreshedHitTargetIndexes = new List<int>(hitTargetIndexes);
                // if (forceRebuild)
                // {
                //     refreshedHitTargetIndexes = Enumerable.Range(0, nowArraySize).ToList();
                // }
                if (nowArraySize != preSize)
                {
                    preSize = nowArraySize;
                    // needRebuild = true;
                    refreshedHitTargetIndexes = fullList;
                }

                // processing search result
                bool needSearchAgain = false;
                // if (needRebuild)
                // {
                //     needSearchAgain = true;
                // }
                // else
                {
                    if (preKeySearch != keySearch)
                    {
                        preKeySearch = keySearch;
                        needSearchAgain = true;
                    }

                    if (preValueSearch != valueSearch)
                    {
                        preValueSearch = valueSearch;
                        needSearchAgain = true;
                    }
                }

                if (needSearchAgain)
                {
                    // needRebuild = true;
                    refreshedHitTargetIndexes = Search(keysProp, valuesProp, keySearch, valueSearch);
                    // Debug.Log($"hit search {keySearch}/{valueSearch}: {string.Join(",", refreshedHitTargetIndexes)}");
                }

                hitTargetIndexes = refreshedHitTargetIndexes;
                // preNumberOfItemsPerPage = numberOfItemsPerPage;
                // paging
                if (numberOfItemsPerPage > 0)
                {
                    int startIndex = curPageIndex * numberOfItemsPerPage;
                    if (startIndex >= hitTargetIndexes.Count)
                    {
                        startIndex = 0;
                        curPageIndex = 0;
                    }
                    int endIndex = Mathf.Min((curPageIndex + 1) * numberOfItemsPerPage, hitTargetIndexes.Count);
                    itemIndexToPropertyIndex = hitTargetIndexes.GetRange(startIndex, endIndex - startIndex);
                    int totalPage = Mathf.Max(1, Mathf.CeilToInt(hitTargetIndexes.Count / (float)numberOfItemsPerPage));

                    // pageField.SetValueWithoutNotify(curPageIndex + 1);


                    // needRebuild = preNumberOfItemsPerPage != numberOfItemsPerPage
                    //               || preTotalPage != totalPage
                    //               || prePageIndex != curPageIndex;

                    // preNumberOfItemsPerPage = numberOfItemsPerPage;
                    preTotalPage = totalPage;
                    prePageIndex = curPageIndex;
                }
                else
                {
                    itemIndexToPropertyIndex = hitTargetIndexes;
                }

                // if (needRebuild)
                {
                    multiColumnListView.itemsSource = itemIndexToPropertyIndex.ToList();
                    multiColumnListView.Rebuild();
                    pagePreButton.SetEnabled(prePageIndex > 0);
                    pageField.SetValueWithoutNotify(prePageIndex + 1);
                    pageLabel.text = $"/ {preTotalPage}";
                    pageNextButton.SetEnabled(prePageIndex + 1 < preTotalPage);
                }
            }

            #endregion

            IntegerField numberOfItemsPerPage = container.Q<IntegerField>(name: NameNumberOfItemsPerPage(property));
            numberOfItemsPerPage.RegisterValueChangedCallback(evt =>
            {
                RefreshList(preKeySearch, preValueSearch, prePageIndex, evt.newValue);
            });

            bool keyStructChecked = false;
            bool keyStructNeedFlatten = false;

            bool valueStructChecked = false;
            bool valueStructNeedFlatten = false;

            multiColumnListView.columns.Add(new Column
            {
                name = "Keys",
                // title = "Keys",
                stretchable = true,
                makeHeader = () =>
                {
                    VisualElement header = new VisualElement();
                    string keyLabel = GetKeyLabel(saintsDictionaryAttribute);
                    if(!string.IsNullOrEmpty(keyLabel))
                    {
                        header.Add(new Label(keyLabel));
                    }
                    ToolbarSearchField keySearch = new ToolbarSearchField
                    {
                        // isDelayed = true,
                        style =
                        {
                            marginRight = 3,
                            display = saintsDictionaryAttribute?.Searchable ?? true
                                ? DisplayStyle.Flex
                                : DisplayStyle.None,
                            width = Length.Percent(97f),
                        },
                    };
                    TextField keySearchText = keySearch.Q<TextField>();
                    if (keySearchText != null)
                    {
                        keySearchText.isDelayed = true;
                    }
                    header.Add(keySearch);
                    keySearch.RegisterValueChangedCallback(evt =>
                    {
                        // Debug.Log($"key search {evt.newValue}");
                        if(evt.newValue != preKeySearch)
                        {
                            RefreshList(evt.newValue, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
                        }
                    });
                    return header;
                },
                makeCell = () => new VisualElement(),
                bindCell = (element, elementIndex) =>
                {
                    int propIndex = itemIndexToPropertyIndex[elementIndex];
                    SerializedProperty elementProp = keysProp.GetArrayElementAtIndex(propIndex);

                    if (!keyStructChecked)
                    {
                        keyStructChecked = true;
                        keyStructNeedFlatten = GetNeedFlatten(elementProp);
                    }

                    elementProp.isExpanded = true;
                    element.Clear();

                    VisualElement keyContainer = new VisualElement
                    {
                        style =
                        {
                            marginRight = 3,
                        },
                    };
                    element.Add(keyContainer);
                    if (keyStructNeedFlatten)
                    {
                        foreach (SerializedProperty childProp in SerializedUtils.GetPropertyChildren(elementProp).Where(each => each != null))
                        {
                            keyContainer.Add(new Label(childProp.displayName));
                            PropertyField propertyField = new PropertyField(childProp)
                            {
                                label = "",
                            };
                            propertyField.Bind(property.serializedObject);
                            keyContainer.Add(propertyField);
                        }
                    }
                    else
                    {
                        PropertyField propertyField = new PropertyField(elementProp)
                        {
                            label = "",
                        };
                        propertyField.Bind(property.serializedObject);
                        keyContainer.Add(propertyField);
                    }

                    // propertyField.BindProperty(elementProp);

                    void RefreshConflict()
                    {
                        if (propIndex >= keysProp.arraySize)
                        {
                            return;
                        }

                        (string curFieldError, int _, object curFieldVaue) = Util.GetValue(property, info, parent);
                        // Debug.Log(curFieldVaue);
                        if (curFieldError != "")
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogError(curFieldError);
#endif
                            return;
                        }

                        IEnumerable allKeyList = keysField.GetValue(curFieldVaue) as IEnumerable;
                        Debug.Assert(allKeyList != null, $"key list {keysField.Name} is null");
                        (object value, int index)[] indexedValue = allKeyList.Cast<object>().WithIndex().ToArray();
                        object thisKey = indexedValue[propIndex].value;
                        // Debug.Log($"checking with {thisKey}");
                        foreach ((object existKey, int _) in indexedValue.Where(each => each.index != propIndex))
                        {
                            // Debug.Log($"{existKey}/{thisKey}");
                            // ReSharper disable once InvertIf
                            if (Util.GetIsEqual(existKey, thisKey))
                            {
                                keyContainer.style.backgroundColor = WarningColor;
                                return;
                            }
                        }

                        keyContainer.style.backgroundColor = Color.clear;
                    }

                    // propertyField.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
                    // {
                    //     RefreshConflict();
                    // });
                    keyContainer.TrackPropertyValue(keysProp, _ =>
                    {
                        RefreshConflict();
                    });
                    // Debug.Log($"RefreshConflict {elementIndex}");
                    RefreshConflict();
                },
            });

            multiColumnListView.columns.Add(new Column
            {
                name = "Values",
                // title = "Values",
                stretchable = true,
                makeHeader = () =>
                {
                    VisualElement header = new VisualElement();

                    string valueLabel = GetValueLabel(saintsDictionaryAttribute);

                    if(!string.IsNullOrEmpty(valueLabel))
                    {
                        header.Add(new Label(valueLabel));
                    }

                    // header.Add(new Label("Values"));
                    ToolbarSearchField valueSearch = new ToolbarSearchField
                    {
                        // isDelayed = true,
                        style =
                        {
                            marginRight = 3,
                            display = saintsDictionaryAttribute?.Searchable ?? true
                                ? DisplayStyle.Flex
                                : DisplayStyle.None,
                            width = Length.Percent(97f),
                        },
                    };
                    TextField valueSearchText = valueSearch.Q<TextField>();
                    if (valueSearchText != null)
                    {
                        valueSearchText.isDelayed = true;
                    }
                    header.Add(valueSearch);
                    valueSearch.RegisterValueChangedCallback(evt =>
                    {
                        // Debug.Log($"value search {evt.newValue}");
                        if(evt.newValue != preValueSearch)
                        {
                            RefreshList(preKeySearch, evt.newValue, prePageIndex, numberOfItemsPerPage.value);
                        }
                    });
                    return header;
                },
                makeCell = () => new VisualElement(),
                bindCell = (element, elementIndex) =>
                {
                    SerializedProperty elementProp = valuesProp.GetArrayElementAtIndex(itemIndexToPropertyIndex[elementIndex]);
                    elementProp.isExpanded = true;

                    if (!valueStructChecked)
                    {
                        valueStructChecked = true;
                        valueStructNeedFlatten = GetNeedFlatten(elementProp);
                    }

                    VisualElement valueContainer = new VisualElement
                    {
                        style =
                        {
                            marginRight = 3,
                        },
                    };
                    element.Add(valueContainer);
                    if (valueStructNeedFlatten)
                    {
                        foreach (SerializedProperty childProp in SerializedUtils.GetPropertyChildren(elementProp).Where(each => each != null))
                        {
                            valueContainer.Add(new Label(childProp.displayName));
                            PropertyField propertyField = new PropertyField(childProp)
                            {
                                label = "",
                            };
                            propertyField.Bind(property.serializedObject);
                            valueContainer.Add(propertyField);
                        }
                    }
                    else
                    {
                        PropertyField propertyField = new PropertyField(elementProp)
                        {
                            label = "",
                        };
                        propertyField.Bind(property.serializedObject);
                        valueContainer.Add(propertyField);
                    }
                },
            });

            pagePreButton.clicked += () =>
            {
                RefreshList(preKeySearch, preValueSearch, Mathf.Max(0, prePageIndex - 1), numberOfItemsPerPage.value);
            };

            pageField.RegisterValueChangedCallback(evt =>
            {
                RefreshList(preKeySearch, preValueSearch, Mathf.Clamp(evt.newValue - 1, 0, preTotalPage - 1), numberOfItemsPerPage.value);
            });

            pageNextButton.clicked += () =>
            {
                RefreshList(preKeySearch, preValueSearch, Mathf.Min(prePageIndex + 1, preTotalPage - 1), numberOfItemsPerPage.value);
            };

            Button addButton = container.Q<Button>(name: NameAddButton(property));
            addButton.clicked += () =>
            {
                IncreaseArraySize(keysProp.arraySize + 1, keysProp, valuesProp);
                property.serializedObject.ApplyModifiedProperties();
                //
                RefreshList(preKeySearch, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
                // multiColumnListView.Rebuild();
            };
            Button deleteButton = container.Q<Button>(name: NameRemoveButton(property));
            deleteButton.clicked += () =>
            {
                // Debug.Log("Clicked");
                // property.serializedObject.Update();
                // var keysProp = property.FindPropertyRelative(propKeysName);
                // var valuesProp = property.FindPropertyRelative(propValuesName);

                List<int> selected = multiColumnListView.selectedIndices
                    .Select(oriIndex => itemIndexToPropertyIndex[oriIndex])
                    .OrderByDescending(each => each)
                    .ToList();

                if (selected.Count == 0)
                {
                    int curSize = keysProp.arraySize;
                    // Debug.Log($"curSize={curSize}");
                    if (curSize == 0)
                    {
                        return;
                    }
                    selected.Add(curSize - 1);
                }

                // Debug.Log($"delete {keysProp.propertyPath}/{keysProp.arraySize} key at {string.Join(",", selected)}");

                DecreaseArraySize(selected, keysProp, valuesProp);
                property.serializedObject.ApplyModifiedProperties();
                // keysProp.serializedObject.ApplyModifiedProperties();
                // valuesProp.serializedObject.ApplyModifiedProperties();
                // multiColumnListView.Rebuild();
                RefreshList(preKeySearch, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
                // multiColumnListView.Rebuild();
                // Debug.Log($"new size = {keysProp.arraySize}");
            };

            multiColumnListView.TrackPropertyValue(keysProp, _ =>
            {
                if (preSize != keysProp.arraySize)
                {
                    RefreshList(preKeySearch, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
                }
            });

            multiColumnListView.itemIndexChanged += (first, second) =>
            {
                // Debug.Log($"first={first}, second={second} | {string.Join(",", itemIndexToPropertyIndex)}");
                int fromPropIndex = itemIndexToPropertyIndex[first];
                int toPropIndex = itemIndexToPropertyIndex[second];

                // Debug.Log($"array {fromPropIndex} <-> {toPropIndex}");

                keysProp.MoveArrayElement(fromPropIndex, toPropIndex);
                valuesProp.MoveArrayElement(fromPropIndex, toPropIndex);

                ListSwapValue(itemIndexToPropertyIndex, fromPropIndex, toPropIndex);
                ListSwapValue(hitTargetIndexes, fromPropIndex, toPropIndex);

                property.serializedObject.ApplyModifiedProperties();
                multiColumnListView.itemsSource = itemIndexToPropertyIndex.ToList();
                // multiColumnListView.Rebuild();
            };

            // multiColumnListView.Rebuild();
            RefreshList(preKeySearch, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
            // Debug.Log($"{string.Join(",", itemIndexToPropertyIndex)}");
        }

        // private static List<int> MakeSource(SerializedProperty property)
        // {
        //     return Enumerable.Range(0, property.arraySize).ToList();
        // }
        private static void ListSwapValue(IList<int> lis, int a, int b)
        {
            int aIndex = lis.IndexOf(a);
            int bIndex = lis.IndexOf(b);

            (lis[aIndex], lis[bIndex]) = (lis[bIndex], lis[aIndex]);
        }
    }
}
#elif UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // Action<object> onValueChangedCallback = null;
            // onValueChangedCallback = value =>
            // {
            //     object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            //     if (newFetchParent == null)
            //     {
            //         Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
            //         return;
            //     }
            //
            //     foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            //     {
            //         saintsPropertyInfo.Drawer.OnValueChanged(
            //             property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement,
            //             info, newFetchParent,
            //             onValueChangedCallback,
            //             value);
            //     }
            // };

            IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);

            IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
            {
                GUIContent label = imguiLabelHelper.NoLabel
                    ? GUIContent.none
                    : new GUIContent(imguiLabelHelper.RichLabel);

                property.serializedObject.Update();

                using(new ImGuiFoldoutStyleRichTextScoop())
                using(new ImGuiLabelStyleRichTextScoop())
                // using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float height =
                        GetFieldHeight(property, label, Screen.width, saintsAttribute, info, true, parent);
                    Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));

                    DrawField(rect, property, label, saintsAttribute, allAttributes, new OnGUIPayload(), info, parent);
                    // ReSharper disable once InvertIf
                    // if (changed.changed)
                    // {
                    //     object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    //     if (newFetchParent == null)
                    //     {
                    //         Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    //         return;
                    //     }
                    //
                    //     (string error, int _, object value) = Util.GetValue(property, info, newFetchParent);
                    //     if (error == "")
                    //     {
                    //         onValueChangedCallback(value);
                    //     }
                    // }
                }

                property.serializedObject.ApplyModifiedProperties();
            })
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 0,
                },
                userData = imguiLabelHelper,
            };
            imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);

            return imGuiContainer;
        }
    }
}
#endif
