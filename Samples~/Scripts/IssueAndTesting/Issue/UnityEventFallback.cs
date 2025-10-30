using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class UnityEventFallback : MonoBehaviour
    {
        [FieldLabelText("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;
    }
}
