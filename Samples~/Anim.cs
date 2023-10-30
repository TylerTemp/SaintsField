using ExtInspector;
using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspectorUnity.Samples
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField]
        public Animator Animator { get; private set; }

        [field: SerializeField, AnimatorState(nameof(Animator)), RichLabel("<color=green><label/></color>")]
        public AnimatorState AnimatorState { get; private set; }
        [field: SerializeField, AnimatorState(nameof(Animator))]
        public string AnimStateName { get; private set; }
    }
}
