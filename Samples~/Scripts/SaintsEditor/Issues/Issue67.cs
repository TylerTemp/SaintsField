using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue67 : SaintsMonoBehaviour
    {
        [LayoutGroup("Root", ELayout.FoldoutBox)]
        public string root1;
        public string root2;

        [LayoutGroup("./Sub", ELayout.FoldoutBox)]  // equals "Root/Sub"
        public string sub1;
        public string sub2;

        [LayoutGroup("../Another", ELayout.FoldoutBox)]  // equals "Root/Sub"
        public string another1;
        public string another2;

        [LayoutEnd(".")]  // equals "Root/Sub"
        public string root3;  // this should still belong to "Root"
        public string root4;

        [LayoutEnd]  // this should close any existing group
        public string outOfAll;
    }
}
