using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue234 : MonoBehaviour
    {
        [Required("Custom required message", EMessageType.Info)]
        public GameObject empty1;

        [Required(messageType: EMessageType.Info)]
        public GameObject empty2;
    }
}
