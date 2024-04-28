using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField]
        public Animator Animator { get; private set; }

        [AnimatorState][RichLabel("<icon=star.png /><label />")]
        public AnimatorState animator;

        [AnimatorState(nameof(Animator))]
        public string animStateName;

        [ReadOnly]
        [AnimatorState]
        public string animStateDisabled;

        [AnimatorParam][RichLabel("<icon=star.png /><label />")]
        public string animParamName;

        [AnimatorParam(nameof(Animator))]
        public int animParamHash;

        [AnimatorParam(nameof(Animator))]
        public int[] animParamHashes;

        [ReadOnly]
        [AnimatorParam]
        public string animParamNameDisable;
    }
}
