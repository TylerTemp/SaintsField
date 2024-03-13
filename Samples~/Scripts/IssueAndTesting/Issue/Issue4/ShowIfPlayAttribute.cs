using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4
{
    public class ShowIfPlayAttribute: VisibilityAttribute
    {
        public ShowIfPlayAttribute(params string[] andCallbacks) : base(false, andCallbacks)
        {
        }
    }
}
