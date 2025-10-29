﻿#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers.SaintsArrayTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsArrayAttribute), true)]
    [CustomPropertyDrawer(typeof(SaintsList<>), true)]
    [CustomPropertyDrawer(typeof(SaintsArray<>), true)]
    public class SaintsArrayDrawer: SaintsPropertyDrawer
    {
        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArraySet_Foldout";
        private static string NameTotalCount(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_TotalCount";

        private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_ListView";
        private static string NameTotalCountBottom(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_TotalCountBottom";

        private static string NamePagePreButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArraySet_PagePreButton";

        private static string NamePage(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArraySet_Page";

        private static string NamePageLabel(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArraySet_PageLabel";
        private static string NamePageNextButton(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArraySet_PageNextButton";

        private static string NameAddButton(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_RemoveButton";

        private static string NameNumberOfItemsPerPage(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArraySet_NameNumberOfItemsPerPage";

        protected override bool UseCreateFieldUIToolKit => true;

        private class ElementField : BaseField<Object>
        {
            public ElementField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsArrayAttribute saintsArrayAttribute = saintsAttribute as SaintsArrayAttribute;

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
                },
            };

            int numberOfItemsPerPage = saintsArrayAttribute?.NumberOfItemsPerPage ?? -1;

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

            ListViewFooterElement footerAddRemove = new ListViewFooterElement();

            footerButtons.Add(footerAddRemove);

            footerAddRemove.AddButton.name = NameAddButton(property);
            footerAddRemove.RemoveButton.name = NameRemoveButton(property);

            foldout.Add(footerButtons);

            return root;
        }

        private static (FieldInfo targetInfo, object targetParent) GetTargetInfo(string propNameCompact, Type type, object saintsSerValue)
        {

            // object keysIterTarget = info.GetValue(parent);
            object keysIterTarget = saintsSerValue;
            List<object> keysParents = new List<object>(3)
            {
                saintsSerValue,
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
            SaintsArrayAttribute saintsArrayAttribute = saintsAttribute as SaintsArrayAttribute;

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;

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
            object fieldValue = info.GetValue(parent);
            if (insideArray)
            {
                fieldValue = ((IEnumerable)fieldValue).Cast<object>().ElementAt(arrayIndex);
            }
            (FieldInfo wrapField, object wrapParent) = GetTargetInfo(propNameCompact, rawType, fieldValue);

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

            int initNumberOfItemsPerPage = saintsArrayAttribute?.NumberOfItemsPerPage ?? -1;
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

            ToolbarSearchField MakeHeader()
            {
                ToolbarSearchField wrapSearch = new ToolbarSearchField
                {
                    style =
                    {
                        marginRight = 3,
                        display = saintsArrayAttribute?.Searchable ?? true
                            ? DisplayStyle.Flex
                            : DisplayStyle.None,
                        width = Length.Percent(97f),
                    },
                };

                TextField searchTextField = wrapSearch.Q<TextField>();
                searchTextField.style.position = Position.Relative;

                searchTextField.Add(loadingImage);
                UIToolkitUtils.KeepRotate(loadingImage);
                loadingImage.RegisterCallback<AttachToPanelEvent>(_ => loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingImage)));
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
            }

#if UNITY_6000_0_OR_NEWER
            listView.makeHeader = (Func<ToolbarSearchField>)MakeHeader;
#else
            listView.parent?.Insert(listView.parent.IndexOf(listView), MakeHeader());
#endif

            listView.makeItem = () => new VisualElement();

            // TODO: reference type
            // bool needUseRef = typeof(ReferenceHashSet<>).IsAssignableFrom(rawType.GetGenericTypeDefinition());
            IReadOnlyList<Attribute> r = SaintsWrapUtils.GetInjectedPropertyAttributes(info, typeof(ValueAttributeAttribute));
            SerializeReference serRef = r.OfType<SerializeReference>().FirstOrDefault();
            IReadOnlyList<Attribute> injectedKeyAttributes = serRef == null
                ? Array.Empty<Attribute>()
                : new[]{serRef};
            // IReadOnlyList<Attribute> injectedKeyAttributes = new List<Attribute>();

            listView.bindItem = (element, elementIndex) =>
            {
                int propIndex = itemIndexToPropertyIndex[elementIndex];
                SerializedProperty elementProp = wrapProp.GetArrayElementAtIndex(propIndex);

                elementProp.isExpanded = true;
                element.Clear();

                VisualElement resultElement =
                    SaintsWrapUtils.CreateCellElement(
                        wrapField,
                        wrapType,
                        elementProp, injectedKeyAttributes, this, this, wrapParent
                    );

                ElementField wrapContainer = new ElementField($"Element {propIndex}", resultElement)
                {
                    // style =
                    // {
                    //     marginRight = 3,
                    // },
                };
                element.Add(wrapContainer);

                // wrapContainer.Add(resultElement);
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

        private static void ListSwapValue(IList<int> lis, int a, int b)
        {
            int aIndex = lis.IndexOf(a);
            int bIndex = lis.IndexOf(b);

            (lis[aIndex], lis[bIndex]) = (lis[bIndex], lis[aIndex]);
        }

        private static SerializedProperty FindPropertyCompact(SerializedProperty property, string propValuesNameCompact)
        {
            SerializedProperty prop = property.FindPropertyRelative(propValuesNameCompact);
            if (prop != null)
            {
                return prop;
            }

            SerializedProperty accProp = property;
            foreach (string propSegName in propValuesNameCompact.Split('.'))
            {
                SerializedProperty findProp = accProp.FindPropertyRelative(propSegName) ?? SerializedUtils.FindPropertyByAutoPropertyName(accProp, propSegName);
                Debug.Assert(findProp != null, $"Failed to find prop {propSegName} in {accProp.propertyPath}");
                accProp = findProp;
            }

            return accProp;
        }

        private static bool IncreaseArraySize(int newValue, SerializedProperty prop)
        {
            int propSize = prop.arraySize;
            if (propSize == newValue)
            {
                return false;
            }

            prop.arraySize = newValue;
            return true;
        }

        private static void DecreaseArraySize(IReadOnlyList<int> indexReversed, SerializedProperty prop)
        {
            int curSize = prop.arraySize;
            foreach (int index in indexReversed.Where(each => each < curSize))
            {
                // Debug.Log($"Remove index {index}");
                prop.DeleteArrayElementAtIndex(index);
            }
        }

        private static IEnumerable<int> Search(SerializedProperty wrapProp, string searchText)
        {
            int size = wrapProp.arraySize;

            bool searchEmpty = string.IsNullOrEmpty(searchText);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (searchEmpty)
            {
                for (int index = 0; index < size; index++)
                {
                    yield return index;
                }
                yield break;
            }

            foreach (int index in SerializedUtils.SearchArrayProperty(wrapProp, searchText))
            {
                yield return index;
            }
        }

        private string _propName;

        private string GetPropName(Type rawType)
        {
            // Type fieldType = ReflectUtils.GetElementType(rawType);

            // ReSharper disable once InvertIf
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_propName == null)
            {
                _propName = ReflectUtils.GetIWrapPropName(rawType);
            }

            Debug.Assert(_propName != null, $"Failed to find property name for {rawType}. Do you froget to define a `static string EditorPropertyName` (nameof(YourPropList))?");

            return _propName;
        }
    }
}
#endif
