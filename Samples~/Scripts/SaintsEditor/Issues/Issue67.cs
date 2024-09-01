using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue67 : SaintsMonoBehaviour
    {
        [LayoutStart("Root", ELayout.FoldoutBox)]
        public string root1;
        public string root2;

        [LayoutStart("./Sub", ELayout.FoldoutBox)]  // equals "Root/Sub"
        public string sub1;
        public string sub2;
        [LayoutEnd(".")]

        [LayoutStart("./Another", ELayout.FoldoutBox)]  // equals "Root/Another"
        public string another1;
        public string another2;
        [LayoutEnd(".")]  // equals "Root/Another"

        public string root3;  // this should still belong to "Root"
        public string root4;

        [LayoutEnd]  // this should close any existing group
        public string outOfAll;

        [LayoutStart("Tabs", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Tab1")]
        public string tab1Item1;
        public int tab1Item2;
        [LayoutEnd(".")]

        [LayoutStart("./Tab2")]
        public string tab2Item1;
        public int tab2Item2;

        [Serializable]
        public struct MyStruct
        {
            [LayoutStart("Root", ELayout.FoldoutBox)]
            public string root1;
            public string root2;

            [LayoutStart("./Sub", ELayout.FoldoutBox)]  // equals "Root/Sub"
            public string sub1;
            public string sub2;
            [LayoutEnd(".")]

            [LayoutStart("./Another", ELayout.FoldoutBox)]  // equals "Root/Another"
            public string another1;
            public string another2;
            [LayoutEnd(".")]  // equals "Root/Another"

            public string root3;  // this should still belong to "Root"
            public string root4;

            [LayoutEnd]  // this should close any existing group
            public string outOfAll;

            [LayoutStart("Tabs", ELayout.Tab | ELayout.Collapse)]
            [LayoutStart("./Tab1")]
            public string tab1Item1;
            public int tab1Item2;
            [LayoutEnd(".")]

            [LayoutStart("./Tab2")]
            public string tab2Item1;
            public int tab2Item2;
        }

        [LayoutEnd]
        [SaintsRow] public MyStruct myStruct;
    }
}
