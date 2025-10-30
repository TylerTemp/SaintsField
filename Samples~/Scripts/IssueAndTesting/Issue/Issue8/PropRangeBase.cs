using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class PropRangeBase : MonoBehaviour
    {
        public int min;
        public int max;

        [PropRange(nameof(min), nameof(max))]
        [FieldInfoBox("Test")]
        [SepTitle("Test", EColor.Green)]
        [FieldLabelText("<icon=star.png /><label/>")]
        public float rangeFloat;

        [MinMaxSlider(nameof(min), nameof(max))]
        public Vector2 rangeVector2;

        [MinMaxSlider(nameof(min), nameof(max))]
        public Vector2Int rangeVector2Int;
    }
}
