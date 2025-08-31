using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer
{
    public partial class PlayaInfoBoxRenderer: AbsRenderer
    {
        private readonly PlayaInfoBoxAttribute _playaInfoBoxAttribute;

        public PlayaInfoBoxRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, PlayaInfoBoxAttribute playaInfoBoxAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaInfoBoxAttribute = playaInfoBoxAttribute;
        }

        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }

        public override string ToString()
        {
            return $"<InfoBox {FieldWithInfo}/>";
        }
    }
}
