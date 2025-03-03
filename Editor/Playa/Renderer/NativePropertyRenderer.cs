using SaintsField.Playa;
using UnityEditor;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativePropertyRenderer: AbsRenderer
    {
        protected bool RenderField;

        public NativePropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            RenderField = fieldWithInfo.PlayaAttributes.Any(each => each is ShowInInspectorAttribute);
        }
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        // private bool _callUpdate;

#endif
        public override void OnDestroy()
        {
        }
    }
}
