using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativePropertyRenderer
    {
        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            float height = GetFieldHeightIMGUI(width, preCheckResult);
            if (height < Mathf.Epsilon)
            {
                return;
            }

            Rect position = EditorGUILayout.GetControlRect(false, height);
            RenderPositionTargetIMGUI(position, preCheckResult);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!RenderField)
            {
                return 0f;
            }
            return FieldHeight(FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target), ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name));
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }
    }
}
