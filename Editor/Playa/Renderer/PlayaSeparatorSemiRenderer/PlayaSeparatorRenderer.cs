using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer: AbsRenderer
    {
        private readonly PlayaSeparatorAttribute _playaSeparatorAttribute;

        public PlayaSeparatorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, PlayaSeparatorAttribute playaSeparatorAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaSeparatorAttribute = playaSeparatorAttribute;
        }

        public override void OnDestroy()
        {
        }
    }
}
