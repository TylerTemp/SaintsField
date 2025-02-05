#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        private PropertyField _result;

        private class UserDataPayload
        {
            public string XML;
            public Label Label;
            public string FriendlyName;
            public RichTextDrawer RichTextDrawer;

            public bool TableHasSize;
        }

        private VisualElement _fieldElement;
        private bool _arraySizeCondition;
        private bool _richLabelCondition;
        private bool _tableCondition;

        private static string NameTableContainer(SerializedProperty property)
        {
            return $"saints-table-container-{property.propertyPath}";
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            UserDataPayload userDataPayload = new UserDataPayload
            {
                FriendlyName = FieldWithInfo.SerializedProperty.displayName,
            };

            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
            ArraySizeAttribute arraySizeAttribute =
                FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();

            VisualElement result;
            if (listDrawerSettingsAttribute != null)
            {
                result = MakeListDrawerSettingsField(listDrawerSettingsAttribute, arraySizeAttribute?.Min ?? -1,
                    arraySizeAttribute?.Max ?? -1);
            }
            else if(tableAttribute != null)
            {
                bool hasSize = FieldWithInfo.SerializedProperty.arraySize > 0;
                userDataPayload.TableHasSize = hasSize;
                result = new VisualElement
                {
                    name = NameTableContainer(FieldWithInfo.SerializedProperty),
                };
                FillTable(FieldWithInfo.SerializedProperty, result);

                _tableCondition = true;
            }
            else
            {
                result = new PropertyField(FieldWithInfo.SerializedProperty)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                    name = FieldWithInfo.SerializedProperty.propertyPath,
                };
            }

            result.userData = userDataPayload;

            OnArraySizeChangedAttribute onArraySizeChangedAttribute = FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            if (onArraySizeChangedAttribute != null)
            {
                OnArraySizeChangedUIToolkit(onArraySizeChangedAttribute.Callback, result, FieldWithInfo.SerializedProperty, (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
            }

            // disable/enable/show/hide
            bool ifCondition = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute
                                                                           // ReSharper disable once MergeIntoLogicalPattern
                                                                           || each is PlayaEnableIfAttribute
                                                                           // ReSharper disable once MergeIntoLogicalPattern
                                                                           || each is PlayaDisableIfAttribute) > 0;
            _arraySizeCondition = FieldWithInfo.PlayaAttributes.Any(each => each is IPlayaArraySizeAttribute);
            _richLabelCondition = FieldWithInfo.PlayaAttributes.Any(each => each is PlayaRichLabelAttribute);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log(
                $"SerField: {FieldWithInfo.SerializedProperty.displayName}({FieldWithInfo.SerializedProperty.propertyPath}); if={ifCondition}; arraySize={_arraySizeCondition}, richLabel={_richLabelCondition}");
#endif

            bool needUpdate = ifCondition || _arraySizeCondition || _richLabelCondition || _tableCondition;

            return (_fieldElement = result, needUpdate);
        }

        private static void FillTable(SerializedProperty arrayProperty, VisualElement result)
        {
            bool hasSize = arrayProperty.arraySize > 0;
            SerializedProperty targetProperty = hasSize
                ? arrayProperty.GetArrayElementAtIndex(0)
                : arrayProperty;

            PropertyField propField = new PropertyField(targetProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            propField.Bind(arrayProperty.serializedObject);

            if (hasSize)
            {
                Foldout foldout = new Foldout
                {
                    value = true,
                    text = arrayProperty.displayName,
                    style = { flexGrow = 1 },
                };
                foldout.Add(propField);
                result.Add(foldout);
            }
            else
            {
                result.Add(propField);
            }
        }

        private static void OnArraySizeChangedUIToolkit(string callback, VisualElement result, SerializedProperty property, MemberInfo memberInfo)
        {
            if (!property.isArray)
            {
                Debug.LogWarning($"{property.propertyPath} is no an array/list");
                return;
            }

            int arraySize = property.arraySize;
            // don't use TrackPropertyValue because if you remove anything from list, it gives error
            // this is Unity's fault
            // result.TrackPropertyValue(property, p =>
            result.TrackSerializedObjectValue(property.serializedObject, _ =>
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

                if (newSize == arraySize)
                {
                    return;
                }

                arraySize = newSize;
                InvokeArraySizeCallback(callback, property, memberInfo);
            });
        }

        private VisualElement MakeListDrawerSettingsField(ListDrawerSettingsAttribute listDrawerSettingsAttribute, int minSize, int maxSize)
        {
            SerializedProperty property = FieldWithInfo.SerializedProperty;

            // int numberOfItemsPerPage = 0;
            int curPageIndex = 0;
            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, property.arraySize).ToList();

            VisualElement MakeItem()
            {
                PropertyField propertyField = new PropertyField();
                return propertyField;
            }

            void BindItem(VisualElement propertyFieldRaw, int index)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log(($"bind: {index}, propIndex: {itemIndexToPropertyIndex[index]}, itemIndexes={string.Join(", ", itemIndexToPropertyIndex)}"));
#endif
                SerializedProperty prop = property.GetArrayElementAtIndex(itemIndexToPropertyIndex[index]);
                PropertyField propertyField = (PropertyField)propertyFieldRaw;
                propertyField.BindProperty(prop);
            }

            ListView listView = new ListView(Enumerable.Range(0, property.arraySize).ToList())
            {
                makeItem = MakeItem,
                bindItem = BindItem,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                showFoldoutHeader = true,
                headerTitle = property.displayName,
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                style =
                {
                    flexGrow = 1,
                },
            };

            VisualElement foldoutContent = listView.Q<VisualElement>(className: "unity-foldout__content");

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
            if(listDrawerSettingsAttribute.Delayed)
            {
                TextField text = searchField.Q<TextField>();
                if (text != null)
                {
                    text.isDelayed = true;
                }
            }
            // searchContainer.Add(searchField);
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
                PagingInfo pagingInfo = GetPagingInfo(property, newPageIndex, searchField.value, numberOfItemsPerPage);

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
                listView.itemsSource = curPageItems;
                // Debug.Log("rebuild listView");
                listView.Rebuild();
                UpdateAddRemoveButtons();
            }

            int arraySize = property.arraySize;
            // don't use TrackPropertyValue because if you remove anything from list, it gives error
            // this is Unity's fault
            // result.TrackPropertyValue(property, p =>
            listView.TrackSerializedObjectValue(property.serializedObject, _ =>
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

                if (newSize == arraySize)
                {
                    return;
                }

                arraySize = newSize;
                numberOfItemsTotalField.SetValueWithoutNotify(arraySize);
                UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
            });

            void UpdateAddRemoveButtons()
            {
                int curSize = property.arraySize;
                bool canNotAddMore = maxSize >= 0 && curSize >= maxSize;
                listViewAddButton.SetEnabled(!canNotAddMore);
                bool canNotRemoveMore = minSize >= 0 && curSize <= minSize;
                listViewRemoveButton.SetEnabled(!canNotRemoveMore);
            }

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
                property.arraySize = e.newValue;
                property.serializedObject.ApplyModifiedProperties();
                UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
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
            pagingContainer.Add(numberOfItemsTotalField);
            pagingContainer.Add(numberOfItemsDesc);

            pagingContainer.Add(pagePreButton);
            pagingContainer.Add(pageField);
            pagingContainer.Add(pageLabel);
            pagingContainer.Add(pageNextButton);

            preContent.Add(pagingContainer);

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

            foldoutContent.Insert(0, preContent);

            UpdateAddRemoveButtons();

            return listView;
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        // private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition, bool richLabelCondition, FieldInfo info, object parent)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit(root);

            if(_arraySizeCondition)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log(
                    $"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; preCheckResult.ArraySize={preCheckResult.ArraySize}, curSize={FieldWithInfo.SerializedProperty.arraySize}");
#endif
                if (preCheckResult.ArraySize != -1 &&
                    ((preCheckResult.ArraySize == 0 && FieldWithInfo.SerializedProperty.arraySize > 0)
                     || (preCheckResult.ArraySize >= 1 && FieldWithInfo.SerializedProperty.arraySize == 0)))
                {
                    FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
                    FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            if (_richLabelCondition)
            {
                string xml = preCheckResult.RichLabelXml;
                // Debug.Log(xml);
                UserDataPayload userDataPayload = (UserDataPayload) _fieldElement.userData;
                if (xml != userDataPayload.XML)
                {
                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if (userDataPayload.RichTextDrawer == null)
                    {
                        userDataPayload.RichTextDrawer = new RichTextDrawer();
                    }
                    if(userDataPayload.Label == null)
                    {
                        UIToolkitUtils.WaitUntilThenDo(
                            _fieldElement,
                            () =>
                            {
                                Label label = _fieldElement.Q<Label>(className: "unity-label");
                                if (label == null)
                                {
                                    return (false, null);
                                }
                                return (true, label);
                            },
                            label =>
                            {
                                userDataPayload.Label = label;
                            }
                        );
                    }
                    else
                    {
                        userDataPayload.XML = xml;
                        UIToolkitUtils.SetLabel(userDataPayload.Label, RichTextDrawer.ParseRichXml(xml, userDataPayload.FriendlyName, GetMemberInfo(FieldWithInfo), FieldWithInfo.Target), userDataPayload.RichTextDrawer);
                    }
                }
            }

            if (_tableCondition)
            {
                SerializedProperty prop = FieldWithInfo.SerializedProperty;
                VisualElement tableContainer = root.Q<VisualElement>(name: NameTableContainer(prop));
                UserDataPayload userDataPayload = (UserDataPayload)tableContainer.userData;
                bool hasSize = prop.arraySize > 0;
                if (userDataPayload.TableHasSize != hasSize)
                {
                    userDataPayload.TableHasSize = hasSize;
                    Debug.Log(tableContainer);
                    tableContainer.Clear();
                    FillTable(prop, tableContainer);
                }
            }

            return preCheckResult;
        }

        // private void OnGeometryChangedEvent(GeometryChangedEvent evt)
        // {
        //     // Debug.Log("OnGeometryChangedEvent");
        //     Label label = _result.Q<Label>(className: "unity-label");
        //     if (label == null)
        //     {
        //         return;
        //     }
        //
        //     // Utils.Util.FixLabelWidthLoopUIToolkit(label);
        //     _result.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        //     Utils.UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
        //     _result = null;
        // }
    }
}
#endif
