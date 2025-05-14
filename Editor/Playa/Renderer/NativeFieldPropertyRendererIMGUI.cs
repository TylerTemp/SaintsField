using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativeFieldPropertyRenderer
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
            return FieldHeight(GetValue(FieldWithInfo).value, GetNiceName(FieldWithInfo));
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            FieldPosition(position, GetValue(FieldWithInfo).value, GetNiceName(FieldWithInfo));
        }
    }
}
