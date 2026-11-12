using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer
{
    public partial class PlayaInfoBoxRenderer: AbsRenderer
    {
        protected override bool AllowGuiColor => true;

        private readonly InfoBoxAttribute _playaInfoBoxAttribute;

        public PlayaInfoBoxRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, InfoBoxAttribute playaInfoBoxAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaInfoBoxAttribute = playaInfoBoxAttribute;
        }

        public override bool OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            return OnSearchFieldUIToolkit(searchString);
#endif

#pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return false;
#pragma warning restore CS0162 // Unreachable code detected
        }

        public override string ToString()
        {
            return $"<InfoBox {FieldWithInfo}/>";
        }
    }
}
