using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class AnimBase : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        public Animator Animator => _animator;
    }
}
