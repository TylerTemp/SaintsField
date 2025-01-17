using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue119 : MonoBehaviour
    {
        public bool omniscient;

        // [Range(1, 40), HideIf(nameof(omniscient)), InfoBox("This won't work: Unity won't fallback")] public float detectionRange;
        [HideIf(nameof(omniscient)), Range(1, 40), InfoBox("This is fine: SaintsField can fallback")] public float fieldOfView;
    }
}
