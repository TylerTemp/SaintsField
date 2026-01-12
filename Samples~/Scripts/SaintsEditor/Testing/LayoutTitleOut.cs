using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutTitleOut : SaintsMonoBehaviour
    {
        [LayoutStart("Titled", ELayout.Title)]
        public string t1;
        public string t2;
        public string t3;

        [LayoutStart("TitledBox", ELayout.TitleBox)]
        public string b1;
        public string b2;
        public string b3;

        [LayoutStart("TitledOut", ELayout.TitleOut)]
        public string o1;
        public string o2;
        public string o3;
    }
}
