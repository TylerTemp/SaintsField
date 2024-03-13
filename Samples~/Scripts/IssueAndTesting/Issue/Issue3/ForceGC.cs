using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue3
{
    public class ForceGC: MonoBehaviour
    {
        [BelowButton(nameof(DoGC))] public string gc;

        private void DoGC()
        {
            System.GC.Collect();
            Debug.Log("GC Collected");
        }
    }
}
