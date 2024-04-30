namespace SaintsField.Editor.Playa.RendererGroup
{
    public interface ISaintsRendererGroup: ISaintsRenderer
    {
        void Add(string groupPath, ISaintsRenderer renderer);
    }
}
