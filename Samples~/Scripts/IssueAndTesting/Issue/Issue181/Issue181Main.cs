using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue182;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62;
using UnityEngine;
using MCDamageHandler = SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue270.MCDamageHandler;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue181
{
    public class Issue181Main : MonoBehaviour
    {
        [GetComponentInParents(excludeSelf: true), HideIf(nameof(_parentOverride)), Comment("will damage this parent")]
        public MCDamageHandler _parent;

        [Comment("will damage this instead of the parent")]  // commented out as I do not have this attribute in project
        public MCDamageHandler _parentOverride;
    }
}
