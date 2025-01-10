using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument
{
    public class NodeTestExample : MonoBehaviour
    {
        [GetByXPath("/DirectChild")] public GameObject directChild;
        [GetByXPath("//StartsWith*//*Child/*")] public Transform[] searchChildren;
    }
}
