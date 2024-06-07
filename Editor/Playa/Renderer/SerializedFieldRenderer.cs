using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Editor.Utils;
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
                    userData = userDataPayload,
                }
                : MakeListDrawerSettingsField(listDrawerSettingsAttribute);

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
            bool arraySizeCondition = FieldWithInfo.PlayaAttributes.Any(each => each is PlayaArraySizeAttribute);
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

        // private const string ListDrawerItemClass = "saintsfield-list-drawer-item";
        private VisualElement MakeListDrawerSettingsField(ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            SerializedProperty property = FieldWithInfo.SerializedProperty;

            int numberOfItemsPerPage = 0;
            int curPageIndex = 0;

            VisualElement MakeItem()
            {
                PropertyField propertyField = new PropertyField();
                return propertyField;
            }

            void BindItem(VisualElement propertyFieldRaw, int index)
            {
                SerializedProperty prop = property.GetArrayElementAtIndex(index + curPageIndex * numberOfItemsPerPage);
                PropertyField propertyField = (PropertyField)propertyFieldRaw;
                propertyField.BindProperty(prop);
                propertyField.userData = index;
            }

            ListView listView = new ListView(Enumerable.Range(0, property.arraySize).ToList())
            {
                makeItem = MakeItem,
                bindItem = BindItem,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
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
                    display = DisplayStyle.None,
                },
            };

            #region Search

            ToolbarSearchField searchField = new ToolbarSearchField
            {
                style =
                {
                    visibility = Visibility.Hidden,
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
                    visibility = Visibility.Hidden,
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
            numberOfItemsPerPageField.Q<TextElement>().style.unityTextAlign = TextAnchor.MiddleRight;
            Label numberOfItemsPerPageLabel = new Label($" / {property.arraySize} Items")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };

            Button pagePreButton = new Button
            {
                text = "<",
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
            pageField.Q<TextElement>().style.unityTextAlign = TextAnchor.MiddleRight;
            Label pageLabel = new Label(" / 1")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };
            Button pageNextButton = new Button
            {
                text = ">",
            };

            void UpdatePage(int newPageIndex)
            {
                int pageCount;
                int skipStart;
                int itemCount;
                if (numberOfItemsPerPage <= 0)
                {
                    pageCount = 1;
                    curPageIndex = 0;
                    skipStart = 0;
                    itemCount = property.arraySize;
                }
                else
                {
                    pageCount = Mathf.CeilToInt((float)property.arraySize / numberOfItemsPerPage);
                    curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                    skipStart = curPageIndex * numberOfItemsPerPage;
                    itemCount = Mathf.Min(numberOfItemsPerPage, property.arraySize - skipStart);
                }

                pageLabel.text = $" / {pageCount}";
                pageField.SetValueWithoutNotify(curPageIndex + 1);

                List<int> curPageItems = Enumerable.Range(skipStart, itemCount).ToList();

                listView.itemsSource = curPageItems;
                listView.Rebuild();
            }

            pagePreButton.clicked += () =>
            {
                if(curPageIndex > 0)
                {
                    UpdatePage(curPageIndex - 1);
                }
            };
            pageNextButton.clicked += () =>
            {
                if(curPageIndex < Mathf.CeilToInt((float)property.arraySize / numberOfItemsPerPage) - 1)
                {
                    UpdatePage(curPageIndex + 1);
                }
            };
            pageField.RegisterValueChangedCallback(evt => UpdatePage(evt.newValue - 1));

            void UpdateNumberOfItemsPerPage(int newNumberOfItemsPerPage)
            {
                int newValue = Mathf.Clamp(newNumberOfItemsPerPage, 0, property.arraySize);
                if(numberOfItemsPerPage != newValue)
                {
                    numberOfItemsPerPage = newValue;
                    UpdatePage(curPageIndex);
                }
            }

            numberOfItemsPerPageField.RegisterValueChangedCallback(evt => UpdateNumberOfItemsPerPage(evt.newValue));

            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(() =>
            {
                property.arraySize += 1;
                property.serializedObject.ApplyModifiedProperties();
                int totalPage = Mathf.CeilToInt((float)property.arraySize / numberOfItemsPerPage);
                UpdatePage(totalPage - 1);
                numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";
            });

            listView.itemsRemoved += objects =>
            {
                int[] sources = listView.itemsSource.Cast<int>().ToArray();

                foreach (int index in objects.Select(removeIndex => sources[removeIndex]).OrderByDescending(each => each))
                {
                    // Debug.Log(index);
                    property.DeleteArrayElementAtIndex(index);
                }
                property.serializedObject.ApplyModifiedProperties();
                numberOfItemsPerPageLabel.text = $" / {property.arraySize} Items";

                UpdatePage(curPageIndex);
            };

            if (listDrawerSettingsAttribute.NumberOfItemsPerPage != 0)
            {
                preContent.style.display = DisplayStyle.Flex;
                pagingContainer.style.visibility = Visibility.Visible;

                listView.RegisterCallback<AttachToPanelEvent>(_ =>
                {
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
                    FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
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

        private RichTextDrawer _richTextDrawer;

        private string _curXml = null;
        private RichTextDrawer.RichTextChunk[] _curXmlChunks;

        public override void Render()
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using(new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                GUIContent useGUIContent = preCheckResult.HasRichLabel
                    ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length))
                    : new GUIContent(FieldWithInfo.SerializedProperty.displayName);

                EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, useGUIContent, GUILayout.ExpandWidth(true));

                if (preCheckResult.HasRichLabel
                    // && Event.current.type == EventType.Repaint
                   )
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

                if (preCheckResult.ArraySize != -1 && FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
                {
                    FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
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
            return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
        }

        public override void RenderPosition(Rect position)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                GUIContent useGUIContent = preCheckResult.HasRichLabel
                    ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length))
                    : new GUIContent(FieldWithInfo.SerializedProperty.displayName);

                EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, useGUIContent, true);

                if (preCheckResult.HasRichLabel
                    // && Event.current.type == EventType.Repaint
                   )
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

                if (preCheckResult.ArraySize != -1 && FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
                {
                    FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
                }
            }
            // EditorGUI.DrawRect(position, Color.blue);
        }

        public override string ToString() => $"Ser<{FieldWithInfo.FieldInfo?.Name ?? FieldWithInfo.SerializedProperty.displayName}>";
    }
}
