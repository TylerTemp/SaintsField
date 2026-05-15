namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    public class Issue389Test : SaintsMonoBehaviour
    {
        [DefaultExpand]
        public NotMatchedContainer notMatched;
        [DefaultExpand]
        public BuildObject matched;

        [DefaultExpand] public NoSuchFunction noSuchFunction;
    }
}
