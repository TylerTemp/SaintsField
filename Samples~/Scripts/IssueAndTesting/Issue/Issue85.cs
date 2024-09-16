using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue85 : MonoBehaviour
    {
        public enum Modes
        {
            Mode1,
            Mode2,
            Mode3,
        }

        [Header("Source")]
        [InfoBox(
            "Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content")]
        public Modes mode1;

        public string test;

        [InfoBox(
            "Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content", below: true)]
        public Modes mode2;
    }
}
