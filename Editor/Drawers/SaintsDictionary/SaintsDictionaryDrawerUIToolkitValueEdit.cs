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
            public UnityEvent<bool, object, object> OnFinished = new UnityEvent<bool, object, object>();

            public PairPanel(Type dictKeyType, Type dictValueType, DictionaryViewPayload payload, bool inHorizontalLayout, IReadOnlyList<object> targets)
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

                    VisualElement r = AbsRenderer.UIToolkitValueEdit(
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
                        targets
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

                    VisualElement r = AbsRenderer.UIToolkitValueEdit(
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
                        targets
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
            public readonly Foldout Foldout;
            public readonly IntegerField ArraySizeField;
            public readonly Button NullButton;
            public readonly MultiColumnListView ListView;
            public readonly ListViewPagerFooterStruct FooterStruct;
            public readonly PairPanel PairPanel;

            public SaintsDictionaryWrapper(string label, bool nullable, MultiColumnListView listView, Type dictKeyType, Type dictValueType, DictionaryViewPayload payload, bool inHorizontalLayout, IReadOnlyList<object> targets)
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

                // body
                Add(ListView = listView);

                // footer
                FooterStruct = new ListViewPagerFooterStruct(true);
                Add(FooterStruct.Root);

                // panel for adding
                Add(PairPanel = new PairPanel(dictKeyType, dictValueType, payload, inHorizontalLayout, targets));

                FooterStruct.AddButton.clicked += () =>
                {
                    PairPanel.style.display = DisplayStyle.Flex;
                    FooterStruct.AddButton.SetEnabled(false);
                };
                PairPanel.OnFinished.AddListener((_, _, _) =>
                {
                    FooterStruct.AddButton.SetEnabled(true);
                });

                RefreshToggleDisplay();
                Foldout.RegisterValueChangedCallback(_ => RefreshToggleDisplay());
            }

            private void RefreshToggleDisplay()
            {
                DisplayStyle display = Foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
                ListView.style.display = display;
                FooterStruct.Root.style.display = display;
            }

        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object rawDictValue,
            bool isReadOnly, Type dictKeyType, Type dictValueType, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {

            if (oldElement is SaintsDictionaryWrapper dictField)
            {
                DictionaryViewPayload oldPayload = (DictionaryViewPayload)dictField.ListView.userData;
                oldPayload.RawDictValue = rawDictValue;
                int totalCount = oldPayload.GetKeys().Count();
                dictField.ArraySizeField.SetValueWithoutNotify(totalCount);
                dictField.FooterStruct.NumberOfItemsTotalField.SetValueWithoutNotify(totalCount);

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
            };

            dictField = new SaintsDictionaryWrapper(label, setterOrNull != null, new MultiColumnListView
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
            }, dictKeyType, dictValueType, payload, inHorizontalLayout, targets);

            // Size
            if (isReadOnly)
            {
                dictField.ArraySizeField.SetEnabled(false);
                dictField.FooterStruct.NumberOfItemsTotalField.SetEnabled(false);
                dictField.FooterStruct.FooterButtons.SetEnabled(false);
            }
            dictField.ArraySizeField.SetValueWithoutNotify(initCount);
            dictField.FooterStruct.NumberOfItemsTotalField.SetValueWithoutNotify(initCount);

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
                        dictField.FooterStruct.AddButton.SetEnabled(false);

                        dictField.ArraySizeField.SetValueWithoutNotify(curCount);
                        dictField.FooterStruct.NumberOfItemsTotalField.SetValueWithoutNotify(curCount);
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
            dictField.FooterStruct.NumberOfItemsTotalField.RegisterValueChangedCallback(ChangeSize);

            #region Key/Value
            dictField.ListView.columns.Add(new Column
            {
                name = "Keys",
                // title = "Keys",
                stretchable = true,
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

                        VisualElement editing = AbsRenderer.UIToolkitValueEdit(keyChild, "", dictKeyType, key, oldKey =>
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                            Debug.Log($"oldKey={oldKey}");
#endif
                            oldValue = payload.GetValue(oldKey);
                            payload.DeleteKey(oldKey);
                        }, newKey =>
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
                        }, false, true, Array.Empty<Attribute>(), targets).result;

                        if (editing != null)
                        {
                            element.Clear();
                            element.Add(editing);
                        }
                    }).Every(100);
                },
            });

            dictField.ListView.columns.Add(new Column
            {
                name = "Values",
                // title = "Keys",
                stretchable = true,
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

                    VisualElement editing = AbsRenderer.UIToolkitValueEdit(valueChild, "", dictValueType, value, null, newValue =>
                    {
                        object refreshedKey = dictField.ListView.itemsSource[elementIndex];
                        payload.SetKeyValue(refreshedKey, newValue);
                    }, false, true, Array.Empty<Attribute>(), targets).result;

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
                dictField.ListView.itemsSource = payload.GetKeys().ToList();
            });

            if (saintsDictionaryAttribute.NumberOfItemsPerPage <= 0)
            {
                dictField.FooterStruct.PagingContainer.style.display = DisplayStyle.None;
            }

            dictField.FooterStruct.RemoveButton.clicked += () =>
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

            dictField.FooterStruct.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt =>
            {
                payload.AsyncSearchItems.NumberOfItemsPerPage = evt.newValue;
                RefreshFieldWithPayload(dictField, payload);
            });
            dictField.FooterStruct.NumberOfItemsPerPageField.SetValueWithoutNotify(saintsDictionaryAttribute.NumberOfItemsPerPage);
            dictField.FooterStruct.PagePreButton.clicked += () =>
            {
                payload.AsyncSearchItems.PageIndex = Mathf.Max(0, payload.AsyncSearchItems.PageIndex - 1);
                RefreshFieldWithPayload(dictField, payload);
            };
            dictField.FooterStruct.PageField.RegisterValueChangedCallback(evt =>
            {
                payload.AsyncSearchItems.PageIndex = Mathf.Clamp(evt.newValue - 1, 0, payload.AsyncSearchItems.TotalPage - 1);
                RefreshFieldWithPayload(dictField, payload);
            });
            dictField.FooterStruct.PageNextButton.clicked += () =>
            {
                payload.AsyncSearchItems.PageIndex = Mathf.Min(payload.AsyncSearchItems.PageIndex + 1, payload.AsyncSearchItems.TotalPage - 1);
                RefreshFieldWithPayload(dictField, payload);
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
                dictField.FooterStruct.PagePreButton.SetEnabled(payload.AsyncSearchItems.PageIndex > 0);
                dictField.FooterStruct.PageField.SetValueWithoutNotify(payload.AsyncSearchItems.PageIndex + 1);
                dictField.FooterStruct.PageLabel.text = $"/ {payload.AsyncSearchItems.TotalPage}";
                dictField.FooterStruct.PageNextButton.SetEnabled(payload.AsyncSearchItems.PageIndex + 1 < payload.AsyncSearchItems.TotalPage);
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
                    if (Util.SearchObjectWithTokens(value, valueSearchTokens))
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


            foreach (int index in Util.SearchArrayObjects(keys, keySearch))
            {
                if (index == -1)
                {
                    yield return null;
                }
                else
                {
                    object key = keys[index];
                    object valueProp = payload.GetValue(key);
                    if (Util.SearchObjectWithTokens(valueProp, valueSearchTokens))
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
