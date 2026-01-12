using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutTabs : SaintsMonoBehaviour
    {
        [LayoutStart("Tabs", ELayout.Tab)]
        [LayoutStart("./Tab1")]
        public string t1;
        public string t2;
        public string t3;

        [LayoutStart("../Tab2")]
        public string b1;
        public string b2;
        public string b3;

        [LayoutStart("../Tab3")]
        public string o1;
        public string o2;
        public string o3;

        // Mix with title

        [LayoutStart("MixedTabs", ELayout.Tab | ELayout.TitleBox | ELayout.Foldout)]
        [LayoutStart("./Tab1")]
        public string mt1;
        public string mt2;
        public string mt3;

        [LayoutStart("../Tab2")]
        public string mb1;
        public string mb2;
        public string mb3;

        [LayoutStart("../Tab3")]
        public string mo1;
        public string mo2;
        public string mo3;
    }
}
