using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField]
        public Animator Animator { get; private set; }

        [AnimatorState][RichLabel("<label />")]
        public AnimatorState animatorState;

        [AnimatorState(nameof(Animator))]
        public string animStateName;
        //
        // [AnimatorParam]
        // public string animParamName;

        // [AnimatorParam(nameof(Animator))]
        // public int animParamHash;

        [AnimatorParam(nameof(Animator))]
        public int[] animParamHashes;
    }
}
