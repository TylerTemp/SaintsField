using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue404AnimCrash : MonoBehaviour {
        public Animator animator;

        [AnimatorState(nameof(animator))]
        public string stateName;
    }
}
