using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue162
{
    public class Issue162And : MonoBehaviour
    {
        [InfoBox("Use HideIf(A) && HideIf(B)")]
        public Transform _receiverTransform;

        [HideIf(nameof(_receiverTransform)), HideIf(nameof(randomReceiver))]
        public bool worldReceiver;
        [HideIf(nameof(_receiverTransform)), HideIf(nameof(worldReceiver))]
        public bool randomReceiver;
    }
}
