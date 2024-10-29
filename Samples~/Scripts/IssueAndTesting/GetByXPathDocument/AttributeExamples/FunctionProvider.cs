using System.Linq;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument.AttributeExamples
{
    public class FunctionProvider : MonoBehaviour
    {
        // Example of returning some target
        public Transform[] GetTransforms() => transform.Cast<Transform>().ToArray();
    }
}
