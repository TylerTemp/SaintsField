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

#if UNITY_2022_2_OR_NEWER
namespace SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer
{
    public partial class SaintsHashSetDrawer
    {
        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_Foldout";
        private static string NameTotalCount(SerializedProperty property) => $"{property.propertyPath}__SaintsHashSet_TotalCount";

        private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsHashSet_ListView";
        private static string NameTotalCountBottom(SerializedProperty property) => $"{property.propertyPath}__SaintsHashSet_TotalCountBottom";

        private static string NamePagePreButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_PagePreButton";

        private static string NamePage(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_Page";

        private static string NamePageLabel(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_PageLabel";
        private static string NamePageNextButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_PageNextButton";

        private static string NameAddButton(SerializedProperty property) => $"{property.propertyPath}__SaintsHashSet_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"{property.propertyPath}__SaintsHashSet_RemoveButton";

        private static string NameNumberOfItemsPerPage(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_NameNumberOfItemsPerPage";

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsHashSetAttribute saintsHashSetAttribute = saintsAttribute as SaintsHashSetAttribute;

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

            ListView multiColumnListView = new ListView
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

            int numberOfItemsPerPage = saintsHashSetAttribute?.NumberOfItemsPerPage ?? -1;

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
            public string SearchText;
            public double DebounceSearchTime;

            public List<int> HitTargetIndexes;
            public List<int> CachedHitTargetIndexes;

            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;

            // public Image LoadingImage;
        }

        private AsyncSearchItems _asyncSearchItems;

        private const float DebounceTime = 0.6f;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsHashSetAttribute saintsHashSetAttribute = saintsAttribute as SaintsHashSetAttribute;

            Foldout foldout = container.Q<Foldout>(name: NameFoldout(property));
            UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            foldout.RegisterValueChangedCallback(newValue => property.isExpanded = newValue.newValue);

            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) == -1 ? info.FieldType : ReflectUtils.GetElementType(info.FieldType);
            Debug.Assert(rawType != null, $"Failed to get element type from {property.propertyPath}");
            // Debug.Log(info.FieldType);
            string propNameCompact = GetPropName(rawType);

            // Debug.Log(propKeysName);

            // Debug.Log($"propKeysNameCompact={propKeysNameCompact}");
            SerializedProperty wrapProp = FindPropertyCompact(property, propNameCompact);
            // property.FindPropertyRelative(propKeysNameCompact) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propKeysNameCompact);
            Debug.Assert(wrapProp != null, $"Failed to get prop from {propNameCompact}");
            // Debug.Log($"keysProp={keysProp.propertyPath}");
            (FieldInfo wrapField, object wrapParent) = GetTargetInfo(propNameCompact, info, rawType, parent);

            Debug.Assert(wrapField != null, $"Failed to get field {propNameCompact} from {property.propertyPath}");
            Type wrapType = ReflectUtils.GetElementType(wrapField.FieldType);

            IntegerField totalCountFieldTop = container.Q<IntegerField>(name: NameTotalCount(property));
            totalCountFieldTop.SetValueWithoutNotify(wrapProp.arraySize);
            IntegerField totalCountBottomField = container.Q<IntegerField>(name: NameTotalCountBottom(property));
            totalCountBottomField.SetValueWithoutNotify(wrapProp.arraySize);

            void ManuallySetSize(int size)
            {
                int newSize = Mathf.Max(size, 0);
                if (newSize >= wrapProp.arraySize)
                {
                    if (IncreaseArraySize(newSize, wrapProp))
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    IReadOnlyList<int> deleteIndexes = Enumerable.Range(newSize, wrapProp.arraySize - newSize)
                        .Reverse()
                        .ToList();
                    DecreaseArraySize(deleteIndexes, wrapProp);
                    property.serializedObject.ApplyModifiedProperties();
                }
                totalCountFieldTop.SetValueWithoutNotify(newSize);
                totalCountBottomField.SetValueWithoutNotify(newSize);
            }

            totalCountFieldTop.TrackPropertyValue(wrapProp, _ =>
            {
                totalCountFieldTop.SetValueWithoutNotify(wrapProp.arraySize);
            });
            totalCountFieldTop.RegisterValueChangedCallback(evt => ManuallySetSize(evt.newValue));

            Label pageLabel = container.Q<Label>(name: NamePageLabel(property));
            Button pagePreButton = container.Q<Button>(name: NamePagePreButton(property));
            IntegerField pageField = container.Q<IntegerField>(name: NamePage(property));
            Button pageNextButton = container.Q<Button>(name: NamePageNextButton(property));

            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, wrapProp.arraySize).ToList();

            ListView listView = container.Q<ListView>(name: NameListView(property));

            #region Paging & Search

            int initNumberOfItemsPerPage = saintsHashSetAttribute?.NumberOfItemsPerPage ?? -1;
            List<int> initTargets = new List<int>(Enumerable.Range(0, wrapProp.arraySize));

            _asyncSearchItems = new AsyncSearchItems
            {
                Started = true,
                Finished = true,
                SourceGenerator = null,
                HitTargetIndexes = new List<int>(initTargets),
                CachedHitTargetIndexes = new List<int>(initTargets),
                SearchText = "",
                DebounceSearchTime = 0,
                Size = wrapProp.arraySize,
                TotalPage = 1,
                NumberOfItemsPerPage = initNumberOfItemsPerPage,
            };

            // Debug.Log($"init HitTargetIndexes={string.Join(", ", _asyncSearchItems.HitTargetIndexes)}");

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
                int nowArraySize = wrapProp.arraySize;

                // List<int> fullList = Enumerable.Range(0, nowArraySize).ToList();
                // List<int> useIndexes = new List<int>(itemIndexToPropertyIndex);
                // ReSharper disable once AccessToModifiedClosure
                List<int> refreshedHitTargetIndexes = new List<int>(_asyncSearchItems.Started? _asyncSearchItems.HitTargetIndexes: _asyncSearchItems.CachedHitTargetIndexes);
                // Debug.Log($"_asyncSearchItems.Started={_asyncSearchItems.Started}; refreshedHitTargetIndexes={string.Join(",", refreshedHitTargetIndexes)}");
                if (nowArraySize != _asyncSearchItems.Size)
                {
                    _asyncSearchItems.Size = nowArraySize;
                    _asyncSearchItems.DebounceSearchTime = 0;
                    _asyncSearchItems.Started = false;
                    _asyncSearchItems.Finished = false;
                    _asyncSearchItems.HitTargetIndexes.Clear();
                    _asyncSearchItems.SourceGenerator?.Dispose();
                    _asyncSearchItems.SourceGenerator = Search(wrapProp, _asyncSearchItems.SearchText).GetEnumerator();

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
                    // Debug.Log($"{refreshedHitTargetIndexes.Count}/{numberOfItemsPerPage}={totalPage}");

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

                bool needRebuild = listView.itemsSource == null
                                   || !listView.itemsSource.Cast<int>().SequenceEqual(itemIndexToPropertyIndex);
                // if (multiColumnListView.itemsSource != null)
                // {
                //     Debug.Log(string.Join(", ", multiColumnListView.itemsSource.Cast<int>()));
                //     Debug.Log(string.Join(", ", itemIndexToPropertyIndex));
                // }

                if (needRebuild)
                {
                    // Debug.Log("rebuild list view");
                    listView.itemsSource = itemIndexToPropertyIndex.ToList();
                    listView.Rebuild();
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

            Func<ToolbarSearchField> makeHeader = () =>
            {
                ToolbarSearchField wrapSearch = new ToolbarSearchField
                {
                    style =
                    {
                        marginRight = 3,
                        display = saintsHashSetAttribute?.Searchable ?? true
                            ? DisplayStyle.Flex
                            : DisplayStyle.None,
                        width = Length.Percent(97f),
                    },
                };

                TextField searchTextField = wrapSearch.Q<TextField>();
                searchTextField.style.position = Position.Relative;

                searchTextField.Add(loadingImage);
                UIToolkitUtils.KeepRotate(loadingImage);
                loadingImage.RegisterCallback<AttachToPanelEvent>(_ =>
                    loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingImage)));
                // _asyncSearchItems.LoadingImage = loadingImage;

                wrapSearch.RegisterValueChangedCallback(evt =>
                {
                    _asyncSearchItems.SearchText = evt.newValue;
                    _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
                    _asyncSearchItems.Started = false;
                    _asyncSearchItems.Finished = false;
                    _asyncSearchItems.HitTargetIndexes.Clear();
                    _asyncSearchItems.SourceGenerator = Search(wrapProp, _asyncSearchItems.SearchText).GetEnumerator();
                    RefreshList();
                });

                wrapSearch.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return)
                    {
                        _asyncSearchItems.DebounceSearchTime = 0f;
                    }
                }, TrickleDown.TrickleDown);
                return wrapSearch;
            };

#if UNITY_6000_0_OR_NEWER
            listView.makeHeader = makeHeader;
#else
            listView.parent?.Insert(listView.parent.IndexOf(listView), makeHeader());
#endif

            listView.makeItem = () => new VisualElement();
            listView.bindItem = (element, elementIndex) =>
            {
                int propIndex = itemIndexToPropertyIndex[elementIndex];
                SerializedProperty elementProp = wrapProp.GetArrayElementAtIndex(propIndex);

                elementProp.isExpanded = true;
                element.Clear();

                VisualElement wrapContainer = new VisualElement
                {
                    style =
                    {
                        marginRight = 3,
                    },
                };
                element.Add(wrapContainer);

                VisualElement resultElement =
                    CreateCellElement(wrapField, wrapType, elementProp, this, this, wrapParent);
                wrapContainer.Add(resultElement);

                wrapContainer.TrackPropertyValue(wrapProp, _ =>
                {
                    RefreshConflict();
                });

                RefreshConflict();
                return;

                void RefreshConflict()
                {
                    if (propIndex >= wrapProp.arraySize)
                    {
                        return;
                    }

                    IEnumerable allKeyList = wrapField.GetValue(wrapParent) as IEnumerable;
                    Debug.Assert(allKeyList != null, $"list {wrapField.Name} is null");
                    (object value, int index)[] indexedValue = allKeyList.Cast<object>().WithIndex().ToArray();
                    object thisKey = indexedValue[propIndex].value;
                    // Debug.Log($"checking with {thisKey}");
                    foreach ((object existKey, int _) in indexedValue.Where(each => each.index != propIndex))
                    {
                        // Debug.Log($"{existKey}/{thisKey}");
                        // ReSharper disable once InvertIf
                        if (Util.GetIsEqual(existKey, thisKey))
                        {
                            wrapContainer.style.backgroundColor = WarningColor;
                            return;
                        }
                    }

                    wrapContainer.style.backgroundColor = Color.clear;
                }
            };

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
                // Debug.Log($"_asyncSearchItems.PageIndex={_asyncSearchItems.PageIndex}");
                RefreshList();
            };

            Button addButton = container.Q<Button>(name: NameAddButton(property));
            addButton.clicked += () =>
            {
                IncreaseArraySize(wrapProp.arraySize + 1, wrapProp);
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

                List<int> selected = listView.selectedIndices
                    .Select(oriIndex => itemIndexToPropertyIndex[oriIndex])
                    .OrderByDescending(each => each)
                    .ToList();

                if (selected.Count == 0)
                {
                    int curSize = wrapProp.arraySize;
                    // Debug.Log($"curSize={curSize}");
                    if (curSize == 0)
                    {
                        return;
                    }
                    selected.Add(curSize - 1);
                }

                // Debug.Log($"delete {keysProp.propertyPath}/{keysProp.arraySize} key at {string.Join(",", selected)}");

                DecreaseArraySize(selected, wrapProp);
                property.serializedObject.ApplyModifiedProperties();
                // keysProp.serializedObject.ApplyModifiedProperties();
                // valuesProp.serializedObject.ApplyModifiedProperties();
                // multiColumnListView.Rebuild();
                _asyncSearchItems.DebounceSearchTime = 0;
                RefreshList();
                // multiColumnListView.Rebuild();
                // Debug.Log($"new size = {keysProp.arraySize}");
            };

            listView.TrackPropertyValue(wrapProp, _ =>
            {
                if (_asyncSearchItems.Size != wrapProp.arraySize)
                {
                    RefreshList();
                }
            });

            listView.itemIndexChanged += (first, second) =>
            {
                // Debug.Log($"first={first}, second={second} | {string.Join(",", itemIndexToPropertyIndex)}");
                int fromPropIndex = itemIndexToPropertyIndex[first];
                int toPropIndex = itemIndexToPropertyIndex[second];

                // Debug.Log($"array {fromPropIndex} <-> {toPropIndex}");

                wrapProp.MoveArrayElement(fromPropIndex, toPropIndex);

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

            listView.schedule.Execute(() =>
            {
                if (_asyncSearchItems.Finished)
                {
                    if(loadingImage.style.visibility != Visibility.Hidden)
                    {
                        loadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        valueLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    return;
                }

                if (_asyncSearchItems.SourceGenerator == null)
                {
                    if(loadingImage.style.visibility != Visibility.Hidden)
                    {
                        loadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        valueLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    return;
                }

                bool emptySearch = string.IsNullOrEmpty(_asyncSearchItems.SearchText);

                if (!emptySearch && _asyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                {
                    if(loadingImage.style.visibility != Visibility.Hidden)
                    {
                        loadingImage.style.visibility = Visibility.Hidden;
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

                if(loadingImage.style.visibility != Visibility.Visible)
                {
                    loadingImage.style.visibility = Visibility.Visible;
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

                        if(loadingImage.style.visibility != Visibility.Hidden)
                        {
                            loadingImage.style.visibility = Visibility.Hidden;
                        }
                        if(valueLoadingImage.style.visibility != Visibility.Hidden)
                        {
                            valueLoadingImage.style.visibility = Visibility.Hidden;
                        }
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
#endif
