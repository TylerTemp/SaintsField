using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativePropertyRenderer
    {
        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }
    }
}
