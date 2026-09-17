using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorInitAttributeRenderer
{
    public partial class OnInspectorInitRenderer: AbsRenderer
    {
        public OnInspectorInitRenderer(OnInspectorInitAttribute onInspectorInitAttribute,
            SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        public override void OnSearchField(string searchString)
        {
        }
    }
}
