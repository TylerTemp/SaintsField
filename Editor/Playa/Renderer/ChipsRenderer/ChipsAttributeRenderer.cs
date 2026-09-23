using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer
{
    public partial class ChipsAttributeRenderer: SerializedFieldBaseRenderer
    {
        private readonly ChipsAttribute _attribute;

        public ChipsAttributeRenderer(ChipsAttribute chipsAttribute, SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _attribute = chipsAttribute;
        }

    }
}
