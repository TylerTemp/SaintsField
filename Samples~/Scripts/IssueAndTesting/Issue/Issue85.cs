using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue85 : MonoBehaviour
    {
        [InfoBox("top")]
        [Header("Source")]
        [SepTitle("Source", EColor.Gray)]
        [InfoBox(
            "Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content")]
        [InfoBox("2")]
        [Header("2")]
        [SepTitle("2", EColor.Gray)]
        [InfoBox("3")]
        [Header("3")]
        [SepTitle("3", EColor.Gray)]
        public string mode1;

        public string test;

        [InfoBox(
            "Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content",
            below: true)]
        public string mode2;
    }
}
