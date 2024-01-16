using UnityEditor;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Unsaintly.Renderer
{
    public class NonSerializedFieldRenderer: AbsRenderer
    {
        public NonSerializedFieldRenderer(UnityEditor.Editor editor, UnsaintlyFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(editor, fieldWithInfo, tryFixUIToolkit)
        {
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement Render()
        {
            object value = fieldWithInfo.fieldInfo.GetValue(serializedObject.targetObject);
            return UIToolkitLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo
                .fieldInfo.Name));
        }
#else
        public override void Render()
        {
            object value = fieldWithInfo.fieldInfo.GetValue(serializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo
                .fieldInfo.Name));
        }
#endif
    }
}
