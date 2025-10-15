using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class UnityEventFallback : MonoBehaviour
    {
        [FieldRichLabel("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;
    }
}
