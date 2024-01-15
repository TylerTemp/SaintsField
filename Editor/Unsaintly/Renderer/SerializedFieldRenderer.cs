using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Unsaintly.Renderer
{
    public class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(UnityEditor.Editor editor, UnsaintlyFieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
        {
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement Render()
        {
            return new PropertyField(serializedObject.FindProperty(fieldWithInfo.fieldInfo.Name));
        }

#else
        public override void Render()
        {
            SerializedProperty property = serializedObject.FindProperty(fieldWithInfo.fieldInfo.Name);
            EditorGUILayout.PropertyField(property);
        }
#endif
    }
}
