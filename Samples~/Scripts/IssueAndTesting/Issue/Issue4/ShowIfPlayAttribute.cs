using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4
{
    public class ShowIfPlayAttribute: PropertyAttribute, ISaintsAttribute, IImGuiVisibilityAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";
    }
}
