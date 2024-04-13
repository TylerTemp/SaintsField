using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AnimSubStateMachineExample : MonoBehaviour
    {
        [AnimatorState]
        public AnimatorState state;

        // This does not have a `animationClip`, thus it won't include a resource target when serialized: only pure data.
        [AnimatorState]
        public AnimatorStateBase stateBase;
    }
}
