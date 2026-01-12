using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutFoldout : SaintsMonoBehaviour
    {
        [LayoutStart("Grouped", ELayout.Foldout)]
        public string t1;
        public string t2;
        public string t3;

        [LayoutStart("GroupedBox", ELayout.Foldout |  ELayout.TitleBox)]
        public string b1;
        public string b2;
        public string b3;

        [LayoutStart("GroupedOut", ELayout.Foldout | ELayout.TitleOut)]
        public string o1;
        public string o2;
        public string o3;
    }
}
