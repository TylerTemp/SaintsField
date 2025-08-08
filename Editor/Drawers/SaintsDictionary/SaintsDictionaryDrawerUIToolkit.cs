#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
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
            $"{property.propertyPath}__SaintsDictionary_Page";

        private static string NamePageLabel(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictionary_PageLabel";
        private static string NamePageNextButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictionary_PageNextButton";

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

            VisualElement foldout = new Foldout
            {
                text = GetPreferredLabel(property),
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
                // isDelayed = true,
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

        private static (FieldInfo targetInfo, object targetParent) GetTargetInfo(string propNameCompact, FieldInfo info, Type type, object parent)
        {
            object keysIterTarget = info.GetValue(parent);
            List<object> keysParents = new List<object>(3)
            {
                keysIterTarget,
            };
            Type keysParentType = type;
            FieldInfo keysField = null;
            // Debug.Log($"propKeysNameCompact={propNameCompact}");
            foreach (string propKeysName in propNameCompact.Split('.'))
            {
                // Debug.Log($"propKeysName={propKeysName}");

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (Type each in ReflectUtils.GetSelfAndBaseTypesFromType(keysParentType))
                {
                    FieldInfo field = each.GetField(propKeysName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (field == null)
                    {
                        continue;
                    }

                    // Debug.Log($"field={field}; keysField={keysField}");

                    keysField = field;
                    keysParentType = keysField.FieldType;
                    keysIterTarget = keysField.GetValue(keysIterTarget);
                    keysParents.Add(keysIterTarget);
                    // Debug.Log($"Prop {propKeysName} Add parents = {keysIterTarget}/{keysIterTarget.GetType()}");
                    // Debug.Log($"set keysField={keysField}/keysParentType={keysParentType}/keysIterTarget={keysIterTarget}");
                    break;
                }

                Debug.Assert(keysField != null, $"Failed to get key {propKeysName} from {keysIterTarget}");
            }

            int keysParentsCount = keysParents.Count;

            object keysParent = keysParentsCount >= 2? keysParents[keysParentsCount - 2]: keysParents[0];

            return (keysField, keysParent);
        }

        private class AsyncSearchItems
        {
            public bool Started;
            public bool Finished;
            public IEnumerator<int> SourceGenerator;
            public string KeySearchText;
            public string ValueSearchText;
            public double DebounceSearchTime;

            public List<int> HitTargetIndexes;
            public List<int> CachedHitTargetIndexes;

            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;

            public readonly HashSet<Image> LoadingImages = new HashSet<Image>();
        }

        private AsyncSearchItems _asyncSearchItems;

        private const float DebounceTime = 0.6f;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Foldout foldout = container.Q<Foldout>(name: NameFoldout(property));
            UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            foldout.RegisterValueChangedCallback(newValue => property.isExpanded = newValue.newValue);

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            Type rawType = arrayIndex == -1 ? info.FieldType : ReflectUtils.GetElementType(info.FieldType);
            Debug.Assert(rawType != null, $"Failed to get element type from {property.propertyPath}");
            // Debug.Log(info.FieldType);
            (string propKeysNameCompact, string propValuesNameCompact) = GetKeysValuesPropName(rawType);

            // Debug.Log(propKeysName);

            // Debug.Log($"propKeysNameCompact={propKeysNameCompact}");
            SerializedProperty keysProp = FindPropertyCompact(property, propKeysNameCompact);
            // property.FindPropertyRelative(propKeysNameCompact) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propKeysNameCompact);
            Debug.Assert(keysProp != null, $"Failed to get keys prop from {propKeysNameCompact}");
            // Debug.Log($"keysProp={keysProp.propertyPath}");
            SerializedProperty valuesProp = FindPropertyCompact(property, propValuesNameCompact);
            // property.FindPropertyRelative(propValuesNameCompact) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propValuesNameCompact);
            Debug.Assert(valuesProp != null, $"Failed to get values prop from {propValuesNameCompact}");

            (FieldInfo keysField, object keysParent) = GetTargetInfo(propKeysNameCompact, info, rawType, parent);

            Debug.Assert(keysField != null, $"Failed to get keys field {propKeysNameCompact} from {property.propertyPath}");
            Type keyType = ReflectUtils.GetElementType(keysField.FieldType);

            (FieldInfo valuesField, object valuesParent) = GetTargetInfo(propValuesNameCompact, info, rawType, parent);

            Debug.Assert(valuesField != null, $"Failed to get values field from {property.propertyPath}");
            Type valueType = ReflectUtils.GetElementType(valuesField.FieldType);

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

            SaintsDictionaryAttribute saintsDictionaryAttribute = saintsAttribute as SaintsDictionaryAttribute;

            int initNumberOfItemsPerPage = saintsDictionaryAttribute?.NumberOfItemsPerPage ?? -1;
            List<int> initTargets = initNumberOfItemsPerPage <= 0
                ? new List<int>(Enumerable.Range(0, keysProp.arraySize))
                : new List<int>(Enumerable.Range(0, keysProp.arraySize).Take(initNumberOfItemsPerPage));

            _asyncSearchItems = new AsyncSearchItems
            {
                Started = true,
                Finished = true,
                SourceGenerator = null,
                HitTargetIndexes = new List<int>(initTargets),
                CachedHitTargetIndexes = new List<int>(initTargets),
                KeySearchText = "",
                ValueSearchText = "",
                DebounceSearchTime = 0,
                Size = keysProp.arraySize,
                TotalPage = 1,
                NumberOfItemsPerPage = initNumberOfItemsPerPage,
            };

            // string preKeySearch = "";
            // string preValueSearch = "";
            // int prePageIndex = 0;
            // int preSize = 0;
            // int preTotalPage = 1;

            void RefreshList()
            {
                int curPageIndex = _asyncSearchItems.PageIndex;
                int numberOfItemsPerPage = _asyncSearchItems.NumberOfItemsPerPage;
                // bool needRebuild = false;
                int nowArraySize = keysProp.arraySize;

                // List<int> fullList = Enumerable.Range(0, nowArraySize).ToList();
                // List<int> useIndexes = new List<int>(itemIndexToPropertyIndex);
                // ReSharper disable once AccessToModifiedClosure
                List<int> refreshedHitTargetIndexes = new List<int>(_asyncSearchItems.Started? _asyncSearchItems.HitTargetIndexes: _asyncSearchItems.CachedHitTargetIndexes);
                if (nowArraySize != _asyncSearchItems.Size)
                {
                    _asyncSearchItems.Size = nowArraySize;
                    _asyncSearchItems.DebounceSearchTime = 0;
                    _asyncSearchItems.Started = false;
                    _asyncSearchItems.Finished = false;
                    _asyncSearchItems.HitTargetIndexes.Clear();
                    _asyncSearchItems.SourceGenerator?.Dispose();
                    _asyncSearchItems.SourceGenerator = Search(keysProp, valuesProp,
                        _asyncSearchItems.KeySearchText, _asyncSearchItems.ValueSearchText).GetEnumerator();

                    // Debug.Log("size changed, tail call refresh list");
                    // ReSharper disable once TailRecursiveCall
                    RefreshList();
                    return;
                }

                // processing search result
                // bool needSearchAgain = false;
                // if (preKeySearch != keySearch)
                // {
                //     preKeySearch = keySearch;
                //     // needSearchAgain = true;
                // }
                //
                // if (preValueSearch != valueSearch)
                // {
                //     preValueSearch = valueSearch;
                //     // needSearchAgain = true;
                // }

                // hitTargetIndexes = refreshedHitTargetIndexes;
                if (numberOfItemsPerPage > 0)
                {
                    int startIndex = curPageIndex * numberOfItemsPerPage;
                    if (startIndex >= refreshedHitTargetIndexes.Count)
                    {
                        startIndex = 0;
                        curPageIndex = 0;
                    }
                    int endIndex = Mathf.Min((curPageIndex + 1) * numberOfItemsPerPage, refreshedHitTargetIndexes.Count);
                    itemIndexToPropertyIndex = refreshedHitTargetIndexes.GetRange(startIndex, endIndex - startIndex);
                    int totalPage = Mathf.Max(1, Mathf.CeilToInt(refreshedHitTargetIndexes.Count / (float)numberOfItemsPerPage));

                    // pageField.SetValueWithoutNotify(curPageIndex + 1);


                    // needRebuild = preNumberOfItemsPerPage != numberOfItemsPerPage
                    //               || preTotalPage != totalPage
                    //               || prePageIndex != curPageIndex;

                    // preNumberOfItemsPerPage = numberOfItemsPerPage;
                    _asyncSearchItems.TotalPage = totalPage;
                    _asyncSearchItems.PageIndex = curPageIndex;
                }
                else
                {
                    itemIndexToPropertyIndex = refreshedHitTargetIndexes;
                }

                // Debug.Log(multiColumnListView.itemsSource);
                // Debug.Log(itemIndexToPropertyIndex);

                bool needRebuild = multiColumnListView.itemsSource == null
                                   || !multiColumnListView.itemsSource.Cast<int>().SequenceEqual(itemIndexToPropertyIndex);
                // if (multiColumnListView.itemsSource != null)
                // {
                //     Debug.Log(string.Join(", ", multiColumnListView.itemsSource.Cast<int>()));
                //     Debug.Log(string.Join(", ", itemIndexToPropertyIndex));
                // }

                if (needRebuild)
                {
                    // Debug.Log("rebuild list view");
                    multiColumnListView.itemsSource = itemIndexToPropertyIndex.ToList();
                    multiColumnListView.Rebuild();
                    pagePreButton.SetEnabled(_asyncSearchItems.PageIndex > 0);
                    pageField.SetValueWithoutNotify(_asyncSearchItems.PageIndex + 1);
                    pageLabel.text = $"/ {_asyncSearchItems.TotalPage}";
                    pageNextButton.SetEnabled(_asyncSearchItems.PageIndex + 1 < _asyncSearchItems.TotalPage);
                }
            }

            #endregion

            IntegerField numberOfItemsPerPage = container.Q<IntegerField>(name: NameNumberOfItemsPerPage(property));
            numberOfItemsPerPage.RegisterValueChangedCallback(evt =>
            {
                _asyncSearchItems.NumberOfItemsPerPage = evt.newValue;
                RefreshList();
            });

            Image keyLoadingImage = new Image
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
            Image valueLoadingImage = new Image
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
                        style =
                        {
                            marginRight = 3,
                            display = saintsDictionaryAttribute?.Searchable ?? true
                                ? DisplayStyle.Flex
                                : DisplayStyle.None,
                            width = Length.Percent(97f),
                        },
                    };

                    TextField searchTextField = keySearch.Q<TextField>();
                    searchTextField.style.position = Position.Relative;

                    searchTextField.Add(keyLoadingImage);
                    UIToolkitUtils.KeepRotate(keyLoadingImage);
                    keyLoadingImage.RegisterCallback<AttachToPanelEvent>(_ => keyLoadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(keyLoadingImage)));

                    header.Add(keySearch);
                    keySearch.RegisterValueChangedCallback(evt =>
                    {
                        _asyncSearchItems.KeySearchText = evt.newValue;
                        _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
                        _asyncSearchItems.Started = false;
                        _asyncSearchItems.Finished = false;
                        _asyncSearchItems.HitTargetIndexes.Clear();
                        _asyncSearchItems.SourceGenerator = Search(keysProp, valuesProp, _asyncSearchItems.KeySearchText, _asyncSearchItems.ValueSearchText).GetEnumerator();
                        _asyncSearchItems.LoadingImages.Add(keyLoadingImage);
                        RefreshList();
                    });

                    keySearch.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return)
                        {
                            _asyncSearchItems.DebounceSearchTime = 0f;
                        }
                    }, TrickleDown.TrickleDown);
                    return header;
                },
                makeCell = () => new VisualElement(),
                bindCell = (element, elementIndex) =>
                {
                    int propIndex = itemIndexToPropertyIndex[elementIndex];
                    SerializedProperty elementProp = keysProp.GetArrayElementAtIndex(propIndex);

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

                    VisualElement resultElement = CreateCellElement(keysField, keyType, elementProp, this, this, keysParent);
                    keyContainer.Add(resultElement);

                    keyContainer.TrackPropertyValue(keysProp, _ =>
                    {
                        RefreshConflict();
                    });

                    RefreshConflict();
                    return;

                    void RefreshConflict()
                    {
                        if (propIndex >= keysProp.arraySize)
                        {
                            return;
                        }
//
//                         (string curFieldError, int _, object curFieldValue) = Util.GetValue(property, info, parent);
//                         // Debug.Log(curFieldValue);
//                         if (curFieldError != "")
//                         {
// #if SAINTSFIELD_DEBUG
//                             Debug.LogError(curFieldError);
// #endif
//                             return;
//                         }

                        IEnumerable allKeyList = keysField.GetValue(keysParent) as IEnumerable;
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
                        style =
                        {
                            marginRight = 3,
                            display = saintsDictionaryAttribute?.Searchable ?? true
                                ? DisplayStyle.Flex
                                : DisplayStyle.None,
                            width = Length.Percent(97f),
                        },
                    };

                    TextField searchTextField = valueSearch.Q<TextField>();
                    searchTextField.style.position = Position.Relative;

                    searchTextField.Add(valueLoadingImage);
                    UIToolkitUtils.KeepRotate(valueLoadingImage);
                    valueLoadingImage.RegisterCallback<AttachToPanelEvent>(_ => valueLoadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(valueLoadingImage)));

                    header.Add(valueSearch);
                    valueSearch.RegisterValueChangedCallback(evt =>
                    {
                        // Debug.Log($"value search {evt.newValue}");
                        _asyncSearchItems.ValueSearchText = evt.newValue;
                        _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
                        _asyncSearchItems.Started = false;
                        _asyncSearchItems.Finished = false;
                        _asyncSearchItems.HitTargetIndexes.Clear();
                        _asyncSearchItems.SourceGenerator = Search(keysProp, valuesProp, _asyncSearchItems.KeySearchText, _asyncSearchItems.ValueSearchText).GetEnumerator();
                        _asyncSearchItems.LoadingImages.Add(valueLoadingImage);
                        RefreshList();
                    });

                    valueSearch.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return)
                        {
                            _asyncSearchItems.DebounceSearchTime = 0f;
                        }
                    }, TrickleDown.TrickleDown);
                    return header;
                },
                makeCell = () => new VisualElement(),
                bindCell = (element, elementIndex) =>
                {
                    int propIndex = itemIndexToPropertyIndex[elementIndex];
                    SerializedProperty elementProp = valuesProp.GetArrayElementAtIndex(propIndex);
                    // IEnumerable valuesResult = valuesField.GetValue(valuesParent) as IEnumerable;
                    // object valueResult = GetIndexAt(valuesResult, propIndex);
                    elementProp.isExpanded = true;
                    element.Clear();

                    // Debug.Log($"elementProp={elementProp.propertyPath}, valuesField={valuesField}, valueType={valueType}, valuesParent={valuesParent}/{valuesParent.GetType()}");

                    VisualElement resultElement = CreateCellElement(valuesField, valueType, elementProp, this, this, valuesParent);

                    element.Add(resultElement);

                    // if (!valueStructChecked)
                    // {
                    //     valueStructChecked = true;
                    //     valueStructNeedFlatten = GetNeedFlatten(elementProp, ReflectUtils.GetElementType(valuesField.FieldType));
                    // }
                    //
                    // VisualElement valueContainer = new VisualElement
                    // {
                    //     style =
                    //     {
                    //         marginRight = 3,
                    //     },
                    // };
                    // element.Add(valueContainer);
                    // if (valueStructNeedFlatten)
                    // {
                    //     foreach (SerializedProperty childProp in SerializedUtils.GetPropertyChildren(elementProp).Where(each => each != null))
                    //     {
                    //         valueContainer.Add(new Label(childProp.displayName));
                    //         PropertyField propertyField = new PropertyField(childProp)
                    //         {
                    //             label = "",
                    //         };
                    //         propertyField.Bind(property.serializedObject);
                    //         valueContainer.Add(propertyField);
                    //     }
                    // }
                    // else
                    // {
                    //     PropertyField propertyField = new PropertyField(elementProp, "");
                    //     propertyField.Bind(property.serializedObject);
                    //     valueContainer.Add(propertyField);
                    // }
                },
            });

            pagePreButton.clicked += () =>
            {
                _asyncSearchItems.PageIndex = Mathf.Max(0, _asyncSearchItems.PageIndex - 1);
                RefreshList();
            };

            pageField.RegisterValueChangedCallback(evt =>
            {
                _asyncSearchItems.PageIndex = Mathf.Clamp(evt.newValue - 1, 0, _asyncSearchItems.TotalPage - 1);
                RefreshList();
            });

            pageNextButton.clicked += () =>
            {
                _asyncSearchItems.PageIndex = Mathf.Min(_asyncSearchItems.PageIndex + 1, _asyncSearchItems.TotalPage - 1);
                RefreshList();
            };

            Button addButton = container.Q<Button>(name: NameAddButton(property));
            addButton.clicked += () =>
            {
                IncreaseArraySize(keysProp.arraySize + 1, keysProp, valuesProp);
                property.serializedObject.ApplyModifiedProperties();
                _asyncSearchItems.DebounceSearchTime = 0;
                // Debug.Log("Add button call refresh list");
                RefreshList();
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
                _asyncSearchItems.DebounceSearchTime = 0;
                RefreshList();
                // multiColumnListView.Rebuild();
                // Debug.Log($"new size = {keysProp.arraySize}");
            };

            multiColumnListView.TrackPropertyValue(keysProp, _ =>
            {
                if (_asyncSearchItems.Size != keysProp.arraySize)
                {
                    RefreshList();
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
                ListSwapValue(_asyncSearchItems.Started? _asyncSearchItems.HitTargetIndexes: _asyncSearchItems.CachedHitTargetIndexes, fromPropIndex, toPropIndex);
                // Debug.Log($"swap {fromPropIndex} -> {toPropIndex}; {string.Join(", ", _asyncSearchItems.HitTargetIndexes)}");

                property.serializedObject.ApplyModifiedProperties();
                RefreshList();
                // multiColumnListView.Rebuild();
            };

            // multiColumnListView.Rebuild();
            // _asyncSearchItems.DebounceSearchTime = 0;
            RefreshList();

            multiColumnListView.schedule.Execute(() =>
            {
                if (_asyncSearchItems.Finished)
                {
                    if(keyLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        keyLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        valueLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    return;
                }

                if (_asyncSearchItems.SourceGenerator == null)
                {
                    if(keyLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        keyLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        valueLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    return;
                }

                bool emptySearch = string.IsNullOrEmpty(_asyncSearchItems.KeySearchText) &&
                                   string.IsNullOrEmpty(_asyncSearchItems.ValueSearchText);

                if (!emptySearch && _asyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                {
                    if(keyLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        keyLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        valueLoadingImage.style.visibility = Visibility.Hidden;
                    }

                    // Debug.Log("Search wait");
                    return;
                }

                if(!_asyncSearchItems.Started)
                {
                    // Debug.Log($"Search start {_asyncSearchItems.DebounceSearchTime} -> {EditorApplication.timeSinceStartup}");
                    _asyncSearchItems.Started = true;
                    RefreshList();
                }

                if (_asyncSearchItems.LoadingImages.Count == 0)
                {
                    _asyncSearchItems.LoadingImages.Add(keyLoadingImage);
                    _asyncSearchItems.LoadingImages.Add(valueLoadingImage);
                }

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (Image loadingImage in _asyncSearchItems.LoadingImages)
                {
                    if(loadingImage.style.visibility != Visibility.Visible)
                    {
                        loadingImage.style.visibility = Visibility.Visible;
                    }
                }

                // Debug.Log($"start to search: {EditorApplication.timeSinceStartup - _asyncSearchItems.DebounceSearchTime}");
                int searchBatch = emptySearch
                    ? int.MaxValue
                    : 50;

                // _asyncSearchBusy = true;

                bool needRefreshDisplay = false;

                // return;

                for (int searchTick = 0; searchTick < searchBatch; searchTick++)
                {
                    // Debug.Log($"searching {searchTick}");
                    if (_asyncSearchItems.SourceGenerator.MoveNext())
                    {
                        // ReSharper disable once InvertIf
                        if(_asyncSearchItems.SourceGenerator.Current != -1)
                        {
                            needRefreshDisplay = true;
                            // Debug.Log($"search found {_asyncSearchItems.SourceGenerator.Current}");
                            _asyncSearchItems.HitTargetIndexes.Add(_asyncSearchItems.SourceGenerator.Current);
                        }
                    }
                    else
                    {
                        _asyncSearchItems.Finished = true;
                        _asyncSearchItems.CachedHitTargetIndexes = new List<int>(_asyncSearchItems.HitTargetIndexes);
                        _asyncSearchItems.SourceGenerator.Dispose();
                        _asyncSearchItems.SourceGenerator = null;

                        // Debug.Log($"search finished {_asyncSearchItems.HitTargetIndexes.Count}");

                        if(keyLoadingImage.style.visibility != Visibility.Hidden)
                        {
                            keyLoadingImage.style.visibility = Visibility.Hidden;
                        }
                        if(valueLoadingImage.style.visibility != Visibility.Hidden)
                        {
                            valueLoadingImage.style.visibility = Visibility.Hidden;
                        }
                        _asyncSearchItems.LoadingImages.Clear();
                        needRefreshDisplay = true;
                        break;
                    }
                }

                if(needRefreshDisplay)
                {
                    RefreshList();
                }
                // _asyncSearchBusy = false;


            }).Every(1);
            // Debug.Log($"{string.Join(",", itemIndexToPropertyIndex)}");
        }

        // private bool _asyncSearchBusy;

        // private static List<int> MakeSource(SerializedProperty property)
        // {
        //     return Enumerable.Range(0, property.arraySize).ToList();
        // }

        private static VisualElement CreateCellElement(FieldInfo info, Type rawType, SerializedProperty serializedProperty, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parent)
        {
            PropertyAttribute[] allAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(info);

            Type useDrawerType = null;
            Attribute useAttribute = null;
            IReadOnlyList<PropertyAttribute> appendPropertyAttributes = null;
            bool isArray = serializedProperty.propertyType == SerializedPropertyType.Generic
                && serializedProperty.isArray;
            if(!isArray)
            {
                ISaintsAttribute saintsAttr = allAttributes
                    .OfType<ISaintsAttribute>()
                    .FirstOrDefault();

                // Debug.Log(saintsAttr);

                useAttribute = saintsAttr as Attribute;
                if (saintsAttr != null)
                {
                    useDrawerType = GetFirstSaintsDrawerType(saintsAttr.GetType());
                }
                else
                {
                    (Attribute attrOrNull, Type drawerType) =
                        GetFallbackDrawerType(info, serializedProperty, allAttributes);
                    // Debug.Log($"{FieldWithInfo.SerializedProperty.propertyPath}: {drawerType}");
                    useAttribute = attrOrNull;
                    useDrawerType = drawerType;

                    if (useDrawerType == null &&
                        serializedProperty.propertyType == SerializedPropertyType.Generic)
                    {
                        PropertyAttribute prop = new SaintsRowAttribute(inline: true);
                        useAttribute = prop;
                        useDrawerType = typeof(SaintsRowAttributeDrawer);
                        appendPropertyAttributes = new[] { prop };
                    }
                }
            }

            // Debug.Log($"{serializedProperty.propertyPath}/{useDrawerType}");

            if (useDrawerType == null)
            {
                VisualElement r = UIToolkitUtils.CreateOrUpdateFieldRawFallback(
                    serializedProperty,
                    allAttributes,
                    rawType,
                    null,
                    info,
                    true,
                    makeRenderer,
                    doTweenPlayRecorder,
                    null,
                    parent
                );
                return UIToolkitCache.MergeWithDec(r, allAttributes);
            }

            // Nah... This didn't handle for mis-ordered case
            // // Above situation will handle all including SaintsRow for general class/struct/interface.
            // // At this point we only need to let Unity handle it
            // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // };
            // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // return (result, false);

            // Debug.Log($"{useAttribute}/{useDrawerType}: {serializedProperty.propertyPath}");

            PropertyDrawer propertyDrawer = MakePropertyDrawer(useDrawerType, info, useAttribute, null);
            // Debug.Log(saintsPropertyDrawer);
            if (propertyDrawer is SaintsPropertyDrawer saintsPropertyDrawer)
            {
                saintsPropertyDrawer.InHorizontalLayout = true;
                saintsPropertyDrawer.AppendPropertyAttributes = appendPropertyAttributes;
            }

            MethodInfo uiToolkitMethod = useDrawerType.GetMethod("CreatePropertyGUI");

            // bool isSaintsDrawer = useDrawerType.IsSubclassOf(typeof(SaintsPropertyDrawer)) || useDrawerType == typeof(SaintsPropertyDrawer);

            bool useImGui = uiToolkitMethod == null ||
                            uiToolkitMethod.DeclaringType == typeof(PropertyDrawer);  // null: old Unity || did not override

            // Debug.Log($"{useDrawerType}/{uiToolkitMethod.DeclaringType}/{FieldWithInfo.SerializedProperty.propertyPath}");

            if (!useImGui)
            {
                // Debug.Log($"{propertyDrawer} draw {serializedProperty.propertyPath}");
                VisualElement r = propertyDrawer.CreatePropertyGUI(serializedProperty);
                return UIToolkitCache.MergeWithDec(r, allAttributes);
            }

            // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
            // We don't need to handle decorators either
            PropertyField result = new PropertyField(serializedProperty, string.Empty)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            result.Bind(serializedProperty.serializedObject);
            return result;
        }

        private static void ListSwapValue(IList<int> lis, int a, int b)
        {
            int aIndex = lis.IndexOf(a);
            int bIndex = lis.IndexOf(b);

            (lis[aIndex], lis[bIndex]) = (lis[bIndex], lis[aIndex]);
        }
    }
}
#elif !SAINTSFIELD_UI_TOOLKIT_DISABLE && (UNITY_2021_3_OR_NEWER || SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK)
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
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
