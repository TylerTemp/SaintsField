using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class RichLabelBase : MonoBehaviour
    {
        [RichLabel(nameof(ArrayLabels), true)]
        [AboveRichLabel(nameof(ArrayLabels), true)]
        [BelowRichLabel(nameof(ArrayLabels), true)]
        [OverlayRichLabel(nameof(ArrayLabels), true)]
        [PostFieldRichLabel(nameof(ArrayLabels), true)]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";
    }
}
