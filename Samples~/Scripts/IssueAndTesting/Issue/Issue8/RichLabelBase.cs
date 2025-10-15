using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class RichLabelBase : MonoBehaviour
    {
        [FieldRichLabel("$" + nameof(ArrayLabels))]
        [FieldAboveText("$" + nameof(ArrayLabels))]
        [FieldBelowText("$" + nameof(ArrayLabels))]
        [OverlayRichLabel("$" + nameof(ArrayLabels))]
        [PostFieldRichLabel("$" + nameof(ArrayLabels))]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";
    }
}
