#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer
    {
        // private PropertyField _result;
        // private VisualElement _fieldElement;

        private Button _addButton;
        private Button _removeButton;

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            Debug.Assert(listDrawerSettingsAttribute != null, $"{FieldWithInfo.SerializedProperty.propertyPath}");
            // ArraySizeAttribute arraySizeAttribute =
            //     FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();

            (VisualElement root, Button addButton, Button removeButton) = MakeListDrawerSettingsField(listDrawerSettingsAttribute, FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault());
            _addButton = addButton;
            _removeButton = removeButton;

            return (root, false);
        }

        private (VisualElement root, Button addButton, Button removeButton) MakeListDrawerSettingsField(ListDrawerSettingsAttribute listDrawerSettingsAttribute, ArraySizeAttribute arraySizeAttribute)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
            };
            SerializedProperty property = FieldWithInfo.SerializedProperty;

            // int numberOfItemsPerPage = 0;
            int curPageIndex = 0;
            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, property.arraySize).ToList();

            VisualElement MakeItem()
            {
                // PropertyField propertyField = new PropertyField();
                return new VisualElement();
            }

            PropertyAttribute[] allAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo);

            void BindItem(VisualElement element, int index)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log(($"bind: {index}, propIndex: {itemIndexToPropertyIndex[index]}, itemIndexes={string.Join(", ", itemIndexToPropertyIndex)}"));
#endif
                if(index >= itemIndexToPropertyIndex.Count)
                {
                    return;
                }

                int propIndex = itemIndexToPropertyIndex[index];
                if(propIndex >= property.arraySize)
                {
                    return;
                }

                SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);
                VisualElement resultField = UIToolkitUtils.CreateOrUpdateFieldRawFallback(
                    prop,
                    allAttributes,
                    ReflectUtils.GetElementType(FieldWithInfo.FieldInfo.FieldType),
                    $"Element {index}",
                    FieldWithInfo.FieldInfo,
                    InAnyHorizontalLayout,
                    this,
                    this,
                    null,
                    FieldWithInfo.Target
                );
                // PropertyField propertyField = (PropertyField)propertyFieldRaw;
                // propertyField.BindProperty(prop);
                // Debug.Log(prop.propertyPath);
                element.Clear();
                // ReSharper disable once InvertIf
                if(resultField != null)
                {
                    element.Add(resultField);
                    // we can not clear the original context menu which will incorrectly copy the whole property, rather than an element
                    resultField.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        evt.menu.AppendAction("Copy Element Property Path",
                            _ => EditorGUIUtility.systemCopyBuffer = prop.propertyPath);

                        // bool spearator = false;
                        if (ClipboardHelper.CanCopySerializedProperty(prop.propertyType))
                        {
                            // spearator = true;
                            // evt.menu.AppendSeparator();
                            evt.menu.AppendAction("Copy Element", _ => ClipboardHelper.DoCopySerializedProperty(prop));
                        }

                        (bool hasReflectionPaste, bool hasValuePaste) =
                            ClipboardHelper.CanPasteSerializedProperty(prop.propertyType);

                        // ReSharper disable once InvertIf
                        if (hasReflectionPaste)
                        {
                            evt.menu.AppendAction("Paste Element", _ =>
                            {
                                ClipboardHelper.DoPasteSerializedProperty(prop);
                                property.serializedObject.ApplyModifiedProperties();
                            }, hasValuePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        }

                        evt.menu.AppendAction("Delete Element", _ =>
                        {
                            property.DeleteArrayElementAtIndex(propIndex);
                            property.serializedObject.ApplyModifiedProperties();
                        });

                        evt.menu.AppendSeparator();
                    }));
                }
            }

            ListView listView = new ListView(Enumerable.Range(0, property.arraySize).ToList())
            {
                makeItem = MakeItem,
                bindItem = BindItem,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                showBoundCollectionSize = false,
                showFoldoutHeader = true,
                headerTitle = property.displayName,
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                style =
                {
                    flexGrow = 1,
                    position = Position.Relative,
                },
            };

            Foldout foldoutElement = listView.Q<Foldout>();

            UIToolkitUtils.AddContextualMenuManipulator(foldoutElement, property, () => {});
            Toggle toggle = foldoutElement.Q<Toggle>();
            if (toggle != null && toggle.style.marginLeft != -12)
            {
                toggle.style.marginLeft = -12;
            }

            VisualElement foldoutContent = foldoutElement.Q<VisualElement>(className: "unity-foldout__content");

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
                // UpdateAddRemoveButtons();
            }

            int arraySize = property.arraySize;

            void CheckArraySizeChange()
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

                // Debug.Log($"size check {arraySize}/{newSize}");

                if (newSize == arraySize)
                {
                    return;
                }

                arraySize = newSize;
                numberOfItemsTotalField.SetValueWithoutNotify(arraySize);
                UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
            }

            // result.TrackPropertyValue(property, p =>
            // listView.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
            listView.TrackPropertyValue(property, _ =>
            {
                CheckArraySizeChange();
            });

            listView.schedule.Execute(CheckArraySizeChange).StartingIn(500);

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
                int min = -1;
                int max = -1;
                if (arraySizeAttribute != null)
                {
                    (string error, bool dynamic, int min, int max) getArraySize = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, property, FieldWithInfo.FieldInfo, FieldWithInfo.Target);
                    if(getArraySize.error == "")
                    {
                        min = getArraySize.min;
                        max = getArraySize.max;
                    }
                }

                int newSize = e.newValue;

                if(min > 0 && newSize < min)
                {
                    newSize = min;
                }
                else if(max > 0 && newSize > max)
                {
                    newSize = max;
                }

                if(property.arraySize != newSize)
                {
                    property.arraySize = newSize;
                    property.serializedObject.ApplyModifiedProperties();
                    UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
                }
                else
                {
                    numberOfItemsTotalField.SetValueWithoutNotify(property.arraySize);
                }
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
            if(listDrawerSettingsAttribute.NumberOfItemsPerPage > 0)
            {
                pagingContainer.Add(numberOfItemsTotalField);
            }
            else
            {
                numberOfItemsTotalField.style.position = Position.Absolute;
                numberOfItemsTotalField.style.right = 2;
                numberOfItemsTotalField.style.top = 1;
                numberOfItemsTotalField.style.minWidth = 50;
            }
            pagingContainer.Add(numberOfItemsDesc);

            pagingContainer.Add(pagePreButton);
            pagingContainer.Add(pageField);
            pagingContainer.Add(pageLabel);
            pagingContainer.Add(pageNextButton);

            preContent.Add(pagingContainer);

            #endregion

            #region Drag
            VisualElement foldoutInput = foldoutElement.Q<VisualElement>(classes: "unity-foldout__input");

            Type elementType =
                ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
                                            FieldWithInfo.PropertyInfo.PropertyType);
            foldoutInput.RegisterCallback<DragEnterEvent>(_ =>
            {
                // Debug.Log($"Drag Enter {evt}");
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            foldoutInput.RegisterCallback<DragLeaveEvent>(_ =>
            {
                // Debug.Log($"Drag Leave {evt}");
                DragAndDrop.visualMode = DragAndDropVisualMode.None;
            });
            foldoutInput.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                // Debug.Log($"Drag Update {evt}");
                // DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            });
            foldoutInput.RegisterCallback<DragPerformEvent>(_ =>
            {
                // Debug.Log($"Drag Perform {evt}");
                if (!DropUIToolkit(elementType, property))
                {
                    return;
                }

                property.serializedObject.ApplyModifiedProperties();
                UpdatePage(curPageIndex, numberOfItemsPerPageField.value);
            });
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

            listView.RegisterCallback<KeyDownEvent>(evt =>
            {
                // ReSharper disable once MergeIntoLogicalPattern
                bool ctrl = evt.modifiers == EventModifiers.Control ||
                            evt.modifiers == EventModifiers.Command;

                bool copyCommand = ctrl && evt.keyCode == KeyCode.C;
                if (copyCommand)
                {
                    int selectedIndex = listView.selectedItems
                        .Cast<int>()
                        .DefaultIfEmpty(-1)
                        .First();

                    if (selectedIndex == -1)
                    {
                        return;
                    }

                    int propIndex = itemIndexToPropertyIndex[selectedIndex];
                    if(propIndex >= property.arraySize)
                    {
                        return;
                    }
                    SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);

                    if (ClipboardHelper.CanCopySerializedProperty(prop.propertyType))
                    {
                        ClipboardHelper.DoCopySerializedProperty(prop);
                    }
                }

                bool pasteCommand = ctrl && evt.keyCode == KeyCode.V;
                if (pasteCommand)
                {
                    int selectedIndex = listView.selectedItems
                        .Cast<int>()
                        .DefaultIfEmpty(-1)
                        .First();

                    if (selectedIndex == -1)
                    {
                        return;
                    }

                    int propIndex = itemIndexToPropertyIndex[selectedIndex];
                    if(propIndex >= property.arraySize)
                    {
                        return;
                    }
                    SerializedProperty prop = property.GetArrayElementAtIndex(propIndex);

                    (bool pasteHasReflection, bool pasteHasValue) = ClipboardHelper.CanPasteSerializedProperty(prop.propertyType);
                    // Debug.Log($"{pasteHasReflection}, {pasteHasValue}");
                    if (pasteHasReflection && pasteHasValue)
                    {
                        ClipboardHelper.DoPasteSerializedProperty(prop);
                        prop.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            foldoutContent.Insert(0, preContent);

            // UpdateAddRemoveButtons();
            root.Add(listView);

            if (listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0)
            {
                root.Add(numberOfItemsTotalField);
            }

            return (root, listViewAddButton, listViewRemoveButton);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);

            (int minSize, int maxSize) = result.ArraySize;

            int curSize = FieldWithInfo.SerializedProperty.arraySize;
            bool canNotAddMore = maxSize >= 0 && curSize >= maxSize;
            _addButton.SetEnabled(!canNotAddMore);
            bool canNotRemoveMore = minSize >= 0 && curSize <= minSize;
            _removeButton.SetEnabled(!canNotRemoveMore);

            return result;
        }
    }
}
#endif
