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

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        private class DictionaryViewPayload
        {
            public object RawDictValue;
            private readonly PropertyInfo _keysProperty;
            private readonly PropertyInfo _indexerProperty;
            private readonly MethodInfo _removeMethod;
            private readonly MethodInfo _containersKeyMethod;
            public readonly AsyncSearchItems<object> AsyncSearchItems;

            public VisualElement KeyLoadingImage;
            public VisualElement ValueLoadingImage;
            public VisualElement KeySearchRoot;
            public VisualElement ValueSearchRoot;
            public ToolbarSearchField KeySearchField;
            public ToolbarSearchField ValueSearchField;
            public bool ObjectNestedSearch;
            public List<object> itemIndexToKeys;

            public DictionaryViewPayload(object rawDictValue, PropertyInfo keysProperty, PropertyInfo indexerProperty,
                MethodInfo removeMethod, MethodInfo containsKeyMethod, AsyncSearchItems<object> asyncSearchItems)
            {
                RawDictValue = rawDictValue;
                _keysProperty = keysProperty;
                _indexerProperty = indexerProperty;
                _removeMethod = removeMethod;
                _containersKeyMethod = containsKeyMethod;
                AsyncSearchItems = asyncSearchItems;
            }

            public IEnumerable<object> GetKeys() => ((IEnumerable)_keysProperty.GetValue(RawDictValue)).Cast<object>();

            public object GetValue(object key) => _indexerProperty.GetValue(RawDictValue, new[] { key });
            public void DeleteKey(object key) => _removeMethod.Invoke(RawDictValue, new[] { key });
            public void SetKeyValue(object key, object value) => _indexerProperty.SetValue(RawDictValue, value, new[] { key });
            public bool ContainsKey(object key) => (bool)_containersKeyMethod.Invoke(RawDictValue, new[] { key });
        }


        private class PairPanel : VisualElement
        {
            // true: add; false: cancel
            // key
            // value
            public readonly UnityEvent<bool, object, object> OnFinished = new UnityEvent<bool, object, object>();

            public PairPanel(Type dictKeyType, Type dictValueType, DictionaryViewPayload payload, bool inHorizontalLayout, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
            {
                const int pairPanelBorderWidth = 1;
                Color pairPanelBorderColor = EColor.EditorEmphasized.GetColor();
                style.display = DisplayStyle.None;

                style.borderLeftWidth = pairPanelBorderWidth;
                style.borderRightWidth = pairPanelBorderWidth;
                style.borderTopWidth = pairPanelBorderWidth;
                style.borderBottomWidth = pairPanelBorderWidth;

                style.borderTopColor = pairPanelBorderColor;
                style.borderBottomColor = pairPanelBorderColor;
                style.borderLeftColor = pairPanelBorderColor;
                style.borderRightColor = pairPanelBorderColor;

                style.marginTop = 1;
                style.marginBottom = 1;
                style.marginLeft = 1;
                style.marginRight = 1;

                VisualElement addPairActionContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    },
                };

                Button addPairConfirmButton = new Button
                {
                    text = "OK",
                    style =
                    {
                        flexGrow = 1,
                    },
                };
                addPairActionContainer.Add(addPairConfirmButton);
                Button addPairCancelButton = new Button(() =>
                {
                    OnFinished.Invoke(false, null, null);
                    style.display = DisplayStyle.None;
                })
                {
                    text = "Cancel",
                    style =
                    {
                        flexGrow = 1,
                    },
                };
                addPairActionContainer.Add(addPairCancelButton);

                VisualElement addPairKeyContainer = new VisualElement();
                Add(addPairKeyContainer);
                object addPairKey = dictKeyType.IsValueType ? Activator.CreateInstance(dictKeyType) : null;
                bool addPairKeyChange = true;
                addPairKeyContainer.schedule.Execute(() =>
                {
                    if (!addPairKeyChange)
                    {
                        return;
                    }

                    VisualElement r = UIToolkitEdit.UIToolkitValueEdit(
                        addPairKeyContainer.Children().FirstOrDefault(),
                        "Key",
                        dictKeyType,
                        addPairKey,
                        null,
                        newKey =>
                        {
                            bool invalidKey = RuntimeUtil.IsNull(newKey);
                            if (!invalidKey)
                            {
                                invalidKey = payload.ContainsKey(newKey);
                            }

                            addPairConfirmButton.SetEnabled(!invalidKey);
                            if (!invalidKey)
                            {
                                addPairKey = newKey;
                                addPairKeyChange = true;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                                Debug.Log($"set new pair key {newKey}");
#endif
                            }
                        },
                        false,
                        inHorizontalLayout,
                        Array.Empty<Attribute>(),
                        targets,
                        richTextTagProvider,
                        $"{foldoutViewKey}.[panel.add.key]"
                    ).result;
                    // ReSharper disable once InvertIf
                    if (r != null)
                    {
                        addPairKeyContainer.Clear();
                        addPairKeyContainer.Add(r);
                    }

                    addPairKeyChange = false;
                }).Every(100);

                VisualElement addPairValueContainer = new VisualElement();
                Add(addPairValueContainer);
                object addPairValue = dictValueType.IsValueType ? Activator.CreateInstance(dictValueType) : null;
                bool addPairValueChanged = true;
                addPairValueContainer.schedule.Execute(() =>
                {
                    if (!addPairValueChanged)
                    {
                        return;
                    }

                    VisualElement r = UIToolkitEdit.UIToolkitValueEdit(
                        addPairValueContainer.Children().FirstOrDefault(),
                        "Value",
                        dictValueType,
                        addPairValue,
                        null,
                        newValue =>
                        {
                            addPairValue = newValue;
                            addPairValueChanged = true;
                        },
                        false,
                        inHorizontalLayout,
                        Array.Empty<Attribute>(),
                        targets,
                        richTextTagProvider,
                        $"{foldoutViewKey}.[panel.add.value]"
                    ).result;
                    // ReSharper disable once InvertIf
                    if (r != null)
                    {
                        addPairValueContainer.Clear();
                        addPairValueContainer.Add(r);
                    }

                    addPairValueChanged = false;
                }).Every(100);

                addPairConfirmButton.clicked += () =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                    Debug.Log($"dictionary set {addPairKey} -> {addPairValue}");
#endif
                    // payload.SetKeyValue(addPairKey, addPairValue);
                    style.display = DisplayStyle.None;
                    OnFinished.Invoke(true, addPairKey, addPairValue);
                    // listViewAddButton.SetEnabled(true);
                    // listView.itemsSource = payload.GetKeys().ToList();
                    // // setterOrNull(payload.RawDictValue);
                    // // listView.Rebuild();
                };

                Add(addPairActionContainer);
            }
        }

        private class SaintsDictionaryWrapper : VisualElement
        {
            public readonly CollectionFoldout Foldout;
            public readonly IntegerField ArraySizeField;
            public readonly MultiColumnListView ListView;
            public readonly ListViewPagerElement Pager;
            public readonly ListViewFooterButtonsElement FooterButtons;
            public readonly PairPanel PairPanel;

            public SaintsDictionaryWrapper(string label, MultiColumnListView listView, Type dictKeyType, Type dictValueType, DictionaryViewPayload payload, bool inHorizontalLayout, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
            {
                // Debug.Log(label);

                Add(Foldout = new CollectionFoldout(label)
                {
                    viewDataKey = foldoutViewKey,
                });
                ArraySizeField = Foldout.ArraySizeField;

                // body
                Foldout.Add(ListView = listView);

                // footer
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

                // panel for adding
                Foldout.Add(PairPanel = new PairPanel(dictKeyType, dictValueType, payload, inHorizontalLayout, targets, richTextTagProvider, $"{foldoutViewKey}.[add.panel]"));

                FooterButtons.AddButton.clicked += () =>
                {
                    PairPanel.style.display = DisplayStyle.Flex;
                    FooterButtons.AddButton.SetEnabled(false);
                };
                PairPanel.OnFinished.AddListener((_, _, _) =>
                {
                    FooterButtons.AddButton.SetEnabled(true);
                });
            }

        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object rawDictValue,
            bool isReadOnly, Type dictKeyType, Type dictValueType, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {

            if (oldElement is SaintsDictionaryWrapper dictField)
            {
                DictionaryViewPayload oldPayload = (DictionaryViewPayload)dictField.ListView.userData;
                oldPayload.RawDictValue = rawDictValue;
                int totalCount = oldPayload.GetKeys().Count();
                dictField.ArraySizeField.SetValueWithoutNotify(totalCount);
                dictField.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(totalCount);

                RefreshFieldWithPayload(dictField, (DictionaryViewPayload)dictField.ListView.userData);

                return null;
            }

            PropertyInfo keysProperty = valueType.GetProperty("Keys");
            Debug.Assert(keysProperty != null, $"Failed to get keys from {valueType}");

            PropertyInfo indexerProperty = valueType.GetProperty("Item", new []{dictKeyType});
            Debug.Assert(keysProperty != null, $"Failed to get key indexer from {valueType}");

            MethodInfo removeMethod = valueType.GetMethod("Remove", new[]{dictKeyType});
            Debug.Assert(keysProperty != null, $"Failed to get `Remove` function from {valueType}");

            MethodInfo containsKeyMethod = valueType.GetMethod("ContainsKey", new[]{dictKeyType});

            Debug.Assert(rawDictValue != null, "Dictionary value should not be null");

            SaintsDictionaryAttribute saintsDictionaryAttribute = allAttributes.OfType<SaintsDictionaryAttribute>().FirstOrDefault()
                                                                  ?? new SaintsDictionaryAttribute(searchable: false, numberOfItemsPerPage: 0);

            int initNumberOfItemsPerPage = saintsDictionaryAttribute.NumberOfItemsPerPage;
            List<object> initKeys = ((IEnumerable)keysProperty.GetValue(rawDictValue)).Cast<object>().ToList();
            int initCount = initKeys.Count;

            AsyncSearchItems<object> asyncSearchItems = new AsyncSearchItems<object>
            {
                Started = true,
                Finished = true,
                SourceGenerator = null,
                HitTargetIndexes = new List<object>(initKeys),
                CachedHitTargetIndexes = new List<object>(initKeys),
                KeySearchText = "",
                ValueSearchText = "",
                DebounceSearchTime = 0,
                Size = initCount,
                TotalPage = 1,
                NumberOfItemsPerPage = initNumberOfItemsPerPage,
            };

            DictionaryViewPayload payload = new DictionaryViewPayload(rawDictValue, keysProperty, indexerProperty, removeMethod, containsKeyMethod, asyncSearchItems)
            {
                itemIndexToKeys = initKeys,
                ObjectNestedSearch = saintsDictionaryAttribute.ObjectSearch,
            };

            dictField = new SaintsDictionaryWrapper(label, new MultiColumnListView
            {
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                // showAddRemoveFooter = !isReadOnly,
                showAddRemoveFooter = false,
                // reorderMode = ListViewReorderMode.Animated,
                reorderable = false,
                showBorder = true,
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
                itemsSource = payload.GetKeys().ToList(),
                userData = payload,
            }, dictKeyType, dictValueType, payload, inHorizontalLayout, targets, richTextTagProvider, $"{foldoutViewKey}.[dict]");
            if (labelGrayColor)
            {
                dictField.Q<Label>().style.color = AbsRenderer.ReColor;
            }

            // Size
            if (isReadOnly)
            {
                dictField.ArraySizeField.SetEnabled(false);
                dictField.Pager.NumberOfItemsTotalField.SetEnabled(false);
                dictField.FooterButtons.SetEnabled(false);
            }
            dictField.ArraySizeField.SetValueWithoutNotify(initCount);
            dictField.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(initCount);

            void ChangeSize(ChangeEvent<int> evt)
            {
                int toCount = evt.newValue;
                object[] keys = payload.GetKeys().ToArray();
                int curCount = keys.Length;
                int delta = toCount - payload.GetKeys().Count();
                switch (delta)
                {
                    case 0:
                        return;
                    case > 0:
                    {
                        dictField.PairPanel.style.display = DisplayStyle.Flex;
                        dictField.FooterButtons.AddButton.SetEnabled(false);

                        dictField.ArraySizeField.SetValueWithoutNotify(curCount);
                        dictField.Pager.NumberOfItemsTotalField.SetValueWithoutNotify(curCount);
                    }
                        break;
                    case < 0:
                    {
                        foreach (object toDeleteKey in keys.Reverse().Take(-delta))
                        {
                            payload.DeleteKey(toDeleteKey);
                        }
                    }
                        break;
                }
            }

            dictField.ArraySizeField.RegisterValueChangedCallback(ChangeSize);
            dictField.Pager.NumberOfItemsTotalField.RegisterValueChangedCallback(ChangeSize);

            #region Key/Value
            ResponsiveLength keyWidth = saintsDictionaryAttribute.KeyWidth;
            dictField.ListView.columns.Add(new Column
            {
                name = "Keys",
                // title = "Keys",
                stretchable = keyWidth.Type == ResponsiveType.None,
                width = MakeLength(keyWidth),
                makeHeader = () =>
                {
                    VisualElement header = new VisualElement();
                    header.Add(new Label(string.IsNullOrEmpty(saintsDictionaryAttribute.KeyLabel)? "Keys": saintsDictionaryAttribute.KeyLabel)
                    {
                        style =
                        {
                            marginLeft = 4,
                        },
                    });
                    SearchContainerStruct searchContainerStruct = SearchContainerStruct.Load();
                    payload.KeySearchRoot = searchContainerStruct.Root;
                    payload.KeySearchField = searchContainerStruct.ToolbarSearchField;
                    if (!saintsDictionaryAttribute.Searchable)
                    {
                        searchContainerStruct.Root.style.display = DisplayStyle.None;
                    }

                    header.Add(searchContainerStruct.Root);

#if UNITY_6000_0_OR_NEWER
                    searchContainerStruct.ToolbarSearchField.placeholderText = "";
#endif
                    payload.KeyLoadingImage = searchContainerStruct.LoadingImage;

                    searchContainerStruct.ToolbarSearchField.RegisterValueChangedCallback(evt =>
                    {
                        payload.AsyncSearchItems.KeySearchText = evt.newValue;
                        payload.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
                        payload.AsyncSearchItems.Started = false;
                        payload.AsyncSearchItems.Finished = false;
                        payload.AsyncSearchItems.HitTargetIndexes.Clear();
                        payload.AsyncSearchItems.SourceGenerator = SearchPayload(payload);
                        payload.AsyncSearchItems.LoadingImages.Add(searchContainerStruct.LoadingImage);
                        RefreshFieldWithPayload(dictField, payload);
                    });
                    return header;
                },
                makeCell = () => new VisualElement
                {
                    style =
                    {
                        marginRight = 2,
                    },
                },
                bindCell = (element, elementIndex) =>
                {
                    object key = dictField.ListView.itemsSource[elementIndex];
                    // Debug.Log($"accessing key {key}@{elementIndex}");
                    object oldValue = payload.GetValue(key);
                    bool keyChanged = true;

                    VisualElement keyChild = element.Children().FirstOrDefault();

                    element.schedule.Execute(() =>
                    {
                        if (!keyChanged)
                        {
                            return;
                        }

                        keyChanged = false;

                        VisualElement editing = UIToolkitEdit.UIToolkitValueEdit(
                                keyChild,
                                "",
                                dictKeyType,
                                key,
                                oldKey =>
                                {
        #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                                    Debug.Log($"oldKey={oldKey}");
        #endif
                                    oldValue = payload.GetValue(oldKey);
                                    payload.DeleteKey(oldKey);
                                },
                                newKey =>
                                {
                            if (RuntimeUtil.IsNull(newKey))
                            {
                                Debug.LogWarning($"Setting key to null is not supported and is ignored");
                                return;
                            }

                            if (payload.ContainsKey(newKey))
                            {
                                Debug.LogWarning($"Setting key {key} to existing key {newKey} is not supported and is ignored");
                                return;
                            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                            Debug.Log($"dictionary editing key {key} -> {newKey}");
#endif
                            // object oldValue = payload.GetValue(key);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                            Debug.Log($"set key {key} -> {newKey} with value {oldValue}");
#endif
                            payload.DeleteKey(key);
                            payload.SetKeyValue(newKey, oldValue);
                            // int sourceIndex = listView.itemsSource.IndexOf(key);
                            // listView.itemsSource[sourceIndex] = newKey;
                            key = newKey;
                            keyChanged = true;
                        },
                                false,
                                true,
                                Array.Empty<Attribute>(),
                                targets,
                                richTextTagProvider,
                                $"{foldoutViewKey}.[key].[{elementIndex}]"
                            ).result;

                        if (editing != null)
                        {
                            element.Clear();
                            element.Add(editing);
                        }
                    }).Every(100);
                },
            });

            ResponsiveLength valueWidth = saintsDictionaryAttribute.ValueWidth;
            dictField.ListView.columns.Add(new Column
            {
                name = "Values",
                // title = "Keys",
                stretchable = valueWidth.Type == ResponsiveType.None,
                width = MakeLength(valueWidth),
                makeHeader = () =>
                {
                    VisualElement header = new VisualElement();
                    header.Add(new Label(string.IsNullOrEmpty(saintsDictionaryAttribute.ValueLabel)? "Values": saintsDictionaryAttribute.ValueLabel)
                    {
                        style =
                        {
                            marginLeft = 4,
                        },
                    });
                    SearchContainerStruct searchContainerStruct = SearchContainerStruct.Load();
                    payload.ValueSearchRoot = searchContainerStruct.Root;
                    payload.ValueSearchField = searchContainerStruct.ToolbarSearchField;
                    header.Add(searchContainerStruct.Root);
#if UNITY_6000_0_OR_NEWER
                    searchContainerStruct.ToolbarSearchField.placeholderText = "";
#endif
                    if (!saintsDictionaryAttribute.Searchable)
                    {
                        searchContainerStruct.Root.style.display = DisplayStyle.None;
                    }

                    payload.ValueLoadingImage = searchContainerStruct.LoadingImage;

                    searchContainerStruct.ToolbarSearchField.RegisterValueChangedCallback(evt =>
                    {
                        // Debug.Log($"value search {evt.newValue}");
                        payload.AsyncSearchItems.ValueSearchText = evt.newValue;
                        payload.AsyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
                        payload.AsyncSearchItems.Started = false;
                        payload.AsyncSearchItems.Finished = false;
                        payload.AsyncSearchItems.HitTargetIndexes.Clear();
                        payload.AsyncSearchItems.SourceGenerator = SearchPayload(payload);
                        payload.AsyncSearchItems.LoadingImages.Add(searchContainerStruct.LoadingImage);
                        RefreshFieldWithPayload(dictField, payload);
                    });

                    return header;
                },
                makeCell = () => new VisualElement()
                {
                    style =
                    {
                        marginRight = 4,
                    },
                },
                bindCell = (element, elementIndex) =>
                {
                    object key = dictField.ListView.itemsSource[elementIndex];
                    object value = payload.GetValue(key);

                    VisualElement valueChild = element.Children().FirstOrDefault();

                    VisualElement editing = UIToolkitEdit.UIToolkitValueEdit(
                        valueChild,
                        "",
                        dictValueType,
                        value,
                        null,
                        newValue =>
                        {
                            object refreshedKey = dictField.ListView.itemsSource[elementIndex];
                            payload.SetKeyValue(refreshedKey, newValue);
                        },
                        false,
                        true,
                        Array.Empty<Attribute>(),
                        targets,
                        richTextTagProvider,
                        $"{foldoutViewKey}.[value].[{elementIndex}]"
                    ).result;

                    if (editing != null)
                    {
                        element.Clear();
                        element.Add(editing);
                    }
                },
            });
            #endregion

            dictField.PairPanel.OnFinished.AddListener((added, key, value) =>
            {
                if (!added)
                {
                    return;
                }

                payload.SetKeyValue(key, value);
                RefreshFieldWithPayload(dictField, payload);
            });

            if (saintsDictionaryAttribute.NumberOfItemsPerPage <= 0)
            {
                dictField.Pager.style.display = DisplayStyle.None;
            }

            dictField.FooterButtons.RemoveButton.clicked += () =>
            {
                int[] toRemoveIndices = dictField.ListView.selectedIndices.ToArray();
                List<object> removeKeys = new List<object>();
                if (toRemoveIndices.Length == 0)
                {
                    removeKeys.Add(dictField.ListView.itemsSource.Count - 1);
                }
                else
                {
                    int index = 0;
                    foreach (object key in dictField.ListView.itemsSource)
                    {
                        if (Array.IndexOf(toRemoveIndices, index) != -1)
                        {
                            removeKeys.Add(key);
                        }

                        index++;
                    }
                }

                foreach (object key in removeKeys)
                {
                    payload.DeleteKey(key);
                    // listView.itemsSource.Remove(key);
                }
            };

            #region Logic

            dictField.Pager.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt =>
            {
                payload.AsyncSearchItems.NumberOfItemsPerPage = evt.newValue;
                RefreshFieldWithPayload(dictField, payload);
            });
            dictField.Pager.NumberOfItemsPerPageField.SetValueWithoutNotify(saintsDictionaryAttribute.NumberOfItemsPerPage);
            dictField.Pager.PagePreButton.clicked += () =>
            {
                payload.AsyncSearchItems.PageIndex = Mathf.Max(0, payload.AsyncSearchItems.PageIndex - 1);
                RefreshFieldWithPayload(dictField, payload);
            };
            dictField.Pager.PageField.RegisterValueChangedCallback(evt =>
            {
                payload.AsyncSearchItems.PageIndex = Mathf.Clamp(evt.newValue - 1, 0, payload.AsyncSearchItems.TotalPage - 1);
                RefreshFieldWithPayload(dictField, payload);
            });
            dictField.Pager.PageNextButton.clicked += () =>
            {
                payload.AsyncSearchItems.PageIndex = Mathf.Min(payload.AsyncSearchItems.PageIndex + 1, payload.AsyncSearchItems.TotalPage - 1);
                RefreshFieldWithPayload(dictField, payload);
            };

            #endregion

            #region Menu

            dictField.Foldout.MenuButton.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                if (setterOrNull == null)
                {
                    genericDropdownMenu.AddDisabledItem("Set To Null", false);
                }
                else
                {
                    genericDropdownMenu.AddItem("Set To Null", false, () =>
                    {
                        beforeSet?.Invoke(payload.RawDictValue);
                        setterOrNull(null);
                    });
                }

                bool curPaging = dictField.Pager.style.display != DisplayStyle.None;
                genericDropdownMenu.AddItem("Paging", curPaging, () =>
                {
                    if (curPaging)
                    {
                        dictField.Pager.style.display = DisplayStyle.None;
                        dictField.Pager.NumberOfItemsPerPageField.value = -1;
                    }
                    else
                    {
                        int configuredItemsPerPage = saintsDictionaryAttribute.NumberOfItemsPerPage;
                        int itemsPerPage = configuredItemsPerPage > 0
                            ? configuredItemsPerPage
                            : Mathf.Max(5, payload.GetKeys().Count() / 2);
                        dictField.Pager.style.display = DisplayStyle.Flex;
                        dictField.Pager.NumberOfItemsPerPageField.value = itemsPerPage;
                    }
                });

                if (payload.KeySearchField != null && payload.ValueSearchField != null)
                {
                    bool curSearch = payload.KeySearchRoot.style.display != DisplayStyle.None;
                    genericDropdownMenu.AddItem("Search", curSearch, () =>
                    {
                        DisplayStyle toDisplay = curSearch ? DisplayStyle.None : DisplayStyle.Flex;
                        payload.KeySearchRoot.style.display = payload.ValueSearchRoot.style.display = toDisplay;
                        if (curSearch)
                        {
                            payload.KeySearchField.value = "";
                            payload.ValueSearchField.value = "";
                        }
                    });

                    if (curSearch)
                    {
                        genericDropdownMenu.AddItem("Object Search", payload.ObjectNestedSearch, () =>
                        {
                            payload.ObjectNestedSearch = !payload.ObjectNestedSearch;
                            if (!string.IsNullOrEmpty(payload.AsyncSearchItems.KeySearchText) ||
                                !string.IsNullOrEmpty(payload.AsyncSearchItems.ValueSearchText))
                            {
                                payload.AsyncSearchItems.DebounceSearchTime = 0;
                                payload.AsyncSearchItems.Started = false;
                                payload.AsyncSearchItems.Finished = false;
                                payload.AsyncSearchItems.HitTargetIndexes.Clear();
                                payload.AsyncSearchItems.SourceGenerator?.Dispose();
                                payload.AsyncSearchItems.SourceGenerator = SearchPayload(payload);
                                RefreshFieldWithPayload(dictField, payload);
                            }
                        });
                    }
                    else
                    {
                        genericDropdownMenu.AddDisabledItem("Object Search", payload.ObjectNestedSearch);
                    }
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem("Search", false);
                }

                Rect menuBound = dictField.Foldout.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
                menuBound.xMin = menuBound.xMax - Mathf.Max(menuBound.width, 120f);
#endif
                genericDropdownMenu.DropDown(menuBound, dictField.Foldout.MenuButton,
#if UNITY_6000_3_OR_NEWER
                    DropdownMenuSizeMode.Auto
#else
                    true
#endif
                );
            };

            #endregion

            RefreshFieldWithPayload(dictField, payload);

            UIToolkitUtils.OnAttachToPanelOnce(dictField.ListView, _ => dictField.ListView.schedule.Execute(() =>
            {
                if (payload.AsyncSearchItems.Finished)
                {
                    if(payload.KeyLoadingImage != null && payload.KeyLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        payload.KeyLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(payload.ValueLoadingImage != null && payload.ValueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        payload.ValueLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    return;
                }

                if (payload.AsyncSearchItems.SourceGenerator == null)
                {
                    if(payload.KeyLoadingImage != null && payload.KeyLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        payload.KeyLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(payload.ValueLoadingImage != null && payload.ValueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        payload.ValueLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    return;
                }

                bool emptySearch = string.IsNullOrEmpty(payload.AsyncSearchItems.KeySearchText) &&
                                   string.IsNullOrEmpty(payload.AsyncSearchItems.ValueSearchText);

                if (!emptySearch && payload.AsyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                {
                    if(payload.KeyLoadingImage != null && payload.KeyLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        payload.KeyLoadingImage.style.visibility = Visibility.Hidden;
                    }
                    if(payload.ValueLoadingImage != null && payload.ValueLoadingImage.style.visibility != Visibility.Hidden)
                    {
                        payload.ValueLoadingImage.style.visibility = Visibility.Hidden;
                    }

                    // Debug.Log("Search wait");
                    return;
                }

                if(!payload.AsyncSearchItems.Started)
                {
                    // Debug.Log($"Search start {_asyncSearchItems.DebounceSearchTime} -> {EditorApplication.timeSinceStartup}");
                    payload.AsyncSearchItems.Started = true;
                    RefreshFieldWithPayload(dictField, payload);
                }

                if (payload.AsyncSearchItems.LoadingImages.Count == 0)
                {
                    if(payload.KeyLoadingImage != null)
                    {
                        payload.AsyncSearchItems.LoadingImages.Add(payload.KeyLoadingImage);
                    }
                    if(payload.ValueLoadingImage != null)
                    {
                        payload.AsyncSearchItems.LoadingImages.Add(payload.ValueLoadingImage);
                    }
                }

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (VisualElement loadingImage in payload.AsyncSearchItems.LoadingImages)
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
                    if (payload.AsyncSearchItems.SourceGenerator.MoveNext())
                    {
                        // ReSharper disable once InvertIf
                        if(payload.AsyncSearchItems.SourceGenerator.Current != null)
                        {
                            needRefreshDisplay = true;
                            // Debug.Log($"search found {_asyncSearchItems.SourceGenerator.Current}");
                            payload.AsyncSearchItems.HitTargetIndexes.Add(payload.AsyncSearchItems.SourceGenerator.Current);
                        }
                    }
                    else
                    {
                        payload.AsyncSearchItems.Finished = true;
                        payload.AsyncSearchItems.CachedHitTargetIndexes = new List<object>(payload.AsyncSearchItems.HitTargetIndexes);
                        payload.AsyncSearchItems.SourceGenerator.Dispose();
                        payload.AsyncSearchItems.SourceGenerator = null;

                        // Debug.Log($"search finished {_asyncSearchItems.HitTargetIndexes.Count}");

                        if(payload.KeyLoadingImage != null && payload.KeyLoadingImage.style.visibility != Visibility.Hidden)
                        {
                            payload.KeyLoadingImage.style.visibility = Visibility.Hidden;
                        }
                        if(payload.ValueLoadingImage != null && payload.ValueLoadingImage.style.visibility != Visibility.Hidden)
                        {
                            payload.ValueLoadingImage.style.visibility = Visibility.Hidden;
                        }
                        payload.AsyncSearchItems.LoadingImages.Clear();
                        needRefreshDisplay = true;
                        break;
                    }
                }

                if(needRefreshDisplay)
                {
                    RefreshFieldWithPayload(dictField, payload);
                }
                // _asyncSearchBusy = false;
            }).Every(1));

            return dictField;
        }

        private static void RefreshFieldWithPayload(SaintsDictionaryWrapper dictField, DictionaryViewPayload payload)
        {
            int curPageIndex = payload.AsyncSearchItems.PageIndex;
            int numberOfItemsPerPage = payload.AsyncSearchItems.NumberOfItemsPerPage;
            // bool needRebuild = false;
            int nowArraySize = payload.GetKeys().Count();

            // List<int> fullList = Enumerable.Range(0, nowArraySize).ToList();
            // List<int> useIndexes = new List<int>(itemIndexToPropertyIndex);
            // ReSharper disable once AccessToModifiedClosure
            List<object> refreshedHitTargetIndexes = new List<object>(payload.AsyncSearchItems.Started? payload.AsyncSearchItems.HitTargetIndexes: payload.AsyncSearchItems.CachedHitTargetIndexes);
            if (nowArraySize != payload.AsyncSearchItems.Size)
            {
                payload.AsyncSearchItems.Size = nowArraySize;
                payload.AsyncSearchItems.DebounceSearchTime = 0;
                payload.AsyncSearchItems.Started = false;
                payload.AsyncSearchItems.Finished = false;
                payload.AsyncSearchItems.HitTargetIndexes.Clear();
                payload.AsyncSearchItems.SourceGenerator?.Dispose();
                // TODO
                payload.AsyncSearchItems.SourceGenerator = SearchPayload(payload);

                // Debug.Log("size changed, tail call refresh list");
                // ReSharper disable once TailRecursiveCall
                RefreshFieldWithPayload(dictField, payload);
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
                payload.itemIndexToKeys = refreshedHitTargetIndexes.GetRange(startIndex, endIndex - startIndex);
                int totalPage = Mathf.Max(1, Mathf.CeilToInt(refreshedHitTargetIndexes.Count / (float)numberOfItemsPerPage));

                // pageField.SetValueWithoutNotify(curPageIndex + 1);


                // needRebuild = preNumberOfItemsPerPage != numberOfItemsPerPage
                //               || preTotalPage != totalPage
                //               || prePageIndex != curPageIndex;

                // preNumberOfItemsPerPage = numberOfItemsPerPage;
                payload.AsyncSearchItems.TotalPage = totalPage;
                payload.AsyncSearchItems.PageIndex = curPageIndex;
            }
            else
            {
                payload.itemIndexToKeys = refreshedHitTargetIndexes;
            }

            // Debug.Log(multiColumnListView.itemsSource);
            // Debug.Log(itemIndexToPropertyIndex);

            bool needRebuild = dictField.ListView.itemsSource == null
                               || !dictField.ListView.itemsSource.Cast<object>().SequenceEqual(payload.itemIndexToKeys);
            // if (multiColumnListView.itemsSource != null)
            // {
            //     Debug.Log(string.Join(", ", multiColumnListView.itemsSource.Cast<int>()));
            //     Debug.Log(string.Join(", ", itemIndexToPropertyIndex));
            // }

            if (needRebuild)
            {
                // Debug.Log("rebuild list view");
                dictField.ListView.itemsSource = payload.itemIndexToKeys.ToList();
                dictField.ListView.Rebuild();
                dictField.Pager.PagePreButton.SetEnabled(payload.AsyncSearchItems.PageIndex > 0);
                dictField.Pager.PageField.SetValueWithoutNotify(payload.AsyncSearchItems.PageIndex + 1);
                dictField.Pager.PageLabel.text = $"/ {payload.AsyncSearchItems.TotalPage}";
                dictField.Pager.PageNextButton.SetEnabled(payload.AsyncSearchItems.PageIndex + 1 < payload.AsyncSearchItems.TotalPage);
            }
        }

        private static IEnumerator<object> SearchPayload(DictionaryViewPayload payload)
        {
            string keySearch = payload.AsyncSearchItems.KeySearchText;
            string valueSearch = payload.AsyncSearchItems.ValueSearchText;
            bool keySearchEmpty = string.IsNullOrEmpty(keySearch);
            bool valueSearchEmpty = string.IsNullOrEmpty(valueSearch);
            object[] keys = payload.GetKeys().ToArray();

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (keySearchEmpty && valueSearchEmpty)
            {
                foreach (object key in keys)
                {
                    yield return key;
                }
                yield break;
            }

            IReadOnlyList<ListSearchToken> valueSearchTokens = SerializedUtils.ParseSearch(valueSearch).ToArray();

            if (keySearchEmpty)
            {
                foreach (object key in keys)
                {
                    object value = payload.GetValue(key);
                    if (Util.SearchObjectWithTokens(value, valueSearchTokens, payload.ObjectNestedSearch))
                    {
                        yield return key;
                    }
                    else
                    {
                        // Debug.Log($"value failed {value} -> {valueSearch}");
                        yield return null;
                    }
                }
                yield break;
            }


            foreach (int index in Util.SearchArrayObjects(keys, keySearch, payload.ObjectNestedSearch))
            {
                if (index == -1)
                {
                    yield return null;
                }
                else
                {
                    object key = keys[index];
                    object valueProp = payload.GetValue(key);
                    if (Util.SearchObjectWithTokens(valueProp, valueSearchTokens, payload.ObjectNestedSearch))
                    {
                        yield return key;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}
#endif
