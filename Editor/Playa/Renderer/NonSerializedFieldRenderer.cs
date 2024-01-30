using UnityEditor;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NonSerializedFieldRenderer: AbsRenderer
    {
        public NonSerializedFieldRenderer(UnityEditor.Editor editor, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(editor, fieldWithInfo, tryFixUIToolkit)
        {
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object value = FieldWithInfo.FieldInfo.GetValue(SerializedObject.targetObject);
            return UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .FieldInfo.Name));
        }
#endif
        public override void Render()
        {
            object value = FieldWithInfo.FieldInfo.GetValue(SerializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .FieldInfo.Name));
        }

    }
}
