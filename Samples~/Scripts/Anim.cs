using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class Anim : SaintsMonoBehaviour
    {
        [field: SerializeField]
        public Animator MyAnimator { get; private set; }

        [AnimatorState][FieldLabelText("<icon=star.png /><label />")]
        public AnimatorState animator;

        // [AnimatorState(nameof(MyAnimator))]
        // public string animStateName;

        // [ReadOnly]
        // [AnimatorState]
        // public string animStateDisabled;
        //
        // [AnimatorParam][FieldLabelText("<icon=star.png /><label />")]
        // public string animParamName;
        //
        // [ShowInInspector, AnimatorParam]
        // [LabelText("<icon=star.png /><label />")]
        // private string ShowAnimParamName
        // {
        //     get => animParamName;
        //     set => animParamName = value;
        // }
        //
        // [AnimatorParam(nameof(MyAnimator))]
        // public int animParamHash;
        //
        // [ShowInInspector, AnimatorParam(nameof(MyAnimator))]
        // public int ShowAnimParamHash
        // {
        //     get => animParamHash;
        //     set => animParamHash = value;
        // }
        //
        // [AnimatorParam(nameof(MyAnimator))]
        // public int[] animParamHashes;
        //
        // [ReadOnly]
        // [AnimatorParam]
        // public string animParamNameDisable;
    }
}
