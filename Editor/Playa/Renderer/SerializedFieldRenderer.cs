using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(fieldWithInfo)
        {
        }

        #region UI Toolkit
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        private PropertyField _result;

        private class UserDataPayload
        {
            public string xml;
            public Label label;
            public string friendlyName;
            public RichTextDrawer richTextDrawer;
        }

        public override VisualElement CreateVisualElement()
        {
            UserDataPayload userDataPayload = new UserDataPayload
            {
                friendlyName = FieldWithInfo.SerializedProperty.displayName,
            };

            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();

            VisualElement result = listDrawerSettingsAttribute == null
                ? new PropertyField(FieldWithInfo.SerializedProperty)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                }
                : MakeListDrawerSettingsField(listDrawerSettingsAttribute);

            result.userData = userDataPayload;

            // ReSharper disable once InvertIf
            // if(TryFixUIToolkit && FieldWithInfo.FieldInfo?.GetCustomAttributes(typeof(ISaintsAttribute), true).Length == 0)
            // {
            //     // Debug.Log($"{fieldWithInfo.fieldInfo.Name} {arr.Length}");
            //     _result = result;
            //     _result.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            // }

            // disable/enable/show/hide
            bool ifCondition = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute
                                                                           // ReSharper disable once MergeIntoLogicalPattern
                                                                           || each is PlayaEnableIfAttribute
                                                                           // ReSharper disable once MergeIntoLogicalPattern
                                                                           || each is PlayaDisableIfAttribute) > 0;
            bool arraySizeCondition = FieldWithInfo.PlayaAttributes.Any(each => each is IPlayaArraySizeAttribute);
            bool richLabelCondition = FieldWithInfo.PlayaAttributes.Any(each => each is PlayaRichLabelAttribute);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log(
                $"SerField: {FieldWithInfo.SerializedProperty.displayName}({FieldWithInfo.SerializedProperty.propertyPath}); if={ifCondition}; arraySize={arraySizeCondition}, richLabel={richLabelCondition}");
#endif
            if (ifCondition || arraySizeCondition || richLabelCondition)
            {
                result.RegisterCallback<AttachToPanelEvent>(_ =>
                    result.schedule
                        .Execute(() => UIToolkitCheckUpdate(result, ifCondition, arraySizeCondition, richLabelCondition))
                        .Every(100)
                );
            }
            //
            // result.RegisterCallback<DetachFromPanelEvent>(_ =>
            // {
            //     // ReSharper disable once InvertIf
            //     if(userDataPayload.richTextDrawer != null)
            //     {
            //         userDataPayload.richTextDrawer.Dispose();
            //         userDataPayload.richTextDrawer = null;
            //     }
            // });

            return result;
        }

        private VisualElement MakeListDrawerSettingsField(ListDrawerSettingsAttribute listDrawerSettingsAttribute)
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

            // listView.itemsAdded += objects =>
            // {
            //     Debug.Log("itemAdded");
            //     property.arraySize += objects.Count();
            //     property.serializedObject.ApplyModifiedProperties();
            // };

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
            Label numberOfItemsPerPageLabel = new Label($" / {property.arraySize} Items")
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

            void UpdateNumberOfItemsPerPage(int newValue)
            {
                int newValueClamp = Mathf.Max(newValue, 0);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                Debug.Log($"update number of items per page {newValueClamp}");
#endif
                UpdatePage(curPageIndex, newValueClamp);
            }

            numberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(() =>
            {
                property.arraySize += 1;
                property.serializedObject.ApplyModifiedProperties();
                int totalVisiblePage = Mathf.CeilToInt((float)itemIndexToPropertyIndex.Count / numberOfItemsPerPageField.value);
                UpdatePage(totalVisiblePage - 1, numberOfItemsPerPageField.value);
                numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";
            });

            listView.itemsRemoved += objects =>
            {
                // int[] sources = listView.itemsSource.Cast<int>().ToArray();
                List<int> curRemoveObjects = objects.ToList();

                foreach (int index in curRemoveObjects.Select(removeIndex => itemIndexToPropertyIndex[removeIndex]).OrderByDescending(each => each))
                {
                    Debug.Log(index);
                    property.DeleteArrayElementAtIndex(index);
                }

                // itemIndexToPropertyIndex.RemoveAll(each => curRemoveObjects.Contains(each));
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";

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
            pagingContainer.Add(numberOfItemsPerPageLabel);

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

            return listView;
        }



        private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition, bool richLabelCondition)
        {
            PreCheckResult preCheckResult = default;
            // Debug.Log(preCheckResult.RichLabelXml);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (ifCondition)
            {
                preCheckResult = UIToolkitOnUpdate(FieldWithInfo, result, true);
            }

            if(!ifCondition && (arraySizeCondition || richLabelCondition))
            {
                preCheckResult = GetPreCheckResult(FieldWithInfo);
            }

            if(arraySizeCondition)
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

            if (richLabelCondition)
            {
                string xml = preCheckResult.RichLabelXml;
                // Debug.Log(xml);
                UserDataPayload userDataPayload = (UserDataPayload) result.userData;
                if (xml != userDataPayload.xml)
                {
                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if (userDataPayload.richTextDrawer == null)
                    {
                        userDataPayload.richTextDrawer = new RichTextDrawer();
                    }
                    if(userDataPayload.label == null)
                    {
                        UIToolkitUtils.WaitUntilThenDo(
                            result,
                            () =>
                            {
                                Label label = result.Q<Label>(className: "unity-label");
                                if (label == null)
                                {
                                    return (false, null);
                                }
                                return (true, label);
                            },
                            label =>
                            {
                                userDataPayload.label = label;
                            }
                        );
                    }
                    else
                    {
                        userDataPayload.xml = xml;
                        UIToolkitUtils.SetLabel(userDataPayload.label, RichTextDrawer.ParseRichXml(xml, userDataPayload.friendlyName), userDataPayload.richTextDrawer);
                    }
                }
            }
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
            public SerializedProperty property;
            public PreCheckResult preCheckResult;

            public bool hasSearch;
            public bool hasPaging;
            public PagingInfo pagingInfo;
            public string searchText = string.Empty;
            public int pageIndex = 0;
            public int numberOfItemsPrePage = 0;
        }

        private ReorderableList _imGuiReorderableList;
        private ImGuiListInfo _imGuiListInfo;

        public override void Render()
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (preCheckResult.ArraySize != -1 && (
                    (preCheckResult.ArraySize == 0 && FieldWithInfo.SerializedProperty.arraySize > 0)
                    || (preCheckResult.ArraySize > 0 && FieldWithInfo.SerializedProperty.arraySize == 0)
                ))
            {
                FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            }

            if (!preCheckResult.IsShown)
            {
                return;
            }

            using(new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();

                bool hasSearch = listDrawerSettingsAttribute?.Searchable ?? false;
                bool hasPaging = listDrawerSettingsAttribute?.NumberOfItemsPerPage > 0;

                if(hasSearch || hasPaging)
                {
                    float listDrawerHeight = GetHeight();
                    Rect position = GUILayoutUtility.GetRect(0, listDrawerHeight);
                    DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position);
                    return;
                }

                GUIContent useGUIContent = preCheckResult.HasRichLabel
                    ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length))
                    : new GUIContent(FieldWithInfo.SerializedProperty.displayName);

                EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, useGUIContent, GUILayout.ExpandWidth(true));

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
                                .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName)
                                .ToArray();
                    }

                    _curXml = preCheckResult.RichLabelXml;

                    _richTextDrawer.DrawChunks(richRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
                }
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

        public override float GetHeight()
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            if(listDrawerSettingsAttribute is null)
            {
                return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
            }

            bool hasSearch = listDrawerSettingsAttribute.Searchable;
            bool hasPaging = listDrawerSettingsAttribute.NumberOfItemsPerPage > 0;

            if(!hasSearch && !hasPaging)
            {
                _imGuiListInfo = null;
                return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
            }

            if (_imGuiListInfo == null)
            {
                int numberOfItemsPrePage = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                _imGuiListInfo = new ImGuiListInfo
                {
                    property = FieldWithInfo.SerializedProperty,
                    preCheckResult = preCheckResult,
                    hasSearch = hasSearch,
                    hasPaging = hasPaging,
                    pagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, 0, "", numberOfItemsPrePage),
                    numberOfItemsPrePage = numberOfItemsPrePage,
                    pageIndex = 0,
                    searchText = "",
                };
                FieldWithInfo.SerializedProperty.isExpanded = true;
            }

            if (!FieldWithInfo.SerializedProperty.isExpanded)
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            float height = _imGuiListInfo.pagingInfo.IndexesCurPage
                               .Select(index => EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index), true))
                               .Sum()
                           + (_imGuiListInfo.pagingInfo.IndexesCurPage.Count == 0? EditorGUIUtility.singleLineHeight: 0)
                           + SaintsPropertyDrawer.SingleLineHeight * 4;  // header with controller line, footer (plus, minus)
            return height;
        }

        public override void RenderPosition(Rect position)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);

            if (preCheckResult.ArraySize != -1 && FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
            {
                FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            }

            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
                if(_imGuiListInfo != null && listDrawerSettingsAttribute != null)
                {
                    _imGuiListInfo.preCheckResult = preCheckResult;
                    DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position);
                    return;
                }

                GUIContent useGUIContent = preCheckResult.HasRichLabel
                    ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length), tooltip: FieldWithInfo.SerializedProperty.tooltip)
                    : new GUIContent(FieldWithInfo.SerializedProperty.displayName, tooltip: FieldWithInfo.SerializedProperty.tooltip);

                EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, useGUIContent, true);

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
                                .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName)
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

        private void DrawListDrawerSettingsField(SerializedProperty property, Rect position)
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
                DrawListDrawerHeader(paddingTitle);
                return;
            }

            PagingInfo newPagingInfo = GetPagingInfo(property, _imGuiListInfo.pageIndex, _imGuiListInfo.searchText, _imGuiListInfo.numberOfItemsPrePage);
            if (!newPagingInfo.IndexesCurPage.SequenceEqual(_imGuiListInfo.pagingInfo.IndexesCurPage))
            {
                _imGuiReorderableList = null;
            }

            _imGuiListInfo.pagingInfo = newPagingInfo;

            if (_imGuiReorderableList == null)
            {
                _imGuiReorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        headerHeight = SaintsPropertyDrawer.SingleLineHeight * 2,
                    };
                _imGuiReorderableList.drawHeaderCallback += DrawListDrawerHeader;
                _imGuiReorderableList.elementHeightCallback += DrawListDrawerItemHeight;
                _imGuiReorderableList.drawElementCallback += DrawListDrawerItem;
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

        private void DrawListDrawerHeader(Rect rect)
        {
            // const float twoNumberInputWidth = 20;
            const float inputWidth = 30;
            // const float itemsLabelWidth = 75;
            const float itemsLabelWidth = 65;
            const float buttonWidth = 19;
            // const float pagingLabelWidth = 35;
            const float pagingLabelWidth = 30;

            const float gap = 5;

            (Rect titleRect, Rect controlRect) = RectUtils.SplitHeightRect(rect, EditorGUIUtility.singleLineHeight);
            controlRect.height -= 1;

            if(GUI.Button(titleRect, "", GUIStyle.none))
            {
                _imGuiListInfo.property.isExpanded = !_imGuiListInfo.property.isExpanded;
                return;
            }

            PreCheckResult preCheckResult = _imGuiListInfo.preCheckResult;
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
                            .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName)
                            .ToArray();
                }

                _curXml = preCheckResult.RichLabelXml;

                _richTextDrawer.DrawChunks(titleRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
            }
            else
            {
                EditorGUI.LabelField(titleRect, _imGuiListInfo.property.displayName);
            }

            if (_imGuiListInfo.property.isExpanded)
            {
                if (!_iconDown)
                {
                    _iconDown = Util.LoadResource<Texture2D>("classic-dropdown.png");
                }
                GUI.DrawTexture(new Rect(titleRect)
                {
                    x = titleRect.x + titleRect.width - 16,
                    width = 16,
                }, _iconDown);
            }
            else
            {
                if (!_iconLeft)
                {
                    _iconLeft = Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                }
                GUI.DrawTexture(new Rect(titleRect)
                {
                    x = titleRect.x + titleRect.width - 16,
                    width = 16,
                }, _iconLeft);
                return;
            }

            float searchInputWidth = rect.width - inputWidth * 2 - itemsLabelWidth - buttonWidth * 2 - pagingLabelWidth;

            (Rect searchRect, Rect pagingRect) = RectUtils.SplitWidthRect(controlRect, _imGuiListInfo.hasPaging? searchInputWidth: controlRect.width);

            if(_imGuiListInfo.hasSearch)
            {
                _imGuiListInfo.searchText = EditorGUI.TextField(new Rect(searchRect)
                {
                    width = searchRect.width - gap,
                }, GUIContent.none, _imGuiListInfo.searchText);
                if (string.IsNullOrEmpty(_imGuiListInfo.searchText))
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

            if(_imGuiListInfo.hasPaging)
            {
                Rect numberOfItemsPerPageRect = new Rect(pagingRect)
                {
                    width = inputWidth,
                };
                _imGuiListInfo.numberOfItemsPrePage = EditorGUI.IntField(numberOfItemsPerPageRect, GUIContent.none,
                    _imGuiListInfo.numberOfItemsPrePage);

                Rect totalItemRect = new Rect(numberOfItemsPerPageRect)
                {
                    x = numberOfItemsPerPageRect.x + numberOfItemsPerPageRect.width,
                    width = itemsLabelWidth,
                };
                EditorGUI.LabelField(totalItemRect, $"/ {_imGuiListInfo.pagingInfo.IndexesAfterSearch.Count} Items");
                // EditorGUI.LabelField(totalItemRect, $"/ 8888 Items");

                Rect prePageRect = new Rect(totalItemRect)
                {
                    x = totalItemRect.x + totalItemRect.width,
                    width = buttonWidth,
                };
                using (new EditorGUI.DisabledScope(_imGuiListInfo.pagingInfo.CurPageIndex <= 0))
                {
                    if (!_iconLeft)
                    {
                        _iconLeft = Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                    }
                    if (GUI.Button(prePageRect, _iconLeft, EditorStyles.miniButtonLeft))
                    {
                        if (_imGuiListInfo.pagingInfo.CurPageIndex > 0)
                        {
                            _imGuiListInfo.pageIndex -= 1;
                        }
                    }
                }

                Rect pageRect = new Rect(prePageRect)
                {
                    x = prePageRect.x + prePageRect.width,
                    width = inputWidth,
                };
                _imGuiListInfo.pageIndex =
                    EditorGUI.IntField(pageRect, GUIContent.none, _imGuiListInfo.pageIndex + 1) - 1;
                Rect totalPageRect = new Rect(pageRect)
                {
                    x = pageRect.x + pageRect.width,
                    width = pagingLabelWidth,
                };
                EditorGUI.LabelField(totalPageRect, $"/ {_imGuiListInfo.pagingInfo.PageCount}");
                // EditorGUI.LabelField(totalPageRect, $"/ 888");

                Rect nextPageRect = new Rect(totalPageRect)
                {
                    x = totalPageRect.x + totalPageRect.width,
                    width = buttonWidth,
                };
                using (new EditorGUI.DisabledScope(_imGuiListInfo.pagingInfo.CurPageIndex >=
                                                   _imGuiListInfo.pagingInfo.PageCount - 1))
                {
                    if (!_iconRight)
                    {
                        _iconRight = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                    }
                    if (GUI.Button(nextPageRect, _iconRight, EditorStyles.miniButtonRight))
                    {
                        if (_imGuiListInfo.pagingInfo.CurPageIndex < _imGuiListInfo.pagingInfo.PageCount - 1)
                        {
                            _imGuiListInfo.pageIndex += 1;
                        }
                    }
                }
            }
        }

        private float DrawListDrawerItemHeight(int index)
        {
            return _imGuiListInfo.pagingInfo.IndexesCurPage.Contains(index)
                ? EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index), true)
                : 0;
        }

        private void DrawListDrawerItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.height <= 0)
            {
                return;
            }

            SerializedProperty property = _imGuiListInfo.property.GetArrayElementAtIndex(index);

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
                    return property.objectReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Enum:
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
                    return property.arraySize.ToString().Contains(search);
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
