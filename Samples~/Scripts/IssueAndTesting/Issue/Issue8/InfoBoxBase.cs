using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class InfoBoxBase : MonoBehaviour
    {
        [InfoBox(nameof(DynamicMessage), EMessageType.Warning, isCallback: true)]
        [BelowInfoBox(nameof(DynamicMessageWithIcon), isCallback: true)]
        public bool _content;

        private string DynamicMessage() => _content ? "False" : "True";
        private (EMessageType, string) DynamicMessageWithIcon => _content ? (EMessageType.Error, "False!") : (EMessageType.None, "True!");
    }
}
