using SaintsField.Editor.Playa.Renderer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer : AbsRenderer
    {
        private readonly SaintsFieldWithInfo _fieldWithInfo;
        public WindowInlineEditorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo): base(serializedObject, fieldWithInfo)
        {
            _fieldWithInfo = fieldWithInfo;
        }

        public override void OnDestroy()
        {

        }

        private Object GetValue()
        {
            object v = _fieldWithInfo.FieldInfo != null
                ? _fieldWithInfo.FieldInfo.GetValue(_fieldWithInfo.Target)
                : _fieldWithInfo.PropertyInfo.GetValue(_fieldWithInfo.Target);
            return v as Object;
        }
    }
}
