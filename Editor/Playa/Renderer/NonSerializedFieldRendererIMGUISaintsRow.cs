using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NonSerializedFieldRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!_renderField)
            {
                return 0;
            }

            return FieldHeight(FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target),
                ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name));
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!_renderField)
            {
                return;
            }

            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .FieldInfo.Name));
        }
    }
}
