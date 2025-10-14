using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue85 : MonoBehaviour
    {
        [FieldInfoBox("top")]
        [Header("Source")]
        [SepTitle("Source", EColor.Gray)]
        [FieldInfoBox(
            "Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content")]
        [FieldInfoBox("2")]
        [Header("2")]
        [SepTitle("2", EColor.Gray)]
        [FieldInfoBox("3")]
        [Header("3")]
        [SepTitle("3", EColor.Gray)]
        public string mode1;

        public string test;

        [FieldInfoBox(
            "Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content",
            below: true)]
        public string mode2;
    }
}
