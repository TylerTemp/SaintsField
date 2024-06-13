using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using SaintsField.Editor.Linq;
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
            List<int> itemIndexToPropertyIndex = Enumerable.Range(0, property.arraySize).ToList();

            VisualElement MakeItem()
            {
                PropertyField propertyField = new PropertyField();
                return propertyField;
            }

            void BindItem(VisualElement propertyFieldRaw, int index)
            {
                // Debug.Log(($"bind: {index}, itemIndex={string.Join(", ", itemIndexToPropertyIndex)}"));
                SerializedProperty prop = property.GetArrayElementAtIndex(itemIndexToPropertyIndex[index]);
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
                    // visibility = Visibility.Hidden,
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
                string searchTarget = searchField.value;
                IReadOnlyList<int> fullIndexResults = string.IsNullOrEmpty(searchTarget)
                    ? Enumerable.Range(0, property.arraySize).ToList()
                    : SearchArrayProperty(property, searchTarget).ToList();

                Debug.Log($"index search={searchTarget} result: {string.Join(",", fullIndexResults)}");

                itemIndexToPropertyIndex.Clear();
                itemIndexToPropertyIndex.AddRange(fullIndexResults);

                int pageCount;
                int skipStart;
                int itemCount;
                if (numberOfItemsPerPage <= 0)
                {
                    pageCount = 1;
                    curPageIndex = 0;
                    skipStart = 0;
                    itemCount = itemIndexToPropertyIndex.Count;
                }
                else
                {
                    pageCount = Mathf.CeilToInt((float)itemIndexToPropertyIndex.Count / numberOfItemsPerPage);
                    curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                    skipStart = curPageIndex * numberOfItemsPerPage;
                    itemCount = Mathf.Min(numberOfItemsPerPage, itemIndexToPropertyIndex.Count - skipStart);
                }

                pageLabel.text = $" / {pageCount}";
                pageField.SetValueWithoutNotify(curPageIndex + 1);

                List<int> curPageItems = fullIndexResults.Skip(skipStart).Take(itemCount).ToList();

                listView.itemsSource = curPageItems;
                listView.Rebuild();
            }

            searchField.RegisterValueChangedCallback(evt =>
            {
                UpdatePage(0);
            });

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

        private static IEnumerable<int> SearchArrayProperty(SerializedProperty property, string search)
        {
            foreach (int index in Enumerable.Range(0, property.arraySize))
            {
                SerializedProperty childProperty = property.GetArrayElementAtIndex(index);
                if(SearchProp(childProperty, search))
                {
                    Debug.Log($"found: {childProperty.propertyPath}");
                    yield return index;
                }
            }
        }

        private static bool SearchProp(SerializedProperty property, string search)
        {
            // bool hasChildProp = false;
            // foreach (SerializedProperty child in GetPropertyChildren(property))
            // {
            //     hasChildProp = true;
            //     if(SearchProp(child, search))
            //     {
            //         return true;
            //     }
            // }
            //
            // if (hasChildProp)
            // {
            //     return false;
            // }

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
                    return property.floatValue.ToString().Contains(search);
                case SerializedPropertyType.String:
                    Debug.Log($"{property.propertyPath}={property.stringValue} contains {search}={property.stringValue?.Contains(search)}");
                    return property.stringValue?.Contains(search) ?? false;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString().Contains(search);
                case SerializedPropertyType.ObjectReference:
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
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename.Contains(search);
                case SerializedPropertyType.Generic:
                {
                    if (property.isArray)
                    {
                        Debug.Log($"is array {property.arraySize}: {property.propertyPath}");
                        return Enumerable.Range(0, property.arraySize)
                            .Select(property.GetArrayElementAtIndex)
                            .Any(childProperty => SearchProp(childProperty, search));
                    }

                    foreach (SerializedProperty child in GetPropertyChildren(property))
                    {
                        if(SearchProp(child, search))
                        {
                            Debug.Log($"found child: {child.propertyPath}");
                            return true;
                        }
                    }

                    return false;
                }
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Hash128:
                default:
                    return false;
            }
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

        public static IEnumerable<SerializedProperty> GetPropertyChildren(SerializedProperty property)
        {
            if (property != null && string.IsNullOrEmpty(property.propertyPath))
            {
                yield break;
            }

            using (SerializedProperty iterator = property.Copy())
            {
                if (!iterator.NextVisible(true))
                {
                    yield break;
                }

                do
                {
                    SerializedProperty childProperty = property.FindPropertyRelative(iterator.name);
                    yield return childProperty;
                } while (iterator.NextVisible(false));
            }
            // var enumerator = property.GetEnumerator();
            // while (enumerator.MoveNext()) {
            //     var prop = enumerator.Current as SerializedProperty;
            //     if (prop == null) continue;
            //     //Add your treatment to the current child property...
            //     // EditorGUILayout.PropertyField(prop);
            //     yield return prop;
            // }

            // SerializedProperty iterator = property.Copy();
            // // MyObject myObj = ScriptableObject.CreateInstance<MyObject>();
            // // SerializedObject mySerializedObject = new UnityEditor.SerializedObject(myObj);
            // // SerializedProperty iterator = mySerializedObject.FindProperty("PropertyName");
            // while (iterator.Next(true))
            // {
            //     yield return iterator.Copy();
            //     // Debug.Log(it.name);
            // }

            // Debug.Log(property.propertyPath);
            // property = property.Copy();
            // SerializedProperty nextElement = property.Copy();
            // bool hasNextElement = nextElement.NextVisible(false);
            // if (!hasNextElement)
            // {
            //     nextElement = null;
            // }
            //
            // property.NextVisible(true);
            // while (true)
            // {
            //     if ((SerializedProperty.EqualContents(property, nextElement)))
            //     {
            //         yield break;
            //     }
            //
            //     yield return property;
            //
            //     bool hasNext = property.NextVisible(false);
            //     if (!hasNext)
            //     {
            //         break;
            //     }
            // }
        }

        public override string ToString() => $"Ser<{FieldWithInfo.FieldInfo?.Name ?? FieldWithInfo.SerializedProperty.displayName}>";
    }
}
