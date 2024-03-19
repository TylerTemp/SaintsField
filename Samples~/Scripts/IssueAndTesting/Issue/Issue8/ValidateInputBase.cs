using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ValidateInputBase : MonoBehaviour
    {
        protected string OnValidateCallback(object value) => $"Not valide {value}";
    }
}
