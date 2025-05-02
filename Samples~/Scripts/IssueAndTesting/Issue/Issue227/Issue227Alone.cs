using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue227
{
    public class Issue227Alone : MonoBehaviour
    {
        [Range(0, 1), ReadOnly(nameof(randomizeOffset))]
        public float delay;

        public bool randomizeOffset = true;

        [ReadOnly(nameof(randomizeOffset)), Range(0, 1)]
        public float delay2;
    }
}
