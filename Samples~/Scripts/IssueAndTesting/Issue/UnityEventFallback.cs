using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class UnityEventFallback : MonoBehaviour
    {
        [RichLabel("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;
    }
}
