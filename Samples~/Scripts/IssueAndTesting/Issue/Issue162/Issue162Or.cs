using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue162
{
    public class Issue162Or : MonoBehaviour
    {
        [InfoBox("Use HideIf(A || B)")]
        public Transform _receiverTransform;

        [HideIf(nameof(_receiverTransform), nameof(randomReceiver))]
        public bool worldReceiver;
        [HideIf(nameof(_receiverTransform), nameof(worldReceiver))]
        public bool randomReceiver;
    }
}
