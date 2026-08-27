using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue438ButtonSep : SaintsMonoBehaviour
    {
        public int normalField;

        [Separator(100)]
        [Button]
        private void B() {}
    }
}
