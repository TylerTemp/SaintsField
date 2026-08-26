using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer.HeaderRenderer
{
    public partial class HeaderAttributeRenderer: AbsRenderer
    {
        private readonly HeaderAttribute _headerAttribute;

        public HeaderAttributeRenderer(HeaderAttribute headerAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _headerAttribute = headerAttribute;
        }

        protected override bool AllowGuiColor => true;

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
