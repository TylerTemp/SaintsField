using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer
{
    public partial class ChipsAttributeRenderer: AbsRenderer
    {
        private readonly ChipsAttribute _attribute;

        public ChipsAttributeRenderer(ChipsAttribute chipsAttribute, SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _attribute = chipsAttribute;
        }

        public override void OnSearchField(string searchString)
        {
        }
    }
}
