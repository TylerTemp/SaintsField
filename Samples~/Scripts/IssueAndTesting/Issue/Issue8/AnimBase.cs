using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class AnimBase : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Animator _animator;
        public UnityEngine.Animator Animator => _animator;
    }
}
