using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue181 : MonoBehaviour
    {
        [GetComponentInParents(excludeSelf: true), HideIf(nameof(_parentOverride))]
        public MCDamageHandler _parent;
        // [Comment("will damage this instead of the parent")]
        public MCDamageHandler _parentOverride;
    }
}
