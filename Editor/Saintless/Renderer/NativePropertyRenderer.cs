using UnityEditor;

namespace SaintsField.Editor.Saintless.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(UnityEditor.Editor editor, SaintlessFieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
        {
        }

        public override void Render()
        {
            // NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
            object value = fieldWithInfo.propertyInfo.GetValue(serializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo
                .propertyInfo.Name));
            // FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }
    }
}
