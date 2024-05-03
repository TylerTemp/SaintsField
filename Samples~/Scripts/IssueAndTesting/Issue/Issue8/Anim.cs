using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class Anim : AnimBase
    {
        [AnimatorParam(nameof(Animator), AnimatorControllerParameterType.Bool)] public int animParam;
        [AnimatorParam(nameof(Animator))] public int animParam2;
        [AnimatorState(nameof(Animator))] public string anim;
    }
}
