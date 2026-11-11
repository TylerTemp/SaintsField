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

#if UNITY_2021_3_OR_NEWER
        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif
        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }

        public override string ToString()
        {
            return $"<InfoBox {FieldWithInfo}/>";
        }
    }
}
