using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue162
{
    public class Issue162And : MonoBehaviour
    {
        [FieldInfoBox("Use HideIf(A) && HideIf(B)")]
        public Transform _receiverTransform;

        [FieldHideIf(nameof(_receiverTransform)), FieldHideIf(nameof(randomReceiver))]
        public bool worldReceiver;
        [FieldHideIf(nameof(_receiverTransform)), FieldHideIf(nameof(worldReceiver))]
        public bool randomReceiver;
    }
}
