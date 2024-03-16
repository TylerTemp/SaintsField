using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Playa;
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

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

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
            if (FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute || each is PlayaHideIfAttribute || each is PlayaEnableIfAttribute ||
                                                            each is PlayaDisableIfAttribute) > 0)
            {
                result.RegisterCallback<AttachToPanelEvent>(_ => result.schedule.Execute(() => UIToolkitOnUpdate(result, true)).Every(100));
            }

            return result;
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
                EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, GUILayout.ExpandWidth(true));
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
                EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, true);
            }
            // EditorGUI.DrawRect(position, Color.blue);
        }

        public override string ToString() => $"Ser<{FieldWithInfo.FieldInfo?.Name ?? FieldWithInfo.SerializedProperty.displayName}>";
    }
}
