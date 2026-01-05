using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class Anim : SaintsMonoBehaviour
    {
        [field: SerializeField]
        public Animator MyAnimator { get; private set; }

        [OnValueChanged(":Debug.Log")]
        [AnimatorState][FieldLabelText("<icon=star.png /><label />")]
        public AnimatorState animatorState;

        [ShowInInspector]
        private AnimatorState ShowAnimatorState
        {
            get => animatorState;
            set => animatorState = value;
        }

        public AnimatorStateBase animatorStateBase;

        [ShowInInspector]
        private AnimatorStateBase ShowAnimatorStateBase
        {
            get => animatorStateBase;
            set => animatorStateBase = value;
        }

        [OnValueChanged(":Debug.Log")]
        [AnimatorState(nameof(MyAnimator))]
        public string animStateName;

        [ShowInInspector, AnimatorState(nameof(MyAnimator))]
        public string ShowAnimStateName
        {
            get => animStateName;
            set => animStateName = value;
        }

        [FieldReadOnly]
        [AnimatorState]
        public string animStateDisabled;

        [OnValueChanged(":Debug.Log")]
        [AnimatorParam][FieldLabelText("<icon=star.png /><label />")]
        public string animParamName;

        [ShowInInspector, AnimatorParam]
        [LabelText("<icon=star.png /><label />")]
        private string ShowAnimParamName
        {
            get => animParamName;
            set => animParamName = value;
        }

        [OnValueChanged(":Debug.Log")]
        [AnimatorParam(nameof(MyAnimator))]
        public int animParamHash;

        [ShowInInspector, AnimatorParam(nameof(MyAnimator))]
        public int ShowAnimParamHash
        {
            get => animParamHash;
            set => animParamHash = value;
        }

        [AnimatorParam(nameof(MyAnimator))]
        public int[] animParamHashes;

        [FieldReadOnly]
        [AnimatorParam]
        public string animParamNameDisable;
    }
}
