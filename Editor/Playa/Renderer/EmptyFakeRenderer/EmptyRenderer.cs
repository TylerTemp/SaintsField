using SaintsField.Editor.Playa.Renderer.BaseRenderer;

namespace SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer
{
    public partial class EmptyRenderer: AbsRenderer
    {
        public EmptyRenderer() : base(null, default)
        {
        }

        protected override bool AllowGuiColor => false;

        public override bool OnSearchField(string searchString)
        {
            return false;
        }
    }
}
