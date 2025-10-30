using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue162
{
    public class Issue162Or : MonoBehaviour
    {
        [FieldInfoBox("Use HideIf(A || B)")]
        public Transform _receiverTransform;

        [FieldHideIf(nameof(_receiverTransform), nameof(randomReceiver))]
        public bool worldReceiver;
        [FieldHideIf(nameof(_receiverTransform), nameof(worldReceiver))]
        public bool randomReceiver;
    }
}
