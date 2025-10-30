using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class RichLabelBase : MonoBehaviour
    {
        [FieldLabelText("$" + nameof(ArrayLabels))]
        [FieldAboveText("$" + nameof(ArrayLabels))]
        [FieldBelowText("$" + nameof(ArrayLabels))]
        [OverlayText("$" + nameof(ArrayLabels))]
        [EndText("$" + nameof(ArrayLabels))]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";
    }
}
