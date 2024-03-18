using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class PropRangeBase : MonoBehaviour
    {
        public int min;
        public int max;

        [PropRange(nameof(min), nameof(max))]
        [InfoBox("Test")]
        [SepTitle("Test", EColor.Green)]
        public float rangeFloat;
    }
}
