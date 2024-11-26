using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections;
using SaintsField.Editor.Drawers;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(fieldWithInfo)
        {
        }

        private static void InvokeCallback(string callback, object newValue, object parent)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(parent);
            types.Reverse();

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            foreach (Type type in types)
            {
                MethodInfo methodInfo = type.GetMethod(callback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]
                {
                    newValue,
                });

                try
                {
                    methodInfo.Invoke(parent, passParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    // Debug.Assert(e.InnerException != null);
                    // return e.InnerException?.Message ?? e.Message;
                    return;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    // return e.Message;
                    return;
                }
                catch (Exception e)
                {
                    // _error = e.Message;
                    Debug.LogException(e);
                    // return e.Message;
                    return;
                }

                // return "";
                return;
            }

            string error = $"No field or method named `{callback}` found on `{parent}`";
            Debug.LogError(error);
        }

        private static void InvokeArraySizeCallback(string callback, SerializedProperty property, MemberInfo memberInfo)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning("Property disposed unexpectedly");
                return;
            }

            (string error, int _, object newValue) = Util.GetValue(property, memberInfo, parent);
            if (error != "")
            {
                Debug.LogError(error);
                return;
            }

            InvokeCallback(callback, newValue, parent);

        }

        #region UI Toolkit
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        private PropertyField _result;

        private class UserDataPayload
        {
            public string XML;
            public Label Label;
            public string FriendlyName;
            public RichTextDrawer RichTextDrawer;
        }

        private VisualElement _fieldElement;
        private bool _arraySizeCondition;
        private bool _richLabelCondition;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            UserDataPayload userDataPayload = new UserDataPayload
            {
                FriendlyName = FieldWithInfo.SerializedProperty.displayName,
            };

            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            ArraySizeAttribute arraySizeAttribute =
                FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();

            VisualElement result = listDrawerSettingsAttribute == null
                ? new PropertyField(FieldWithInfo.SerializedProperty)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                }
                : MakeListDrawerSettingsField(listDrawerSettingsAttribute, arraySizeAttribute?.Min ?? -1, arraySizeAttribute?.Max ?? -1);

            OnArraySizeChangedAttribute onArraySizeChangedAttribute = FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            if (onArraySizeChangedAttribute != null)
            {
                OnArraySizeChangedUIToolkit(onArraySizeChangedAttribute.Callback, result, FieldWithInfo.SerializedProperty, (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
            }

            result.userData = userDataPayload;

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

            bool needUpdate = ifCondition || _arraySizeCondition || _richLabelCondition;

            return (_fieldElement = result, needUpdate);
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
                // text = "<",
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

        protected override PreCheckResult OnUpdateUIToolKit()
        // private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition, bool richLabelCondition, FieldInfo info, object parent)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit();

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
#endif
        #endregion

        private RichTextDrawer _richTextDrawer;

        private string _curXml;
        private RichTextDrawer.RichTextChunk[] _curXmlChunks;

        private class ImGuiListInfo
        {
            public SerializedProperty Property;
            public PreCheckResult PreCheckResult;

            public bool HasSearch;
            public bool HasPaging;
            public PagingInfo PagingInfo;
            public string SearchText = string.Empty;
            public int PageIndex;
            public int NumberOfItemsPrePage;
        }

        private ReorderableList _imGuiReorderableList;
        private ImGuiListInfo _imGuiListInfo;

        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            bool isArray = FieldWithInfo.SerializedProperty.isArray;
            OnArraySizeChangedAttribute onArraySizeChangedAttribute =
                FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            int arraySize = -1;
            if (isArray && onArraySizeChangedAttribute != null)
            {
                arraySize = FieldWithInfo.SerializedProperty.arraySize;
            }

            if (preCheckResult.ArraySize != -1 && (
                    (preCheckResult.ArraySize == 0 && FieldWithInfo.SerializedProperty.arraySize > 0)
                    || (preCheckResult.ArraySize > 0 && FieldWithInfo.SerializedProperty.arraySize == 0)
                ))
            {
                FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();

            // bool hasSearch = listDrawerSettingsAttribute?.Searchable ?? false;
            // bool hasPaging = listDrawerSettingsAttribute?.NumberOfItemsPerPage > 0;

            // if(hasSearch || hasPaging || arraySizeAttribute != null)
            if(listDrawerSettingsAttribute != null)
            {
                Rect rect = EditorGUILayout.GetControlRect(true, 0f);
                float listDrawerHeight = GetHeightIMGUI(rect.width);
                Rect position = GUILayoutUtility.GetRect(0, listDrawerHeight);
                ArraySizeAttribute arraySizeAttribute = FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
                // ReSharper disable once ConvertToUsingDeclaration
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position, arraySizeAttribute, listDrawerSettingsAttribute.Delayed);
                    if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                       arraySize != FieldWithInfo.SerializedProperty.arraySize)
                    {
                        FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                        InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                            FieldWithInfo.SerializedProperty,
                            (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                    }

                }
                return;
            }

            GUIContent useGUIContent = preCheckResult.HasRichLabel
                ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length))
                : new GUIContent(FieldWithInfo.SerializedProperty.displayName);

            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, useGUIContent,
                    GUILayout.ExpandWidth(true));

                if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                   arraySize != FieldWithInfo.SerializedProperty.arraySize)
                {
                    // Debug.Log("size changed");
                    FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                        FieldWithInfo.SerializedProperty,
                        (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                }
            }

            if (preCheckResult.HasRichLabel)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                // GUILayout.Label("Mouse over!");
                Rect richRect = new Rect(lastRect)
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                };
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if(_richTextDrawer == null)
                {
                    _richTextDrawer = new RichTextDrawer();
                }

                // Debug.Log(preCheckResult.RichLabelXml);
                if (_curXml != preCheckResult.RichLabelXml)
                {
                    _curXmlChunks =
                        RichTextDrawer
                            .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName, FieldWithInfo.FieldInfo, FieldWithInfo.Target)
                            .ToArray();
                }

                _curXml = preCheckResult.RichLabelXml;

                _richTextDrawer.DrawChunks(richRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
            }
        }

        ~SerializedFieldRenderer()
        {
            _richTextDrawer = null;
        }

        public override void OnDestroy()
        {
            _richTextDrawer?.Dispose();
            _richTextDrawer = null;
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            if(listDrawerSettingsAttribute is null)
            {
                return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
            }

            bool hasSearch = listDrawerSettingsAttribute.Searchable;
            bool hasPaging = listDrawerSettingsAttribute.NumberOfItemsPerPage > 0;
            // ArraySizeAttribute arraySizeAttribute = FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
            //
            // if(!hasSearch && !hasPaging && arraySizeAttribute == null)
            // {
            //     Debug.Log($"No IMGUIListInfo");
            //     _imGuiListInfo = null;
            //     return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
            // }

            if (_imGuiListInfo == null)
            {
                int numberOfItemsPrePage = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                _imGuiListInfo = new ImGuiListInfo
                {
                    Property = FieldWithInfo.SerializedProperty,
                    PreCheckResult = preCheckResult,
                    HasSearch = hasSearch,
                    HasPaging = hasPaging,
                    PagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, 0, "", numberOfItemsPrePage),
                    NumberOfItemsPrePage = numberOfItemsPrePage,
                    PageIndex = 0,
                    SearchText = "",
                };
                FieldWithInfo.SerializedProperty.isExpanded = true;
            }
            else
            {
                _imGuiListInfo.PagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, _imGuiListInfo.PageIndex,
                    _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
            }

            if (!FieldWithInfo.SerializedProperty.isExpanded)
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            int extraLineCount = (hasSearch || hasPaging) ? 4 : 3;

            float height = _imGuiListInfo.PagingInfo.IndexesCurPage
                               .Select(index => EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index), true))
                               .Sum()
                           + (_imGuiListInfo.PagingInfo.IndexesCurPage.Count == 0? EditorGUIUtility.singleLineHeight: 0)
                           + SaintsPropertyDrawer.SingleLineHeight * extraLineCount;  // header with controller line, footer (plus, minus)
            return height;
        }

        protected override void RenderPositionTarget(Rect position, PreCheckResult preCheckResult)
        {
            bool isArray = FieldWithInfo.SerializedProperty.isArray;
            OnArraySizeChangedAttribute onArraySizeChangedAttribute =
                FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            int arraySize = -1;
            if (isArray && onArraySizeChangedAttribute != null)
            {
                arraySize = FieldWithInfo.SerializedProperty.arraySize;
            }

            if (preCheckResult.ArraySize != -1 && FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
            {
                FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
                if(_imGuiListInfo != null && listDrawerSettingsAttribute != null)
                {
                    _imGuiListInfo.PreCheckResult = preCheckResult;
                    ArraySizeAttribute arraySizeAttribute = FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
                    // ReSharper disable once ConvertToUsingDeclaration
                    using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                    {
                        DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position, arraySizeAttribute, listDrawerSettingsAttribute.Delayed);
                        if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                           arraySize != FieldWithInfo.SerializedProperty.arraySize)
                        {
                            FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                            InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                                FieldWithInfo.SerializedProperty,
                                (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                        }
                    }
                    return;
                }

                GUIContent useGUIContent = preCheckResult.HasRichLabel
                    ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length), tooltip: FieldWithInfo.SerializedProperty.tooltip)
                    : new GUIContent(FieldWithInfo.SerializedProperty.displayName, tooltip: FieldWithInfo.SerializedProperty.tooltip);

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, useGUIContent, true);
                    if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                       arraySize != FieldWithInfo.SerializedProperty.arraySize)
                    {
                        FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                        InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                            FieldWithInfo.SerializedProperty,
                            (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                    }
                }

                #region RichLabel
                if (preCheckResult.HasRichLabel)
                {
                    Rect richRect = new Rect(position)
                    {
                        height = SaintsPropertyDrawer.SingleLineHeight,
                    };

                    // EditorGUI.DrawRect(richRect, Color.blue);
                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if(_richTextDrawer == null)
                    {
                        _richTextDrawer = new RichTextDrawer();
                    }

                    // Debug.Log(preCheckResult.RichLabelXml);
                    if (_curXml != preCheckResult.RichLabelXml)
                    {
                        _curXmlChunks =
                            RichTextDrawer
                                .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName, FieldWithInfo.FieldInfo, FieldWithInfo.Target)
                                .ToArray();
                    }

                    _curXml = preCheckResult.RichLabelXml;

                    _richTextDrawer.DrawChunks(richRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
                }
                #endregion
            }
            // EditorGUI.DrawRect(position, Color.blue);
        }
        private class UnsetGuiStyleFixedHeight : IDisposable
        {
            private readonly GUIStyle _guiStyle;
            private readonly float _oldValue;

            public UnsetGuiStyleFixedHeight(GUIStyle guiStyle)
            {
                _guiStyle = guiStyle;
                _oldValue = guiStyle.fixedHeight;
                _guiStyle.fixedHeight = 0;
            }

            public void Dispose()
            {
                _guiStyle.fixedHeight = _oldValue;
            }
        }

        private void DrawListDrawerSettingsField(SerializedProperty property, Rect position, ArraySizeAttribute arraySizeAttribute, bool delayed)
        {
            Rect usePosition = new Rect(position)
            {
                y = position.y + 1,
                height = position.height - 2,
            };

            if (!property.isExpanded)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    GUIStyle headerBackground = "RL Header";
                    using (new UnsetGuiStyleFixedHeight(headerBackground))
                    {
                        headerBackground.Draw(usePosition, false, false, false, false);
                    }
                }

                Rect paddingTitle = new Rect(usePosition)
                {
                    x = usePosition.x + 6,
                    y = usePosition.y + 1,
                    height = usePosition.height - 2,
                    width = usePosition.width - 12,
                };
                DrawListDrawerHeader(paddingTitle, delayed);
                return;
            }

            PagingInfo newPagingInfo = GetPagingInfo(property, _imGuiListInfo.PageIndex, _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
            if (!newPagingInfo.IndexesCurPage.SequenceEqual(_imGuiListInfo.PagingInfo.IndexesCurPage))
            {
                _imGuiReorderableList = null;
            }

            _imGuiListInfo.PagingInfo = newPagingInfo;

            if (_imGuiReorderableList == null)
            {
                _imGuiReorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        headerHeight = SaintsPropertyDrawer.SingleLineHeight * ((_imGuiListInfo.HasPaging || _imGuiListInfo.HasSearch)? 2: 1),
                    };
                _imGuiReorderableList.drawHeaderCallback += v => DrawListDrawerHeader(v, delayed);
                _imGuiReorderableList.elementHeightCallback += DrawListDrawerItemHeight;
                _imGuiReorderableList.drawElementCallback += DrawListDrawerItem;

                if(arraySizeAttribute != null)
                {
                    if(arraySizeAttribute.Min > 0)
                    {
                        // _imGuiReorderableList.displayRemove = true;
                        // _imGuiReorderableList.onRemoveCallback += r =>
                        // {
                        //     Debug.Log(r);
                        // };
                        _imGuiReorderableList.onCanRemoveCallback += r =>
                        {
                            bool canRemove = r.count > arraySizeAttribute.Min;
                            // Debug.Log($"canRemove={canRemove}, count={r.count}, min={arraySizeAttribute.Min}");
                            return canRemove;
                        };
                    }

                    if (arraySizeAttribute.Max > 0)
                    {
                        _imGuiReorderableList.onCanAddCallback += r =>
                        {
                            bool canAdd = r.count < arraySizeAttribute.Max;
                            // Debug.Log($"canAdd={canAdd}, count={r.count}, max={arraySizeAttribute.Max}");
                            return canAdd;
                        };
                    }
                    // _imGuiReorderableList.onCanAddCallback += _ => !(arraySizeAttribute.Min >= 0 && property.arraySize <= arraySizeAttribute.Min);
                }
            }



            // Debug.Log(ReorderableList.defaultBehaviours);
            // Debug.Log(ReorderableList.defaultBehaviours.headerBackground);

            using(new UnsetGuiStyleFixedHeight("RL Header"))
            {
                _imGuiReorderableList.DoList(usePosition);
            }
        }

        private Texture2D _iconDown;
        private Texture2D _iconLeft;
        private Texture2D _iconRight;

        private void DrawListDrawerHeader(Rect rect, bool delayed)
        {
            // const float twoNumberInputWidth = 20;
            const float inputWidth = 30;
            // const float itemsLabelWidth = 75;
            const float itemsLabelWidth = 65;
            const float buttonWidth = 19;
            // const float pagingLabelWidth = 35;
            const float pagingLabelWidth = 30;
            const float pagingSepWidth = 8;

            const float gap = 5;

            (Rect titleRect, Rect controlRect) = RectUtils.SplitHeightRect(rect, EditorGUIUtility.singleLineHeight);
            controlRect.height -= 1;

            (Rect titleFoldRect, Rect titleButtonRect) = RectUtils.SplitWidthRect(titleRect, 16);

            if (!_imGuiListInfo.HasPaging && !_imGuiListInfo.HasSearch)  // draw element count container
            {
                (Rect titleButtonNewRect, Rect titleItemTotalRect) =
                    RectUtils.SplitWidthRect(titleButtonRect, titleButtonRect.width - 50);
                titleItemTotalRect.y += 1;
                titleItemTotalRect.height -= 2;

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newCount = EditorGUI.DelayedIntField(titleItemTotalRect, GUIContent.none,
                        _imGuiListInfo.PagingInfo.IndexesAfterSearch.Count);
                    if (changed.changed)
                    {
                        _imGuiListInfo.Property.arraySize = newCount;
                        _imGuiListInfo.Property.serializedObject.ApplyModifiedProperties();
                        _imGuiListInfo.PagingInfo = GetPagingInfo(_imGuiListInfo.Property, _imGuiListInfo.PageIndex,
                            _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
                        return;
                    }

                    // EditorGUI.LabelField(new Rect(titleItemTotalRect)
                    // {
                    //     width = titleItemTotalRect.width - 6,
                    // }, "Items", new GUIStyle("label") { alignment = TextAnchor.MiddleRight, normal =
                    // {
                    //     textColor = Color.gray,
                    // }, fontStyle = FontStyle.Italic});
                }

                titleButtonRect = titleButtonNewRect;
            }

            if(GUI.Button(titleButtonRect, "", GUIStyle.none))
            {
                _imGuiListInfo.Property.isExpanded = !_imGuiListInfo.Property.isExpanded;
                return;
            }

            PreCheckResult preCheckResult = _imGuiListInfo.PreCheckResult;
            if (preCheckResult.HasRichLabel)
            {
                if(_richTextDrawer == null)
                {
                    _richTextDrawer = new RichTextDrawer();
                }

                // Debug.Log(preCheckResult.RichLabelXml);
                if (_curXml != preCheckResult.RichLabelXml)
                {
                    _curXmlChunks =
                        RichTextDrawer
                            .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName, FieldWithInfo.FieldInfo, FieldWithInfo.Target)
                            .ToArray();
                }

                _curXml = preCheckResult.RichLabelXml;

                _richTextDrawer.DrawChunks(titleButtonRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
            }
            else
            {
                EditorGUI.LabelField(titleButtonRect, _imGuiListInfo.Property.displayName);
            }

            if (_imGuiListInfo.Property.isExpanded)
            {
                if (!_iconDown)
                {
                    _iconDown = Util.LoadResource<Texture2D>("classic-dropdown.png");
                }
                GUI.DrawTexture(titleFoldRect, _iconDown);
            }
            else
            {
                if (!_iconRight)
                {
                    _iconRight = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                }
                GUI.DrawTexture(titleFoldRect, _iconRight);
                return;
            }

            float searchInputWidth = rect.width - inputWidth * 2 - itemsLabelWidth - pagingSepWidth - buttonWidth * 2 - pagingLabelWidth;

            (Rect searchRect, Rect pagingRect) = RectUtils.SplitWidthRect(controlRect, _imGuiListInfo.HasPaging? searchInputWidth: controlRect.width);

            if(_imGuiListInfo.HasSearch)
            {
                if(delayed)
                {
                    _imGuiListInfo.SearchText = EditorGUI.DelayedTextField(new Rect(searchRect)
                    {
                        width = searchRect.width - gap,
                    }, GUIContent.none, _imGuiListInfo.SearchText);
                }
                else
                {
                    _imGuiListInfo.SearchText = EditorGUI.TextField(new Rect(searchRect)
                    {
                        width = searchRect.width - gap,
                    }, GUIContent.none, _imGuiListInfo.SearchText);
                }
                if (string.IsNullOrEmpty(_imGuiListInfo.SearchText))
                {
                    EditorGUI.LabelField(new Rect(searchRect)
                    {
                        width = searchRect.width - 6,
                    }, "Search", new GUIStyle("label") { alignment = TextAnchor.MiddleRight, normal =
                    {
                        textColor = Color.gray,
                    }, fontStyle = FontStyle.Italic});
                }
            }

            if(_imGuiListInfo.HasPaging)
            {
                Rect numberOfItemsPerPageRect = new Rect(pagingRect)
                {
                    width = inputWidth,
                };
                _imGuiListInfo.NumberOfItemsPrePage = EditorGUI.IntField(numberOfItemsPerPageRect, GUIContent.none,
                    _imGuiListInfo.NumberOfItemsPrePage);

                Rect numberOfItemsSepRect = new Rect(numberOfItemsPerPageRect)
                {
                    x = numberOfItemsPerPageRect.x + numberOfItemsPerPageRect.width,
                    width = pagingSepWidth,
                };
                EditorGUI.LabelField(numberOfItemsSepRect, $"/");

                Rect numberOfItemsTotalRect = new Rect(numberOfItemsSepRect)
                {
                    x = numberOfItemsSepRect.x + numberOfItemsSepRect.width,
                    width = itemsLabelWidth,
                };
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newCount = EditorGUI.DelayedIntField(numberOfItemsTotalRect, GUIContent.none,
                        _imGuiListInfo.PagingInfo.IndexesAfterSearch.Count);
                    if (changed.changed)
                    {
                        _imGuiListInfo.Property.arraySize = newCount;
                        _imGuiListInfo.Property.serializedObject.ApplyModifiedProperties();
                        _imGuiListInfo.PagingInfo = GetPagingInfo(_imGuiListInfo.Property, _imGuiListInfo.PageIndex,
                            _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
                        return;
                    }
                }
                // EditorGUI.LabelField(totalItemRect, $"/ 8888 Items");

                EditorGUI.LabelField(numberOfItemsTotalRect, "Items", new GUIStyle("label") { alignment = TextAnchor.MiddleRight, normal =
                {
                    textColor = Color.gray,
                }, fontStyle = FontStyle.Italic});

                Rect prePageRect = new Rect(numberOfItemsTotalRect)
                {
                    x = numberOfItemsTotalRect.x + numberOfItemsTotalRect.width,
                    width = buttonWidth,
                };
                using (new EditorGUI.DisabledScope(_imGuiListInfo.PagingInfo.CurPageIndex <= 0))
                {
                    if (!_iconLeft)
                    {
                        _iconLeft = Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                    }
                    if (GUI.Button(prePageRect, _iconLeft, EditorStyles.miniButtonLeft))
                    {
                        if (_imGuiListInfo.PagingInfo.CurPageIndex > 0)
                        {
                            _imGuiListInfo.PageIndex -= 1;
                        }
                    }
                }

                Rect pageRect = new Rect(prePageRect)
                {
                    x = prePageRect.x + prePageRect.width,
                    width = inputWidth,
                };
                _imGuiListInfo.PageIndex =
                    EditorGUI.IntField(pageRect, GUIContent.none, _imGuiListInfo.PageIndex + 1) - 1;
                Rect totalPageRect = new Rect(pageRect)
                {
                    x = pageRect.x + pageRect.width,
                    width = pagingLabelWidth,
                };
                EditorGUI.LabelField(totalPageRect, $"/ {_imGuiListInfo.PagingInfo.PageCount}");
                // EditorGUI.LabelField(totalPageRect, $"/ 888");

                Rect nextPageRect = new Rect(totalPageRect)
                {
                    x = totalPageRect.x + totalPageRect.width,
                    width = buttonWidth,
                };
                using (new EditorGUI.DisabledScope(_imGuiListInfo.PagingInfo.CurPageIndex >=
                                                   _imGuiListInfo.PagingInfo.PageCount - 1))
                {
                    if (!_iconRight)
                    {
                        _iconRight = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                    }
                    if (GUI.Button(nextPageRect, _iconRight, EditorStyles.miniButtonRight))
                    {
                        if (_imGuiListInfo.PagingInfo.CurPageIndex < _imGuiListInfo.PagingInfo.PageCount - 1)
                        {
                            _imGuiListInfo.PageIndex += 1;
                        }
                    }
                }
            }
        }

        private float DrawListDrawerItemHeight(int index)
        {
            if (_imGuiListInfo.PagingInfo.IndexesCurPage.Contains(index))
            {
                if(index >= _imGuiListInfo.Property.arraySize)
                {
                    return 0;
                }
                SerializedProperty element = FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true);
            }

            return 0;
        }

        private void DrawListDrawerItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.height <= 0)
            {
                return;
            }

            SerializedProperty property = _imGuiListInfo.Property.GetArrayElementAtIndex(index);

            Rect useRect = property.propertyType == SerializedPropertyType.Generic
                ? new Rect(rect)
                {
                    x = rect.x + 12,
                    width = rect.width - 12,
                }
                : rect;

            EditorGUI.PropertyField(useRect, property, new GUIContent($"Element {index}"), true);
        }

        #region ListUtils

        private static IEnumerable<int> SearchArrayProperty(SerializedProperty property, string search)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (int index in Enumerable.Range(0, property.arraySize))
            {
                SerializedProperty childProperty = property.GetArrayElementAtIndex(index);
                if(SearchProp(childProperty, search))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"found: {childProperty.propertyPath}");
#endif
                    yield return index;
                }
            }
        }

        private static bool SearchProp(SerializedProperty property, string search)
        {
            SerializedPropertyType propertyType;
            try
            {
                propertyType = property.propertyType;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }

            // Debug.Log($"{property.propertyPath} is {propertyType}");

            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString().Contains(search);
                case SerializedPropertyType.Float:
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    return property.floatValue.ToString().Contains(search);
                case SerializedPropertyType.String:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"{property.propertyPath}={property.stringValue} contains {search}={property.stringValue?.Contains(search)}");
#endif
                    return property.stringValue?.Contains(search) ?? false;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString().Contains(search);
                case SerializedPropertyType.ObjectReference:
                    // ReSharper disable once Unity.NoNullPropagation
                    if (property.objectReferenceValue is ScriptableObject so)
                    {
                        return SearchSoProp(so, search);
                    }
                    return property.objectReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Enum:
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if(property.enumNames.Length <= property.enumValueIndex || property.enumValueIndex < 0)
                    {
                        return false;
                    }
                    return property.enumNames[property.enumValueIndex].Contains(search);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString().Contains(search);
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString().Contains(search);
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString().Contains(search);
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString().Contains(search);
                case SerializedPropertyType.ArraySize:
                    if (property.isArray)
                    {
                        return property.arraySize.ToString().Contains(search);
                    }
                    goto default;
                case SerializedPropertyType.Character:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue.ToString().Contains(search);
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString().Contains(search);
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString().Contains(search);
                case SerializedPropertyType.ExposedReference:
                    // ReSharper disable once Unity.NoNullPropagation
                    return property.exposedReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString().Contains(search);
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString().Contains(search);
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString().Contains(search);
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString().Contains(search);
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString().Contains(search);
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename.Contains(search);
#endif
                case SerializedPropertyType.Generic:
                {
                    if (property.isArray)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                        Debug.Log($"is array {property.arraySize}: {property.propertyPath}");
#endif
                        return Enumerable.Range(0, property.arraySize)
                            .Select(property.GetArrayElementAtIndex)
                            .Any(childProperty => SearchProp(childProperty, search));
                    }

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (SerializedProperty child in SerializedUtils.GetPropertyChildren(property))
                    {
                        if(SearchProp(child, search))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"found child: {child.propertyPath}");
#endif
                            return true;
                        }
                    }

                    return false;
                }
                case SerializedPropertyType.Gradient:
#if UNITY_2021_1_OR_NEWER
                case SerializedPropertyType.Hash128:
#endif
                default:
                    return false;
            }
        }

        private static bool SearchSoProp(ScriptableObject so, string search)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using(SerializedObject serializedObject = new SerializedObject(so))
            {
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (SearchProp(iterator, search))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private struct PagingInfo
        {
            public IReadOnlyList<int> IndexesAfterSearch;
            public List<int> IndexesCurPage;
            public int CurPageIndex;
            public int PageCount;
        }

        private static PagingInfo GetPagingInfo(SerializedProperty property, int newPageIndex, string search, int numberOfItemsPerPage)
        {
            IReadOnlyList<int> fullIndexResults = string.IsNullOrEmpty(search)
                ? Enumerable.Range(0, property.arraySize).ToList()
                : SearchArrayProperty(property, search).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"index search={search} result: {string.Join(",", fullIndexResults)}; numberOfItemsPerPage={numberOfItemsPerPage}");
#endif

            // List<int> itemIndexToPropertyIndex = fullIndexResults.ToList();
            int curPageIndex;

            int pageCount;
            int skipStart;
            int itemCount;
            if (numberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                curPageIndex = 0;
                skipStart = 0;
                itemCount = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.CeilToInt((float)fullIndexResults.Count / numberOfItemsPerPage);
                curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                skipStart = curPageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            List<int> curPageItemIndexes = fullIndexResults.Skip(skipStart).Take(itemCount).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"set items: {string.Join(", ", curPageItemIndexes)}, itemIndexToPropertyIndex={string.Join(",", fullIndexResults)}");
#endif

            return new PagingInfo
            {
                IndexesAfterSearch = fullIndexResults,
                IndexesCurPage = curPageItemIndexes,
                CurPageIndex = curPageIndex,
                PageCount = pageCount,
            };
        }


        #endregion

        public override string ToString() => $"Ser<{FieldWithInfo.FieldInfo?.Name ?? FieldWithInfo.SerializedProperty.displayName}>";
    }
}
