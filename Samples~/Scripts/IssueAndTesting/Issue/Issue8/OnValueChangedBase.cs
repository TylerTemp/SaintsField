using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class OnValueChangedBase : MonoBehaviour
    {
        protected void OnValueChangedCallback(object value) => Debug.Log($"Value changed to {value}");
    }
}
