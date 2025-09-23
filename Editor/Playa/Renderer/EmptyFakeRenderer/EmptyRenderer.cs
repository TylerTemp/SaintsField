using SaintsField.Editor.Playa.Renderer.BaseRenderer;

namespace SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer
{
    public partial class EmptyRenderer: AbsRenderer
    {
        public EmptyRenderer() : base(null, default)
        {
        }

        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }
    }
}
