using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer.SpaceRenderer
{
    public partial class SpaceAttributeRenderer: AbsRenderer
    {
        private readonly SpaceAttribute _spaceAttribute;

        public SpaceAttributeRenderer(SpaceAttribute spaceAttribute, SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _spaceAttribute = spaceAttribute;
        }

        protected override bool AllowGuiColor => false;

#if UNITY_2021_3_OR_NEWER
        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif

        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }
    }
}
