#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer
{
    public partial class SaintsHashSetDrawer
    {
        private sealed class ValueSearchItems
        {
            public bool Started = true;
            public bool Finished = true;
            public IEnumerator<int> SourceGenerator;
            public string SearchText = "";
            public double DebounceSearchTime;
            public readonly List<int> HitIndexes = new List<int>();
            public readonly List<int> CachedHitIndexes = new List<int>();
            public readonly List<int> VisibleIndexes = new List<int>();
            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;
        }

        private sealed class HashSetViewPayload
        {
            public object RawSetValue;
            public readonly Type ElementType;
            public readonly MethodInfo AddMethod;
            public readonly MethodInfo RemoveMethod;
            public readonly MethodInfo ContainsMethod;
            public readonly ValueSearchItems SearchItems;
            public readonly List<object> Values = new List<object>();
            public Action<object> BeforeSet;
            public Action<object> SetterOrNull;
            public bool ObjectSearch;
            public bool DefaultSearch = true;
            public bool ExtraSearch;
            public object ExtraSearchTarget;
            public (MethodInfo methodInfo, SearchParamType paramType) ExtraSearchMethod;

            public HashSetViewPayload(object rawSetValue, Type elementType, Type setInterface,
                ValueSearchItems searchItems)
            {
                RawSetValue = rawSetValue;
                ElementType = elementType;
                SearchItems = searchItems;

                Type collectionInterface = typeof(ICollection<>).MakeGenericType(elementType);
                AddMethod = setInterface.GetMethod("Add", new[] { elementType });
                RemoveMethod = collectionInterface.GetMethod("Remove", new[] { elementType });
                ContainsMethod = collectionInterface.GetMethod("Contains", new[] { elementType });
                ReloadValues();
            }

            public void ReloadValues()
            {
                Values.Clear();
                if (!RuntimeUtil.IsNull(RawSetValue))
                {
                    Values.AddRange(((IEnumerable)RawSetValue).Cast<object>());
                }
            }

            public bool Contains(object value) =>
                !RuntimeUtil.IsNull(RawSetValue) && (bool)ContainsMethod.Invoke(RawSetValue, new[] { value });

            public bool Add(object value) =>
                !RuntimeUtil.IsNull(RawSetValue) && (bool)AddMethod.Invoke(RawSetValue, new[] { value });

            public bool Remove(object value) =>
                !RuntimeUtil.IsNull(RawSetValue) && (bool)RemoveMethod.Invoke(RawSetValue, new[] { value });
        }

        private sealed class HashSetItemPanel : VisualElement
        {
            public readonly UnityEvent<bool, object> OnFinished = new UnityEvent<bool, object>();

            public HashSetItemPanel(Type elementType, HashSetViewPayload payload, bool inHorizontalLayout,
                IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
            {
                const int borderWidth = 1;
                Color borderColor = EColor.EditorEmphasized.GetColor();
                style.display = DisplayStyle.None;
                style.borderLeftWidth = borderWidth;
                style.borderRightWidth = borderWidth;
                style.borderTopWidth = borderWidth;
                style.borderBottomWidth = borderWidth;
                style.borderTopColor = borderColor;
                style.borderBottomColor = borderColor;
                style.borderLeftColor = borderColor;
                style.borderRightColor = borderColor;
                style.marginTop = 1;
                style.marginBottom = 1;
                style.marginLeft = 1;
                style.marginRight = 1;

                Button confirmButton = new Button
                {
                    text = "OK",
                    style = { flexGrow = 1 },
                };
                Button cancelButton = new Button(() =>
                {
                    style.display = DisplayStyle.None;
                    OnFinished.Invoke(false, null);
                })
                {
                    text = "Cancel",
                    style = { flexGrow = 1 },
                };

                VisualElement valueContainer = new VisualElement();
                Add(valueContainer);
                object candidate = CreateValueEditDefault(elementType);
                bool candidateChanged = true;
                valueContainer.schedule.Execute(() =>
                {
                    if (!candidateChanged)
                    {
                        return;
                    }
                    candidateChanged = false;

                    VisualElement editing = UIToolkitEdit.UIToolkitValueEdit(
                        valueContainer.Children().FirstOrDefault(),
                        "Value",
                        elementType,
                        candidate,
                        null,
                        newValue =>
                        {
                            candidate = newValue;
                            candidateChanged = true;
                            confirmButton.SetEnabled(!payload.Contains(candidate));
                        },
                        false,
                        inHorizontalLayout,
                        Array.Empty<Attribute>(),
                        targets,
                        richTextTagProvider,
                        $"{foldoutViewKey}.[value]"
                    ).result;
                    if (editing != null)
                    {
                        valueContainer.Clear();
                        valueContainer.Add(editing);
                    }
                    confirmButton.SetEnabled(!payload.Contains(candidate));
                }).Every(100);

                VisualElement actions = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    },
                };
                actions.Add(confirmButton);
                actions.Add(cancelButton);
                Add(actions);

                confirmButton.clicked += () =>
                {
                    if (payload.Contains(candidate))
                    {
                        return;
                    }
                    style.display = DisplayStyle.None;
                    OnFinished.Invoke(true, candidate);
                    candidate = CreateValueEditDefault(elementType);
                    candidateChanged = true;
                };
            }
        }

        private sealed class SaintsHashSetWrapper : VisualElement
        {
            public readonly CollectionFoldout Foldout;
            public readonly ToolbarSearchField SearchField;
            public readonly Image LoadingImage;
            public readonly ListView ListView;
            public readonly ListViewPagerElement Pager;
            public readonly ListViewFooterButtonsElement FooterButtons;
            public readonly HashSetItemPanel ItemPanel;

            public SaintsHashSetWrapper(string label, HashSetViewPayload payload, bool inHorizontalLayout,
                IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
            {
                Add(Foldout = new CollectionFoldout(label)
                {
                    viewDataKey = foldoutViewKey,
                });
                Foldout.contentContainer.style.marginLeft = 0;

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
                Foldout.Add(SearchField);

                ListView = new ListView
                {
                    selectionType = SelectionType.Multiple,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    showBoundCollectionSize = false,
                    showFoldoutHeader = false,
                    showAddRemoveFooter = false,
                    reorderable = false,
                    showBorder = true,
                };
                Foldout.Add(ListView);

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
                Foldout.Add(footer);

                ItemPanel = new HashSetItemPanel(payload.ElementType, payload, inHorizontalLayout, targets,
                    richTextTagProvider, $"{foldoutViewKey}.[add.panel]");
                Foldout.Add(ItemPanel);
            }
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType,
            object rawSetValue, Type setInterface, Type elementType, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            if (oldElement is SaintsHashSetWrapper oldWrapper)
            {
                HashSetViewPayload oldPayload = (HashSetViewPayload)oldWrapper.ListView.userData;
                oldPayload.RawSetValue = rawSetValue;
                oldPayload.BeforeSet = beforeSet;
                oldPayload.SetterOrNull = setterOrNull;
                oldPayload.ReloadValues();
                RestartValueSearch(oldWrapper, oldPayload, oldPayload.SearchItems.SearchText, false, true);
                return null;
            }

            SaintsHashSetAttribute attribute = allAttributes.OfType<SaintsHashSetAttribute>().FirstOrDefault() ??
                                               new SaintsHashSetAttribute(searchable: false);
            object extraSearchTarget = targets.FirstOrDefault(each => each != null);
            (MethodInfo methodInfo, SearchParamType paramType) extraSearchMethod = default;
            if (!string.IsNullOrEmpty(attribute.ExtraSearch) && extraSearchTarget != null)
            {
                extraSearchMethod = GetSearchMethodInfo(attribute.ExtraSearch, extraSearchTarget.GetType(),
                    elementType);
            }
            Debug.Assert(string.IsNullOrEmpty(attribute.ExtraSearch) || extraSearchTarget != null,
                $"extraSearch target not found for `{attribute.ExtraSearch}`");
            Debug.Assert(string.IsNullOrEmpty(attribute.ExtraSearch) || extraSearchMethod.methodInfo != null,
                $"extraSearchMethod `{attribute.ExtraSearch}` not found for {elementType}");

            List<object> initialValues = ((IEnumerable)rawSetValue).Cast<object>().ToList();
            ValueSearchItems searchItems = new ValueSearchItems
            {
                Size = initialValues.Count,
                NumberOfItemsPerPage = attribute.NumberOfItemsPerPage,
            };
            searchItems.HitIndexes.AddRange(Enumerable.Range(0, initialValues.Count));
            searchItems.CachedHitIndexes.AddRange(searchItems.HitIndexes);

            HashSetViewPayload payload = new HashSetViewPayload(rawSetValue, elementType, setInterface, searchItems)
            {
                BeforeSet = beforeSet,
                SetterOrNull = setterOrNull,
                ObjectSearch = attribute.ObjectSearch,
                DefaultSearch = true,
                ExtraSearch = extraSearchMethod.methodInfo != null,
                ExtraSearchTarget = extraSearchTarget,
                ExtraSearchMethod = extraSearchMethod,
            };
            SaintsHashSetWrapper wrapper = new SaintsHashSetWrapper(label, payload, inHorizontalLayout, targets,
                richTextTagProvider, foldoutViewKey);
            wrapper.ListView.userData = payload;
            wrapper.SearchField.style.display = attribute.Searchable ? DisplayStyle.Flex : DisplayStyle.None;
            wrapper.Pager.style.display = attribute.NumberOfItemsPerPage > 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            wrapper.Pager.NumberOfItemsPerPageField.SetValueWithoutNotify(attribute.NumberOfItemsPerPage);

            bool editable = setterOrNull != null;
            wrapper.Foldout.ArraySizeField.SetEnabled(editable);
            wrapper.Pager.NumberOfItemsTotalField.SetEnabled(editable);
            wrapper.FooterButtons.SetEnabled(editable);

            IReadOnlyList<Attribute> elementAttributes = allAttributes
                .Where(each => each is not SaintsHashSetAttribute)
                .ToArray();
            wrapper.ListView.makeItem = () => new VisualElement();
            wrapper.ListView.bindItem = (element, elementIndex) =>
            {
                if (elementIndex < 0 || elementIndex >= payload.SearchItems.VisibleIndexes.Count)
                {
                    return;
                }

                int valueIndex = payload.SearchItems.VisibleIndexes[elementIndex];
                if (valueIndex < 0 || valueIndex >= payload.Values.Count)
                {
                    return;
                }

                object oldValue = payload.Values[valueIndex];
                VisualElement editing = UIToolkitEdit.UIToolkitValueEdit(
                    element.Children().FirstOrDefault(),
                    $"Element {valueIndex}",
                    elementType,
                    oldValue,
                    null,
                    editable
                        ? newValue => ReplaceValue(wrapper, payload, oldValue, newValue)
                        : null,
                    labelGrayColor,
                    inHorizontalLayout,
                    elementAttributes,
                    targets,
                    richTextTagProvider,
                    $"{foldoutViewKey}.[{valueIndex}]"
                ).result;
                if (editing != null)
                {
                    element.Clear();
                    element.Add(editing);
                }
            };

            void ChangeSize(ChangeEvent<int> evt)
            {
                int targetCount = Mathf.Max(evt.newValue, 0);
                int currentCount = payload.Values.Count;
                if (targetCount == currentCount)
                {
                    return;
                }

                if (targetCount > currentCount)
                {
                    wrapper.ItemPanel.style.display = DisplayStyle.Flex;
                    wrapper.FooterButtons.AddButton.SetEnabled(false);
                    wrapper.Foldout.ArraySizeField.SetValueWithoutNotify(currentCount);
                    wrapper.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(currentCount);
                    return;
                }

                foreach (object value in payload.Values.Skip(targetCount).Reverse().ToArray())
                {
                    RemoveValue(payload, value);
                }
                FinishValueMutation(wrapper, payload);
            }

            wrapper.Foldout.ArraySizeField.RegisterValueChangedCallback(ChangeSize);
            wrapper.Pager.NumberOfItemsTotalField.RegisterValueChangedCallback(ChangeSize);
            wrapper.FooterButtons.AddButton.clicked += () =>
            {
                wrapper.ItemPanel.style.display = DisplayStyle.Flex;
                wrapper.FooterButtons.AddButton.SetEnabled(false);
            };
            wrapper.ItemPanel.OnFinished.AddListener((added, value) =>
            {
                wrapper.FooterButtons.AddButton.SetEnabled(true);
                if (!added || payload.Contains(value))
                {
                    return;
                }

                payload.BeforeSet?.Invoke(payload.RawSetValue);
                if (payload.Add(value))
                {
                    payload.SetterOrNull?.Invoke(payload.RawSetValue);
                }
                FinishValueMutation(wrapper, payload);
            });
            wrapper.FooterButtons.RemoveButton.clicked += () =>
            {
                List<int> selected = wrapper.ListView.selectedIndices
                    .Where(each => each >= 0 && each < payload.SearchItems.VisibleIndexes.Count)
                    .Select(each => payload.SearchItems.VisibleIndexes[each])
                    .Distinct()
                    .OrderByDescending(each => each)
                    .ToList();
                if (selected.Count == 0 && payload.Values.Count > 0)
                {
                    selected.Add(payload.Values.Count - 1);
                }
                if (selected.Count == 0)
                {
                    return;
                }

                payload.BeforeSet?.Invoke(payload.RawSetValue);
                foreach (int valueIndex in selected)
                {
                    if (valueIndex >= 0 && valueIndex < payload.Values.Count)
                    {
                        payload.Remove(payload.Values[valueIndex]);
                    }
                }
                payload.SetterOrNull?.Invoke(payload.RawSetValue);
                FinishValueMutation(wrapper, payload);
            };

            wrapper.SearchField.RegisterValueChangedCallback(evt =>
                RestartValueSearch(wrapper, payload, evt.newValue, true));
            wrapper.SearchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    payload.SearchItems.DebounceSearchTime = 0d;
                }
            }, TrickleDown.TrickleDown);
            wrapper.Pager.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt =>
            {
                payload.SearchItems.NumberOfItemsPerPage = Mathf.Max(evt.newValue, 0);
                payload.SearchItems.PageIndex = 0;
                RefreshValueField(wrapper, payload);
            });
            wrapper.Pager.PagePreButton.clicked += () =>
            {
                payload.SearchItems.PageIndex = Mathf.Max(0, payload.SearchItems.PageIndex - 1);
                RefreshValueField(wrapper, payload);
            };
            wrapper.Pager.PageField.RegisterValueChangedCallback(evt =>
            {
                payload.SearchItems.PageIndex = Mathf.Clamp(evt.newValue - 1, 0,
                    payload.SearchItems.TotalPage - 1);
                RefreshValueField(wrapper, payload);
            });
            wrapper.Pager.PageNextButton.clicked += () =>
            {
                payload.SearchItems.PageIndex = Mathf.Min(payload.SearchItems.PageIndex + 1,
                    payload.SearchItems.TotalPage - 1);
                RefreshValueField(wrapper, payload);
            };

            wrapper.Foldout.MenuButton.clicked += () => ShowValueMenu(wrapper, payload, attribute);
            wrapper.schedule.Execute(() => TickValueSearch(wrapper, payload)).Every(1);
            RefreshValueField(wrapper, payload);
            return wrapper;
        }

        private static void ShowValueMenu(SaintsHashSetWrapper wrapper, HashSetViewPayload payload,
            SaintsHashSetAttribute attribute)
        {
            GenericDropdownMenu menu = new GenericDropdownMenu();
            if (payload.SetterOrNull == null)
            {
                menu.AddDisabledItem("Set To Null", false);
            }
            else
            {
                menu.AddItem("Set To Null", false, () =>
                {
                    payload.BeforeSet?.Invoke(payload.RawSetValue);
                    payload.SetterOrNull(null);
                });
            }
            menu.AddSeparator("");

            bool paging = wrapper.Pager.style.display != DisplayStyle.None;
            menu.AddItem("Paging", paging, () =>
            {
                if (paging)
                {
                    wrapper.Pager.style.display = DisplayStyle.None;
                    wrapper.Pager.NumberOfItemsPerPageField.value = -1;
                }
                else
                {
                    int numberOfItemsPerPage = attribute.NumberOfItemsPerPage > 0
                        ? attribute.NumberOfItemsPerPage
                        : Mathf.Max(5, payload.Values.Count / 2);
                    wrapper.Pager.style.display = DisplayStyle.Flex;
                    wrapper.Pager.NumberOfItemsPerPageField.value = numberOfItemsPerPage;
                }
            });

            bool searching = wrapper.SearchField.style.display != DisplayStyle.None;
            menu.AddItem("Search", searching, () =>
            {
                if (!searching && !payload.DefaultSearch && !payload.ExtraSearch)
                {
                    payload.DefaultSearch = true;
                }
                wrapper.SearchField.style.display = searching ? DisplayStyle.None : DisplayStyle.Flex;
                if (searching)
                {
                    wrapper.SearchField.value = "";
                }
            });

            bool hasExtraSearch = payload.ExtraSearchMethod.methodInfo != null;
            if (searching && hasExtraSearch)
            {
                menu.AddItem("Default Search", payload.DefaultSearch, () =>
                {
                    payload.DefaultSearch = !payload.DefaultSearch;
                    if (!payload.DefaultSearch && !payload.ExtraSearch)
                    {
                        wrapper.SearchField.style.display = DisplayStyle.None;
                        wrapper.SearchField.value = "";
                    }
                    else
                    {
                        RestartValueSearch(wrapper, payload, payload.SearchItems.SearchText, false, true);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem("Default Search", payload.DefaultSearch);
            }

            if (searching && payload.DefaultSearch)
            {
                menu.AddItem("Object Search", payload.ObjectSearch, () =>
                {
                    payload.ObjectSearch = !payload.ObjectSearch;
                    RestartValueSearch(wrapper, payload, payload.SearchItems.SearchText, false, true);
                });
            }
            else
            {
                menu.AddDisabledItem("Object Search", payload.ObjectSearch);
            }

            if (hasExtraSearch)
            {
                if (searching)
                {
                    menu.AddItem("Extra Search", payload.ExtraSearch, () =>
                    {
                        payload.ExtraSearch = !payload.ExtraSearch;
                        if (!payload.DefaultSearch && !payload.ExtraSearch)
                        {
                            wrapper.SearchField.style.display = DisplayStyle.None;
                            wrapper.SearchField.value = "";
                        }
                        else
                        {
                            RestartValueSearch(wrapper, payload, payload.SearchItems.SearchText, false, true);
                        }
                    });
                }
                else
                {
                    menu.AddDisabledItem("Extra Search", payload.ExtraSearch);
                }
            }

            Rect menuBound = wrapper.Foldout.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
            menuBound.xMin = menuBound.xMax - Mathf.Max(menuBound.width, 120f);
#endif
            menu.DropDown(menuBound, wrapper.Foldout.MenuButton,
#if UNITY_6000_3_OR_NEWER
                DropdownMenuSizeMode.Auto
#else
                true
#endif
            );
        }

        private static void ReplaceValue(SaintsHashSetWrapper wrapper, HashSetViewPayload payload,
            object oldValue, object newValue)
        {
            if (Util.GetIsEqual(oldValue, newValue))
            {
                return;
            }
            if (payload.Contains(newValue))
            {
                Debug.LogWarning($"Setting hash set value {oldValue} to existing value {newValue} is ignored");
                wrapper.ListView.Rebuild();
                return;
            }

            payload.BeforeSet?.Invoke(payload.RawSetValue);
            if (!payload.Remove(oldValue))
            {
                wrapper.ListView.Rebuild();
                return;
            }
            if (!payload.Add(newValue))
            {
                payload.Add(oldValue);
                wrapper.ListView.Rebuild();
                return;
            }
            payload.SetterOrNull?.Invoke(payload.RawSetValue);
            FinishValueMutation(wrapper, payload);
        }

        private static void RemoveValue(HashSetViewPayload payload, object value)
        {
            payload.BeforeSet?.Invoke(payload.RawSetValue);
            if (payload.Remove(value))
            {
                payload.SetterOrNull?.Invoke(payload.RawSetValue);
            }
        }

        private static void FinishValueMutation(SaintsHashSetWrapper wrapper, HashSetViewPayload payload)
        {
            payload.ReloadValues();
            RestartValueSearch(wrapper, payload, payload.SearchItems.SearchText, false, true);
        }

        private static void RestartValueSearch(SaintsHashSetWrapper wrapper, HashSetViewPayload payload,
            string searchText, bool resetPage, bool immediate = false)
        {
            ValueSearchItems searchItems = payload.SearchItems;
            string safeSearchText = searchText ?? "";
            if (resetPage)
            {
                searchItems.PageIndex = 0;
            }

            searchItems.Size = payload.Values.Count;
            searchItems.SourceGenerator?.Dispose();
            searchItems.SourceGenerator = null;
            if (string.IsNullOrEmpty(safeSearchText))
            {
                searchItems.SearchText = "";
                searchItems.Started = true;
                searchItems.Finished = true;
                searchItems.HitIndexes.Clear();
                searchItems.HitIndexes.AddRange(Enumerable.Range(0, payload.Values.Count));
                searchItems.CachedHitIndexes.Clear();
                searchItems.CachedHitIndexes.AddRange(searchItems.HitIndexes);
                RefreshValueField(wrapper, payload);
                return;
            }

            IReadOnlyList<int> current = searchItems.Started
                ? searchItems.HitIndexes
                : searchItems.CachedHitIndexes;
            searchItems.CachedHitIndexes.Clear();
            searchItems.CachedHitIndexes.AddRange(current);
            searchItems.HitIndexes.Clear();
            searchItems.SearchText = safeSearchText;
            searchItems.Started = false;
            searchItems.Finished = false;
            searchItems.DebounceSearchTime = immediate
                ? 0d
                : EditorApplication.timeSinceStartup + DebounceTime;
            searchItems.SourceGenerator = SearchValuePayload(payload, safeSearchText).GetEnumerator();
            RefreshValueField(wrapper, payload);
        }

        private static IEnumerable<int> SearchValuePayload(HashSetViewPayload payload, string searchText)
        {
            IReadOnlyList<ListSearchToken> tokens = SerializedUtils.ParseSearch(searchText).ToArray();
            for (int index = 0; index < payload.Values.Count; index++)
            {
                object value = payload.Values[index];
                bool matched = payload.DefaultSearch &&
                               Util.SearchObjectWithTokens(value, tokens, payload.ObjectSearch);
                if (!matched && payload.ExtraSearch && payload.ExtraSearchMethod.methodInfo != null)
                {
                    object[] methodParams = payload.ExtraSearchMethod.paramType switch
                    {
                        SearchParamType.Index => new object[] { index, tokens },
                        SearchParamType.Target => new[] { value, tokens },
                        _ => new[] { value, index, tokens },
                    };
                    matched = (bool)payload.ExtraSearchMethod.methodInfo.Invoke(payload.ExtraSearchTarget,
                        methodParams);
                }
                yield return matched ? index : -1;
            }
        }

        private static void TickValueSearch(SaintsHashSetWrapper wrapper, HashSetViewPayload payload)
        {
            ValueSearchItems searchItems = payload.SearchItems;
            if (!searchItems.Started && searchItems.SourceGenerator != null &&
                EditorApplication.timeSinceStartup > searchItems.DebounceSearchTime)
            {
                searchItems.Started = true;
                RefreshValueField(wrapper, payload);
            }

            if (searchItems.Started && !searchItems.Finished && searchItems.SourceGenerator != null)
            {
                wrapper.LoadingImage.style.visibility = Visibility.Visible;
                bool needRefresh = false;
                for (int tick = 0; tick < 50; tick++)
                {
                    if (searchItems.SourceGenerator.MoveNext())
                    {
                        int index = searchItems.SourceGenerator.Current;
                        if (index != -1)
                        {
                            searchItems.HitIndexes.Add(index);
                            needRefresh = true;
                        }
                    }
                    else
                    {
                        searchItems.Finished = true;
                        searchItems.CachedHitIndexes.Clear();
                        searchItems.CachedHitIndexes.AddRange(searchItems.HitIndexes);
                        searchItems.SourceGenerator.Dispose();
                        searchItems.SourceGenerator = null;
                        needRefresh = true;
                        break;
                    }
                }
                if (needRefresh)
                {
                    RefreshValueField(wrapper, payload);
                }
            }

            if (searchItems.Finished || searchItems.SourceGenerator == null)
            {
                wrapper.LoadingImage.style.visibility = Visibility.Hidden;
            }
        }

        private static void RefreshValueField(SaintsHashSetWrapper wrapper, HashSetViewPayload payload)
        {
            ValueSearchItems searchItems = payload.SearchItems;
            IReadOnlyList<int> results = searchItems.Started
                ? searchItems.HitIndexes
                : searchItems.CachedHitIndexes;
            int pageCount;
            int pageIndex;
            int skip;
            int take;
            if (searchItems.NumberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                pageIndex = 0;
                skip = 0;
                take = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.Max(1,
                    Mathf.CeilToInt(results.Count / (float)searchItems.NumberOfItemsPerPage));
                pageIndex = Mathf.Clamp(searchItems.PageIndex, 0, pageCount - 1);
                skip = pageIndex * searchItems.NumberOfItemsPerPage;
                take = searchItems.NumberOfItemsPerPage;
            }

            searchItems.PageIndex = pageIndex;
            searchItems.TotalPage = pageCount;
            searchItems.VisibleIndexes.Clear();
            searchItems.VisibleIndexes.AddRange(results.Where(each => each >= 0 && each < payload.Values.Count)
                .Skip(skip).Take(take));

            wrapper.ListView.itemsSource = searchItems.VisibleIndexes.ToList();
            wrapper.ListView.Rebuild();
            wrapper.Foldout.ArraySizeField.SetValueWithoutNotify(payload.Values.Count);
            wrapper.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(payload.Values.Count);
            wrapper.Pager.PagePreButton.SetEnabled(pageIndex > 0);
            wrapper.Pager.PageField.SetValueWithoutNotify(pageIndex + 1);
            wrapper.Pager.PageLabel.text = $"/ {pageCount}";
            wrapper.Pager.PageNextButton.SetEnabled(pageIndex + 1 < pageCount);
        }

        private static object CreateValueEditDefault(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }
            if (type == typeof(Guid))
            {
                return Guid.NewGuid();
            }
            if (type?.IsEnum == true)
            {
                Array values = Enum.GetValues(type);
                return values.Length == 0 ? Activator.CreateInstance(type) : values.GetValue(0);
            }
            return type?.IsValueType == true ? Activator.CreateInstance(type) : null;
        }
    }
}
#endif
