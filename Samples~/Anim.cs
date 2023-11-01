using UnityEngine;

namespace SaintsField.Samples
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField]
        public Animator Animator { get; private set; }

        [field: SerializeField, AnimatorState(nameof(Animator)), RichLabel("<color=green><label/></color>")]
        public AnimatorState AnimatorState { get; private set; }
        [field: SerializeField, AnimatorState(nameof(Animator))]
        public string AnimStateName { get; private set; }

        [SerializeField, AnimatorParam(nameof(Animator))]
        private string _animParamName;

        [SerializeField, AnimatorParam(nameof(Animator))]
        private int _animParamHash;
    }
}
