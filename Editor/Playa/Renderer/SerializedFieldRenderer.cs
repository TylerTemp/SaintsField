using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Playa;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo)
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

            PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
                userData = userDataPayload,
            };

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
