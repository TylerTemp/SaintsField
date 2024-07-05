namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4
{
    public class DisabledIfPlayAttribute: ReadOnlyAttribute
    {
        public DisabledIfPlayAttribute(EMode editorMode, params object[] by) : base(editorMode, by)
        {
        }

        public DisabledIfPlayAttribute(params object[] by) : base(by)
        {
        }
    }
}
