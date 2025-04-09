using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class NoContLayout : SaintsMonoBehaviour
    {
        [Layout("1", ELayout.TitleBox)] public string l1;

        public string lOut;

        [Layout("1")] public string l2;

        [LayoutStart("Start", ELayout.TitleBox)]
        public string s1;
        public string s2;

        [LayoutStart("Foldout", ELayout.FoldoutBox)]
        public string foldoutItem1;

        // tabs
        [Layout("Tabs", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Tab1")]
        public string tab1Item1;
        public int tab1Item2;

        [LayoutStart("../Tab2")]
        public string tab2Item1;
        public int tab2Item2;

        [LayoutStart("../Tab3")]
        public string tab3Item1;
        public int tab3Item2;

        // nested groups
        [LayoutStart("Nested", ELayout.Background | ELayout.TitleOut)]
        public int nestedOne;

    }
}
