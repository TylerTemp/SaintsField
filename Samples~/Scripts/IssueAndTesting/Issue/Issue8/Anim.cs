using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class Anim : AnimBase
    {
        [AnimatorParam(nameof(Animator), AnimatorControllerParameterType.Bool)] public int animParam;
        [AnimatorState(nameof(Animator))] public UnityEngine.Animator anim;
    }
}
