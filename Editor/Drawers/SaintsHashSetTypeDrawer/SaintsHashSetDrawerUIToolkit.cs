#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer
{
    public partial class SaintsHashSetDrawer
    {
        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_Foldout";

        private static string NameSearch(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_Search";

        private static string NameLoading(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_Loading";

        private static string NameListView(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_ListView";

        private static string NamePager(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_Pager";

        private static string NameFooterButtons(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsHashSet_FooterButtons";

        private class ElementField : BaseField<Object>
        {
            public ElementField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
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
        }

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            SaintsHashSetAttribute attribute = saintsAttribute as SaintsHashSetAttribute ??
                                               new SaintsHashSetAttribute();

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

            CollectionFoldout foldout = new CollectionFoldout(GetPreferredLabel(property))
            {
                value = property.isExpanded,
                name = NameFoldout(property),
                viewDataKey = SerializedUtils.GetUniqueId(property),
            };
            foldout.contentContainer.style.marginLeft = 0;
            if (!string.IsNullOrEmpty(property.tooltip))
            {
                Label foldoutLabel = foldout.Q<Label>();
                if (foldoutLabel != null)
                {
                    foldoutLabel.tooltip = property.tooltip;
                }
            }
            root.Add(foldout);

            ToolbarSearchField searchField = new ToolbarSearchField
            {
                name = NameSearch(property),
                style =
                {
                    display = attribute.Searchable ? DisplayStyle.Flex : DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                    width = StyleKeyword.Auto,
                },
            };
            TextField searchTextField = searchField.Q<TextField>();
            searchTextField.style.position = Position.Relative;
            Image loadingImage = new Image
            {
                name = NameLoading(property),
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
            foldout.Add(searchField);

            ListView listView = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,
                name = NameListView(property),
                viewDataKey = SerializedUtils.GetUniqueId(property),
            };
            foldout.Add(listView);

            VisualElement footer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                },
            };
            footer.Add(new ListViewPagerElement
            {
                name = NamePager(property),
                style =
                {
                    display = attribute.NumberOfItemsPerPage > 0
                        ? DisplayStyle.Flex
                        : DisplayStyle.None,
                },
            });
            footer.Add(new ListViewFooterButtonsElement
            {
                name = NameFooterButtons(property),
            });
            foldout.Add(footer);

            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            EnsureSerializedVersion(property);

            SaintsHashSetAttribute attribute = saintsAttribute as SaintsHashSetAttribute ??
                                               new SaintsHashSetAttribute();
            (string contextError, HashSetFieldContext context) = TryGetHashSetContext(property, info, parent);
            Debug.Assert(contextError == "", contextError);
            if (contextError != "")
            {
                return;
            }

            CollectionFoldout foldout = container.Q<CollectionFoldout>(name: NameFoldout(property));
            ToolbarSearchField searchField = container.Q<ToolbarSearchField>(name: NameSearch(property));
            Image loadingImage = container.Q<Image>(name: NameLoading(property));
            ListView listView = container.Q<ListView>(name: NameListView(property));
            ListViewPagerElement pager = container.Q<ListViewPagerElement>(name: NamePager(property));
            ListViewFooterButtonsElement footerButtons =
                container.Q<ListViewFooterButtonsElement>(name: NameFooterButtons(property));

            UIToolkitUtils.AddContextualMenuManipulator(foldout, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            foldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;

            (object callbackParent, ISaintsHashSetEditorTool hashSetTool) ResolveSearchTarget()
            {
                object directParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent ?? parent;
                object fieldValue = info.GetValue(directParent);
                if (insideArray)
                {
                    fieldValue = ((IEnumerable)fieldValue).Cast<object>().ElementAt(arrayIndex);
                }
                return (directParent, (ISaintsHashSetEditorTool)fieldValue);
            }

            object extraSearchTarget = ResolveSearchTarget().callbackParent;
            (MethodInfo methodInfo, SearchParamType paramType) extraSearchMethod =
                string.IsNullOrEmpty(attribute.ExtraSearch)
                    ? default
                    : GetSearchMethodInfo(attribute.ExtraSearch, extraSearchTarget.GetType(), context.ElementType);
            Debug.Assert(string.IsNullOrEmpty(attribute.ExtraSearch) || extraSearchMethod.methodInfo != null,
                $"extraSearchMethod `{attribute.ExtraSearch}` not found in {extraSearchTarget.GetType()} for {context.ElementType}");

            bool defaultSearch = true;
            bool objectSearch = attribute.ObjectSearch;
            bool extraSearch = extraSearchMethod.methodInfo != null;

            IntegerField totalCountTop = foldout.ArraySizeField;
            IntegerField totalCountBottom = pager.NumberOfItemsTotalField;
            IntegerField numberOfItemsPerPage = pager.NumberOfItemsPerPageField;
            Button pagePreButton = pager.PagePreButton;
            IntegerField pageField = pager.PageField;
            Label pageLabel = pager.PageLabel;
            Button pageNextButton = pager.PageNextButton;

            List<int> initialIndexes = Enumerable.Range(0, context.WrapProp.arraySize).ToList();
            AsyncSearchItems searchItems = new AsyncSearchItems
            {
                Started = true,
                Finished = true,
                HitTargetIndexes = new List<int>(initialIndexes),
                CachedHitTargetIndexes = new List<int>(initialIndexes),
                SearchText = "",
                Size = context.WrapProp.arraySize,
                TotalPage = 1,
                NumberOfItemsPerPage = attribute.NumberOfItemsPerPage,
            };
            List<int> itemIndexToPropertyIndex = new List<int>(initialIndexes);

            void SetFullResults()
            {
                searchItems.SourceGenerator?.Dispose();
                searchItems.SourceGenerator = null;
                searchItems.Started = true;
                searchItems.Finished = true;
                searchItems.Size = context.WrapProp.arraySize;
                searchItems.SearchText = "";
                searchItems.HitTargetIndexes = Enumerable.Range(0, context.WrapProp.arraySize).ToList();
                searchItems.CachedHitTargetIndexes = new List<int>(searchItems.HitTargetIndexes);
            }

            void RefreshList()
            {
                if (searchItems.Size != context.WrapProp.arraySize)
                {
                    RestartSearch(searchItems.SearchText, false, true);
                    return;
                }

                List<int> results = new List<int>(searchItems.Started
                    ? searchItems.HitTargetIndexes
                    : searchItems.CachedHitTargetIndexes);
                int pageIndex = searchItems.PageIndex;
                if (searchItems.NumberOfItemsPerPage > 0)
                {
                    int pageCount = Mathf.Max(1,
                        Mathf.CeilToInt(results.Count / (float)searchItems.NumberOfItemsPerPage));
                    pageIndex = Mathf.Clamp(pageIndex, 0, pageCount - 1);
                    int start = pageIndex * searchItems.NumberOfItemsPerPage;
                    itemIndexToPropertyIndex = results.Skip(start).Take(searchItems.NumberOfItemsPerPage).ToList();
                    searchItems.TotalPage = pageCount;
                }
                else
                {
                    pageIndex = 0;
                    searchItems.TotalPage = 1;
                    itemIndexToPropertyIndex = results;
                }
                searchItems.PageIndex = pageIndex;

                bool needRebuild = listView.itemsSource == null ||
                                   !listView.itemsSource.Cast<int>().SequenceEqual(itemIndexToPropertyIndex);
                if (needRebuild)
                {
                    listView.itemsSource = itemIndexToPropertyIndex.ToList();
                    listView.Rebuild();
                }

                totalCountTop.SetValueWithoutNotify(context.WrapProp.arraySize);
                totalCountBottom.SetValueWithoutNotify(context.WrapProp.arraySize);
                pagePreButton.SetEnabled(searchItems.PageIndex > 0);
                pageField.SetValueWithoutNotify(searchItems.PageIndex + 1);
                pageLabel.text = $"/ {searchItems.TotalPage}";
                pageNextButton.SetEnabled(searchItems.PageIndex + 1 < searchItems.TotalPage);
            }

            void RestartSearch(string searchText, bool resetPage, bool immediate = false)
            {
                string safeSearchText = searchText ?? "";
                if (resetPage)
                {
                    searchItems.PageIndex = 0;
                }

                searchItems.Size = context.WrapProp.arraySize;
                searchItems.SourceGenerator?.Dispose();
                searchItems.SourceGenerator = null;

                if (string.IsNullOrEmpty(safeSearchText))
                {
                    SetFullResults();
                    RefreshList();
                    return;
                }

                searchItems.CachedHitTargetIndexes = new List<int>(searchItems.Started
                    ? searchItems.HitTargetIndexes
                    : searchItems.CachedHitTargetIndexes);
                searchItems.HitTargetIndexes.Clear();
                searchItems.SearchText = safeSearchText;
                searchItems.Started = false;
                searchItems.Finished = false;
                searchItems.DebounceSearchTime = immediate
                    ? 0d
                    : EditorApplication.timeSinceStartup + DebounceTime;

                (object callbackParent, ISaintsHashSetEditorTool tool) = ResolveSearchTarget();
                searchItems.SourceGenerator = Search(
                    tool,
                    context.WrapProp,
                    context.ElementType,
                    safeSearchText,
                    defaultSearch,
                    objectSearch,
                    callbackParent,
                    extraSearch ? extraSearchMethod : default
                ).GetEnumerator();
                RefreshList();
            }

            void ManuallySetSize(int size)
            {
                int newSize = Mathf.Max(size, 0);
                if (newSize >= context.WrapProp.arraySize)
                {
                    IncreaseArraySize(newSize, context.WrapProp);
                }
                else
                {
                    DecreaseArraySize(Enumerable.Range(newSize, context.WrapProp.arraySize - newSize)
                        .Reverse().ToArray(), context.WrapProp);
                }
                property.serializedObject.ApplyModifiedProperties();
                RestartSearch(searchItems.SearchText, false, true);
            }

            totalCountTop.SetValueWithoutNotify(context.WrapProp.arraySize);
            totalCountBottom.SetValueWithoutNotify(context.WrapProp.arraySize);
            totalCountTop.TrackPropertyValue(context.WrapProp,
                _ => totalCountTop.SetValueWithoutNotify(context.WrapProp.arraySize));
            totalCountTop.RegisterValueChangedCallback(evt => ManuallySetSize(evt.newValue));
            totalCountBottom.RegisterValueChangedCallback(evt => ManuallySetSize(evt.newValue));

            numberOfItemsPerPage.SetValueWithoutNotify(attribute.NumberOfItemsPerPage);
            numberOfItemsPerPage.RegisterValueChangedCallback(evt =>
            {
                searchItems.NumberOfItemsPerPage = Mathf.Max(evt.newValue, 0);
                searchItems.PageIndex = 0;
                RefreshList();
            });
            pagePreButton.clicked += () =>
            {
                searchItems.PageIndex = Mathf.Max(0, searchItems.PageIndex - 1);
                RefreshList();
            };
            pageField.RegisterValueChangedCallback(evt =>
            {
                searchItems.PageIndex = Mathf.Clamp(evt.newValue - 1, 0, searchItems.TotalPage - 1);
                RefreshList();
            });
            pageNextButton.clicked += () =>
            {
                searchItems.PageIndex = Mathf.Min(searchItems.PageIndex + 1, searchItems.TotalPage - 1);
                RefreshList();
            };

            searchField.RegisterValueChangedCallback(evt => RestartSearch(evt.newValue, true));
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    searchItems.DebounceSearchTime = 0d;
                }
            }, TrickleDown.TrickleDown);

            List<InjectAttributeBase> injectAttributes = new List<InjectAttributeBase>();
            bool hasSerializeReference = UsesReferenceWrap(context.RawType);
            foreach (InjectAttributeBase injectAttribute in ReflectCache.GetCustomAttributes<InjectAttributeBase>(info))
            {
                if (injectAttribute.Decorator == typeof(SerializeReference))
                {
                    hasSerializeReference = true;
                    continue;
                }
                injectAttributes.Add(new ValueAttributeAttribute(injectAttribute.Depth, injectAttribute.Decorator,
                    injectAttribute.Parameters));
            }
            WrapType valueWrapType = SaintsWrapUtils.EnsureWrapType(
                property.FindPropertyRelative("_wrapType"), context.WrapField, hasSerializeReference);
            IReadOnlyList<Attribute> cellAttributes = ReflectCache.GetCustomAttributes<Attribute>(info)
                .Where(each => each is not SaintsHashSetAttribute)
                .Where(each => each is PropertyAttribute or InjectAttributeBase)
                .ToArray();

            listView.makeItem = () => new VisualElement();
            listView.bindItem = (element, elementIndex) =>
            {
                if (elementIndex < 0 || elementIndex >= itemIndexToPropertyIndex.Count)
                {
                    return;
                }

                int propertyIndex = itemIndexToPropertyIndex[elementIndex];
                if (propertyIndex < 0 || propertyIndex >= context.WrapProp.arraySize)
                {
                    return;
                }

                SerializedProperty elementProp = context.WrapProp.GetArrayElementAtIndex(propertyIndex);
                elementProp.isExpanded = true;
                element.Clear();

                VisualElement resultElement = SaintsWrapUtils.CreateCellElement(
                    valueWrapType,
                    context.WrapField,
                    context.WrapType,
                    elementProp,
                    cellAttributes,
                    injectAttributes,
                    hasSerializeReference,
                    this,
                    this,
                    this,
                    context.WrapParent
                );
                ElementField elementField = new ElementField($"Element {propertyIndex}", resultElement);
                element.Add(elementField);
                elementField.TrackPropertyValue(context.WrapProp, _ => RefreshConflict());
                RefreshConflict();

                void RefreshConflict()
                {
                    if (propertyIndex >= context.WrapProp.arraySize)
                    {
                        return;
                    }

                    IEnumerable allValues = context.WrapField.GetValue(context.WrapParent) as IEnumerable;
                    Debug.Assert(allValues != null, $"list {context.WrapField.Name} is null");
                    (object value, int index)[] indexedValues = allValues.Cast<object>().WithIndex().ToArray();
                    object thisValue = indexedValues[propertyIndex].value;
                    foreach ((object otherValue, int _) in indexedValues.Where(each => each.index != propertyIndex))
                    {
                        if (Util.GetIsEqual(otherValue, thisValue))
                        {
                            elementField.style.backgroundColor = WarningColor;
                            return;
                        }
                    }
                    elementField.style.backgroundColor = Color.clear;
                }
            };

            footerButtons.AddButton.clicked += () =>
            {
                IncreaseArraySize(context.WrapProp.arraySize + 1, context.WrapProp);
                property.serializedObject.ApplyModifiedProperties();
                RestartSearch(searchItems.SearchText, false, true);
            };
            footerButtons.RemoveButton.clicked += () =>
            {
                List<int> selected = listView.selectedIndices
                    .Where(each => each >= 0 && each < itemIndexToPropertyIndex.Count)
                    .Select(each => itemIndexToPropertyIndex[each])
                    .OrderByDescending(each => each)
                    .ToList();
                if (selected.Count == 0 && context.WrapProp.arraySize > 0)
                {
                    selected.Add(context.WrapProp.arraySize - 1);
                }
                if (selected.Count == 0)
                {
                    return;
                }

                DecreaseArraySize(selected, context.WrapProp);
                property.serializedObject.ApplyModifiedProperties();
                RestartSearch(searchItems.SearchText, false, true);
            };

            listView.TrackPropertyValue(context.WrapProp, _ =>
            {
                if (searchItems.Size != context.WrapProp.arraySize)
                {
                    RestartSearch(searchItems.SearchText, false, true);
                }
            });
            listView.itemIndexChanged += (first, second) =>
            {
                if (first < 0 || first >= itemIndexToPropertyIndex.Count ||
                    second < 0 || second >= itemIndexToPropertyIndex.Count)
                {
                    return;
                }

                int fromPropertyIndex = itemIndexToPropertyIndex[first];
                int toPropertyIndex = itemIndexToPropertyIndex[second];
                context.WrapProp.MoveArrayElement(fromPropertyIndex, toPropertyIndex);
                property.serializedObject.ApplyModifiedProperties();
                RestartSearch(searchItems.SearchText, false, true);
            };

            void RefreshSearchingStatus()
            {
                RestartSearch(searchItems.SearchText, false, true);
            }

            void SetSearchDisplay(bool enabled)
            {
                if (enabled && !defaultSearch && !extraSearch)
                {
                    defaultSearch = true;
                }

                searchField.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
                if (!enabled)
                {
                    searchField.value = "";
                }
            }

            foldout.MenuButton.clicked += () =>
            {
                GenericDropdownMenu menu = new GenericDropdownMenu();

                bool pagingEnabled = pager.style.display != DisplayStyle.None;
                menu.AddItem("Paging", pagingEnabled, () =>
                {
                    if (pagingEnabled)
                    {
                        pager.style.display = DisplayStyle.None;
                        numberOfItemsPerPage.value = -1;
                    }
                    else
                    {
                        int itemsPerPage = attribute.NumberOfItemsPerPage > 0
                            ? attribute.NumberOfItemsPerPage
                            : Mathf.Max(5, context.WrapProp.arraySize / 2);
                        pager.style.display = DisplayStyle.Flex;
                        numberOfItemsPerPage.value = itemsPerPage;
                    }
                });

                bool searchEnabled = searchField.style.display != DisplayStyle.None;
                menu.AddItem("Search", searchEnabled, () => SetSearchDisplay(!searchEnabled));

                if (searchEnabled && extraSearchMethod.methodInfo != null)
                {
                    menu.AddItem("Default Search", defaultSearch, () =>
                    {
                        defaultSearch = !defaultSearch;
                        if (!defaultSearch && !extraSearch)
                        {
                            SetSearchDisplay(false);
                        }
                        else
                        {
                            RefreshSearchingStatus();
                        }
                    });
                }
                else
                {
                    menu.AddDisabledItem("Default Search", defaultSearch);
                }

                if (searchEnabled && defaultSearch)
                {
                    menu.AddItem("Object Search", objectSearch, () =>
                    {
                        objectSearch = !objectSearch;
                        RefreshSearchingStatus();
                    });
                }
                else
                {
                    menu.AddDisabledItem("Object Search", objectSearch);
                }

                if (extraSearchMethod.methodInfo != null)
                {
                    if (searchEnabled)
                    {
                        menu.AddItem("Extra Search", extraSearch, () =>
                        {
                            extraSearch = !extraSearch;
                            if (!defaultSearch && !extraSearch)
                            {
                                SetSearchDisplay(false);
                            }
                            else
                            {
                                RefreshSearchingStatus();
                            }
                        });
                    }
                    else
                    {
                        menu.AddDisabledItem("Extra Search", extraSearch);
                    }
                }

                Rect menuBound = foldout.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
                menuBound.xMin = menuBound.xMax - Mathf.Max(menuBound.width, 120f);
#endif
                menu.DropDown(menuBound, foldout.MenuButton,
#if UNITY_6000_3_OR_NEWER
                    DropdownMenuSizeMode.Auto
#else
                    true
#endif
                );
            };

            RefreshList();
            listView.schedule.Execute(() =>
            {
                if (!searchItems.Started && searchItems.SourceGenerator != null &&
                    EditorApplication.timeSinceStartup > searchItems.DebounceSearchTime)
                {
                    searchItems.Started = true;
                    RefreshList();
                }

                if (searchItems.Started && !searchItems.Finished && searchItems.SourceGenerator != null)
                {
                    loadingImage.style.visibility = Visibility.Visible;
                    bool needRefresh = false;
                    for (int searchTick = 0; searchTick < 50; searchTick++)
                    {
                        if (searchItems.SourceGenerator.MoveNext())
                        {
                            int result = searchItems.SourceGenerator.Current;
                            if (result != -1)
                            {
                                searchItems.HitTargetIndexes.Add(result);
                                needRefresh = true;
                            }
                        }
                        else
                        {
                            searchItems.Finished = true;
                            searchItems.CachedHitTargetIndexes = new List<int>(searchItems.HitTargetIndexes);
                            searchItems.SourceGenerator.Dispose();
                            searchItems.SourceGenerator = null;
                            needRefresh = true;
                            break;
                        }
                    }
                    if (needRefresh)
                    {
                        RefreshList();
                    }
                }

                if (searchItems.Finished || searchItems.SourceGenerator == null)
                {
                    loadingImage.style.visibility = Visibility.Hidden;
                }
            }).Every(1);
        }
    }
}
#endif
