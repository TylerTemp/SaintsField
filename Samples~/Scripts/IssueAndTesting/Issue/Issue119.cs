using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue119 : MonoBehaviour
    {
        public bool omniscient;

        [Range(1, 40), FieldHideIf(nameof(omniscient)), FieldInfoBox("This won't work: Unity won't fallback")] public float detectionRange;
        [FieldHideIf(nameof(omniscient)), Range(1, 40), FieldInfoBox("This is fallback is broken in 3.18.1...")] public float f;
        [FieldHideIf(nameof(omniscient)), FieldInfoBox("This is fine: SaintsField can fallback"), Range(1, 40), ] public float fieldOfView;
    }
}
