using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class CnLayoutTitleEatChar : SaintsMonoBehaviour
    {
        [LayoutStart("中文<icon=LensFlare Icon/>名字", ELayout.FoldoutBox)]
        public int i1;
        public int i2;
    }
}
