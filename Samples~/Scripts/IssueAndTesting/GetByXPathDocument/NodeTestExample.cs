using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument
{
    public class NodeTestExample : SaintsMonoBehaviour
    {
        [GetByXPath("/DirectChild")] public GameObject directChild;
        [GetByXPath("//StartsWith*//*Child/*")] public Transform[] searchChildren;
    }
}
