using UnityEditor;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(UnityEditor.Editor editor, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(editor, fieldWithInfo, tryFixUIToolkit)
        {
        }
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            return UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
            // return FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }
#endif
        public override void Render()
        {
            // NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
            // FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }

    }
}
