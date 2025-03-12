using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue161
{
    public class Issue161Prefab : MonoBehaviour
    {
        [GetComponent, BelowButton(nameof(DebugShow), "r")] public Issue161Sign sign;

        private void DebugShow(object v) => Debug.Log(v);
    }
}
