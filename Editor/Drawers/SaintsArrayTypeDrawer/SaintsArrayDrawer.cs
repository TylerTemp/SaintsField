#if UNITY_2022_2_OR_NEWER
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
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
        private static string NameRoot(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet";
        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_Foldout";

        // private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsArraySet_ListView";

        protected override bool UseCreateFieldUIToolKit => true;

        private class ElementField : BaseField<Object>
        {
            public ElementField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private struct Payload
        {
            public readonly Foldout Foldout;
            public readonly IntegerField ArraySizeField;
            public readonly ListView ListView;
            public readonly SearchPager SearchPager;

            public Payload(
                Foldout foldout,
                IntegerField arraySizeField,
                ListView listView,
                SearchPager searchPager)
            {
                Foldout = foldout;
                ArraySizeField = arraySizeField;
                ListView = listView;
                SearchPager = searchPager;
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsArrayAttribute saintsArrayAttribute = saintsAttribute as SaintsArrayAttribute ?? new SaintsArrayAttribute(searchable: false, numberOfItemsPerPage: 0);

            VisualElement root = new VisualElement
            {
                name = NameRoot(property),
            };

            VisualElement header = new VisualElement();
            root.Add(header);

            Foldout foldout = new Foldout
            {
                text = GetPreferredLabel(property),
                // value = property.isExpanded,
                name = NameFoldout(property),
                viewDataKey = property.propertyPath,
            };
            header.Add(foldout);
            VisualElement foldoutContent = foldout.contentContainer;
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

            IntegerField arraySizeField = new IntegerField
            {
                isDelayed = true,
                style =
                {
                    width = 50,
                    marginLeft = 0,
                },
            };
            rightAlign.Add(arraySizeField);

            VisualElement textInputElement = arraySizeField.Q<VisualElement>(name: "unity-text-input");
            if (textInputElement != null)
            {
                textInputElement.style.borderTopLeftRadius = textInputElement.style.borderTopRightRadius = 0;
                textInputElement.style.marginLeft = 0;
            }

            SearchPager searchPager = new SearchPager();
            root.Add(searchPager);

            ListView listView = new ListView
            {
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                headerTitle = GetPreferredLabel(property),
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },

                // name = NameListView(property),
            };

            root.Add(listView);

            // ListViewPagerFooterStruct footerStruct = new ListViewPagerFooterStruct(true);
            // root.Add(footerStruct.Root);

            root.userData = new Payload(foldout, arraySizeField, listView, searchPager);

            bool noSearch = !saintsArrayAttribute.Searchable;
            if (noSearch)
            {
                searchPager.SearchContainer.style.visibility = Visibility.Hidden;
            }
            bool noPaging = saintsArrayAttribute.NumberOfItemsPerPage <= 0;
            if (noPaging)
            {
                searchPager.PagingContainer.style.display = DisplayStyle.None;
            }
            // Debug.Log($"noSearch={noSearch}, noPaging={noPaging}");
            if (noSearch && noPaging)
            {
                searchPager.style.display = DisplayStyle.None;
            }

            foldout.RegisterValueChangedCallback(_ => RefreshToggleDisplay());
            foldout.RegisterCallback<AttachToPanelEvent>(_ => foldout.schedule.Execute(RefreshToggleDisplay));

            return root;

            void RefreshToggleDisplay()
            {
                DisplayStyle display = foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
                listView.style.display = display;
                // footerStruct.Root.style.display = display;
                if(!noSearch || !noPaging)
                {
                    searchPager.style.display = display;
                }
            }
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

            VisualElement root = container.Q<VisualElement>(name: NameRoot(property));
            Payload payload = (Payload)root.userData;

            Foldout foldout = payload.Foldout;
            UIToolkitUtils.AddContextualMenuManipulator(foldout, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

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

            // IntegerField totalCountFieldTop = container.Q<IntegerField>(name: NameTotalCount(property));
            IntegerField totalCountFieldTop = payload.ArraySizeField;
            totalCountFieldTop.SetValueWithoutNotify(wrapProp.arraySize);
            IntegerField totalCountBottomField = payload.SearchPager.NumberOfItemsTotalField;
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

            Label pageLabel = payload.SearchPager.PageLabel;
            Button pagePreButton = payload.SearchPager.PagePreButton;
            IntegerField pageField = payload.SearchPager.PageField;
            Button pageNextButton = payload.SearchPager.PageNextButton;

            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, wrapProp.arraySize).ToList();

            ListView listView = payload.ListView;

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

            IntegerField numberOfItemsPerPage = payload.SearchPager.NumberOfItemsPerPageField;
            numberOfItemsPerPage.RegisterValueChangedCallback(evt =>
            {
                _asyncSearchItems.NumberOfItemsPerPage = evt.newValue;
                RefreshList();
            });

            VisualElement loadingImage = payload.SearchPager.LoadingImage;

            ToolbarSearchField wrapSearch = payload.SearchPager.ToolbarSearchField;

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

            listView.makeItem = () => new VisualElement();

            // TODO: reference type
            // bool needUseRef = typeof(ReferenceHashSet<>).IsAssignableFrom(rawType.GetGenericTypeDefinition());
            IReadOnlyList<Attribute> r = SaintsWrapUtils.GetInjectedPropertyAttributes(info, typeof(ValueAttributeAttribute));
            SerializeReference serRef = r.OfType<SerializeReference>().FirstOrDefault();
            IReadOnlyList<Attribute> injectedKeyAttributes = serRef == null
                ? Array.Empty<Attribute>()
                : new[]{serRef};
            // IReadOnlyList<Attribute> injectedKeyAttributes = new List<Attribute>();

            WrapType valueWrapType = SaintsWrapUtils.EnsureWrapType(
                property.FindPropertyRelative("_wrapType"), wrapField, injectedKeyAttributes);

            listView.bindItem = (element, elementIndex) =>
            {
                int propIndex = itemIndexToPropertyIndex[elementIndex];
                SerializedProperty elementProp = wrapProp.GetArrayElementAtIndex(propIndex);

                elementProp.isExpanded = true;
                element.Clear();

                VisualElement resultElement =
                    SaintsWrapUtils.CreateCellElement(
                        valueWrapType,
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

            Button addButton = payload.ListView.Q<Button>("unity-list-view__add-button");
            addButton.clickable = new Clickable(() => {
                IncreaseArraySize(wrapProp.arraySize + 1, wrapProp);
                property.serializedObject.ApplyModifiedProperties();
                _asyncSearchItems.DebounceSearchTime = 0;
                // Debug.Log("Add button call refresh list");
                RefreshList();
                // multiColumnListView.Rebuild();
            });
            Button deleteButton = payload.ListView.Q<Button>("unity-list-view__remove-button");
            deleteButton.clickable = new Clickable(() =>
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
            });

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
                    // if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    // {
                    //     valueLoadingImage.style.visibility = Visibility.Hidden;
                    // }
                    return;
                }

                if (_asyncSearchItems.SourceGenerator == null)
                {
                    if(loadingImage.style.visibility != Visibility.Hidden)
                    {
                        loadingImage.style.visibility = Visibility.Hidden;
                    }
                    // if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    // {
                    //     valueLoadingImage.style.visibility = Visibility.Hidden;
                    // }
                    return;
                }

                bool emptySearch = string.IsNullOrEmpty(_asyncSearchItems.SearchText);

                if (!emptySearch && _asyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                {
                    if(loadingImage.style.visibility != Visibility.Hidden)
                    {
                        loadingImage.style.visibility = Visibility.Hidden;
                    }
                    // if(valueLoadingImage.style.visibility != Visibility.Hidden)
                    // {
                    //     valueLoadingImage.style.visibility = Visibility.Hidden;
                    // }

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
                        // if(valueLoadingImage.style.visibility != Visibility.Hidden)
                        // {
                        //     valueLoadingImage.style.visibility = Visibility.Hidden;
                        // }
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

            Debug.Assert(_propName != null, $"Failed to find property name for {rawType}. Do you forget to define a `static string EditorPropertyName` (nameof(YourPropList))?");

            return _propName;
        }
    }
}
#endif
