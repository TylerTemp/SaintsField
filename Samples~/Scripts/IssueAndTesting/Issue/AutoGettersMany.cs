using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class AutoGettersMany : MonoBehaviour
    {
        [GetByXPath("assets:://*.prefab")]
        [GetByXPath("scene:://*")]
        public Transform prefab;
    }
}
