using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue366NestedFoldoutBox : SaintsMonoBehaviour
    {
        [LayoutStart("Main", ELayout.FoldoutBox)]
        [LayoutStart("./Sub", ELayout.FoldoutBox)]
        public string sub1;
    }
}
