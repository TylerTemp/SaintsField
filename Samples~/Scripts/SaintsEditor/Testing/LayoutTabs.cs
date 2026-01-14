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

        [LayoutStart("../<icon=star.png/>Tab2")]
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

        // Colors + Icons

        [LayoutStart("Color Tab", ELayout.Tab)]

        [LayoutStart("./<color=#FCBF07><icon=d_AudioClip Icon/>Music")]
        public string m1;
        public string m2;
        public string m3;

        [LayoutStart("../<color=#34F42B><icon=greenLight/>Light")]
        public string l1;
        public string l2;
        public string l3;

        [LayoutStart("../<color=#B0FC58><icon=d_Cloth Icon/>Skin")]
        public string skin1;
        public string skin2;
        public string skin3;

        [LayoutStart("../<color=Aquamarine><icon=d_Settings Icon/>Settings")]
        public string s1;
        public string s2;
        public string s3;

        [LayoutStart("../<color=Bisque><icon=d_UnityEditor.GameView/>Controller")]
        public string g1;
        public string g2;
        public string g3;

        [LayoutStart("../<color=CadetBlue><icon=star.png/>Favorite")]
        public string f1;
        public string f2;
        public string f3;

        [LayoutStart("../<color=Chartreuse><icon=AudioSource Gizmo/>Audio")]
        public string a1;
        public string a2;
        public string a3;
    }
}
