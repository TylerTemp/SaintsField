using System;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer : AbsRenderer
    {
        private readonly SaintsFieldWithInfo _fieldWithInfo;
        private readonly Type _editorType;
        public WindowInlineEditorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, Type editorType): base(serializedObject, fieldWithInfo)
        {
            _fieldWithInfo = fieldWithInfo;
            _editorType = editorType;
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
