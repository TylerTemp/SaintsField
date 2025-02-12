using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NonSerializedFieldRenderer
    {
        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!_renderField)
            {
                return;
            }

            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name));
        }
    }
}
