using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer: AbsRenderer
    {
        protected override bool AllowGuiColor => true;

        private readonly SeparatorAttribute _playaSeparatorAttribute;
        private readonly string _colorHex;

        public PlayaSeparatorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, SeparatorAttribute playaSeparatorAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaSeparatorAttribute = playaSeparatorAttribute;
            _colorHex = ColorUtility.ToHtmlStringRGB(_playaSeparatorAttribute.Color);
        }

        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }
    }
}
