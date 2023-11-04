using UnityEngine;

namespace SaintsField.Samples
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField]
        public Animator Animator { get; private set; }

        [AnimatorState(nameof(Animator))]
        public AnimatorState animatorState;

        [AnimatorState(nameof(Animator))]
        public string animStateName;

        [AnimatorParam(nameof(Animator))]
        public string animParamName;

        [AnimatorParam(nameof(Animator))]
        public int animParamHash;
    }
}
