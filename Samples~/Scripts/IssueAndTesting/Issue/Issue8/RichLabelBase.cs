using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class RichLabelBase : MonoBehaviour
    {
        [RichLabel("$" + nameof(ArrayLabels))]
        [AboveRichLabel("$" + nameof(ArrayLabels))]
        [BelowRichLabel("$" + nameof(ArrayLabels))]
        [OverlayRichLabel("$" + nameof(ArrayLabels))]
        [PostFieldRichLabel("$" + nameof(ArrayLabels))]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";
    }
}
