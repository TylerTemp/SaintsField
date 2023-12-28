using UnityEditor;

namespace SaintsField.Editor.Saintless.Renderer
{
    public class NonSerializedFieldRenderer: AbsRenderer
    {
        public NonSerializedFieldRenderer(UnityEditor.Editor editor, SaintlessFieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
        {
        }

        public override void Render()
        {
            object value = fieldWithInfo.fieldInfo.GetValue(serializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo
                .fieldInfo.Name));
        }
    }
}
