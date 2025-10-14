using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue126 : MonoBehaviour
    {
        [FieldInfoBox("AutoUpdate off", show: nameof(autoUpdate))]
        public bool autoUpdate;
    }
}
