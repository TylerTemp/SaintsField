using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer : AbsRenderer
    {
        private readonly SaintsFieldWithInfo _fieldWithInfo;
        public WindowInlineEditorRenderer(SaintsFieldWithInfo fieldWithInfo): base(fieldWithInfo)
        {
            _fieldWithInfo = fieldWithInfo;
        }

        public override void OnDestroy()
        {

        }



        private UnityEngine.Object GetValue()
        {
            object v = _fieldWithInfo.FieldInfo != null
                ? _fieldWithInfo.FieldInfo.GetValue(_fieldWithInfo.Target)
                : _fieldWithInfo.PropertyInfo.GetValue(_fieldWithInfo.Target);
            return v as Object;
        }
    }
}
