using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutHorizontal : SaintsMonoBehaviour
    {
        [LayoutStart("Horizontal", ELayout.Horizontal)]
        public string t1;
        public string t2;
        public string t3;

        [LayoutStart("HorizontalBox", ELayout.Horizontal | ELayout.TitleBox)]
        public string b1;
        public string b2;
        public string b3;

        [LayoutStart("HorizontalFoldout", ELayout.Horizontal | ELayout.FoldoutBox)]
        public string o1;
        public string o2;
        public string o3;
    }
}
