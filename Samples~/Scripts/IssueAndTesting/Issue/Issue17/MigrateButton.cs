using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue17
{
    public class MigrateButton : MonoBehaviour
    {
        [Button]
        private void MethodOne() { }

        [Button("Button Text")]
        private void MethodTwo() { }
    }
}
