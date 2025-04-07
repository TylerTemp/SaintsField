using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer
{
    public partial class PlayaInfoBoxRenderer: AbsRenderer
    {
        protected readonly PlayaInfoBoxAttribute PlayaInfoBoxAttribute;

        public PlayaInfoBoxRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, PlayaInfoBoxAttribute playaInfoBoxAttribute) : base(serializedObject, fieldWithInfo)
        {
            PlayaInfoBoxAttribute = playaInfoBoxAttribute;
        }

        public override void OnDestroy()
        {
        }
    }
}
