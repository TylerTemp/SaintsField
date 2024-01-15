using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Unsaintly.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(UnityEditor.Editor editor, UnsaintlyFieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
        {
        }
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement Render()
        {
            object value = fieldWithInfo.propertyInfo.GetValue(serializedObject.targetObject);
            return UIToolkitLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo
                .propertyInfo.Name));
            // return FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }
#else
        public override void Render()
        {
            // NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
            object value = fieldWithInfo.propertyInfo.GetValue(serializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo
                .propertyInfo.Name));
            // FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }
#endif
    }
}
