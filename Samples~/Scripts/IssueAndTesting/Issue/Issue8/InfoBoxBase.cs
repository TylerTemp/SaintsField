using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class InfoBoxBase : MonoBehaviour
    {
        [FieldInfoBox(nameof(DynamicMessage), EMessageType.Warning, isCallback: true)]
        [FieldBelowInfoBox(nameof(DynamicMessageWithIcon), isCallback: true)]
        public bool _content;

        private string DynamicMessage() => _content ? "False" : "True";
        private (EMessageType, string) DynamicMessageWithIcon => _content ? (EMessageType.Error, "False!") : (EMessageType.None, "True!");
    }
}
