using System.Linq;
using UnityEditor;
using UnityEngine;
using SaintsField.Playa;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo, tryFixUIToolkit)
        {
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        private PropertyField _result;

        public override VisualElement CreateVisualElement()
        {
            PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            // ReSharper disable once InvertIf
            if(TryFixUIToolkit && FieldWithInfo.FieldInfo?.GetCustomAttributes(typeof(ISaintsAttribute), true).Length == 0)
            {
                // Debug.Log($"{fieldWithInfo.fieldInfo.Name} {arr.Length}");
                _result = result;
                _result.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            }

            // disable/enable/show/hide
            bool ifCondition = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute
                                                                           // ReSharper disable once MergeIntoLogicalPattern
                                                                           || each is PlayaEnableIfAttribute
                                                                           // ReSharper disable once MergeIntoLogicalPattern
                                                                           || each is PlayaDisableIfAttribute) > 0;
            bool arraySizeCondition = FieldWithInfo.PlayaAttributes.Any(each => each is PlayaArraySizeAttribute);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log(
                $"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; if={ifCondition}; arraySize={arraySizeCondition}");
#endif
            if (ifCondition || arraySizeCondition)
            {
                result.RegisterCallback<AttachToPanelEvent>(_ =>
                    result.schedule
                        .Execute(() => UIToolkitCheckUpdate(result, ifCondition, arraySizeCondition))
                        .Every(100)
                );
            }

            return result;
        }

        private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition)
        {
            PreCheckResult preCheckResult = default;
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (ifCondition)
            {
                preCheckResult = UIToolkitOnUpdate(FieldWithInfo, result, true);
            }

            if(!ifCondition && arraySizeCondition)
            {
                preCheckResult = GetPreCheckResult(FieldWithInfo);
            }

            if (!arraySizeCondition)
            {
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log(
                $"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; preCheckResult.ArraySize={preCheckResult.ArraySize}, curSize={FieldWithInfo.SerializedProperty.arraySize}");
#endif
            if (preCheckResult.ArraySize == -1 ||
                FieldWithInfo.SerializedProperty.arraySize == preCheckResult.ArraySize)
            {
                return;
            }

            FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();

        }

        private void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            // Debug.Log("OnGeometryChangedEvent");
            Label label = _result.Q<Label>(className: "unity-label");
            if (label == null)
            {
                return;
            }

            // Utils.Util.FixLabelWidthLoopUIToolkit(label);
            _result.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            Utils.UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
            _result = null;
        }

#endif
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

                EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, GUILayout.ExpandWidth(true));

                if (preCheckResult.ArraySize != -1 && FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
                {
                    FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
                }
            }
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

                EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, true);

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
