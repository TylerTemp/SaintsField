using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue126 : MonoBehaviour
    {
        [InfoBox("AutoUpdate off", show: nameof(autoUpdate))]
        public bool autoUpdate;
    }
}
