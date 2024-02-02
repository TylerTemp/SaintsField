using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public interface ISaintsRendererGroup: ISaintsRenderer
    {
        void Add(string groupPath, ISaintsRenderer renderer);
    }
}
