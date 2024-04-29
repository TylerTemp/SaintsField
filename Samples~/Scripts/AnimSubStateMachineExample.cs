using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AnimSubStateMachineExample : MonoBehaviour
    {
        [AnimatorState, OnValueChanged(nameof(OnChanged))]
        public string stateName;

        [AnimatorState, OnValueChanged(nameof(OnChangedState))]
        public AnimatorState state;

        // This does not have a `animationClip`, thus it won't include a resource target when serialized: only pure data.
        [AnimatorState, OnValueChanged(nameof(OnChangedState))]
        public AnimatorStateBase stateBase;

        private void OnChanged(string changedValue) => Debug.Log(changedValue);
        private void OnChangedState(AnimatorStateChanged changedValue) => Debug.Log($"layerIndex={changedValue.layerIndex}, animationClip={changedValue.animationClip}, subStateMachineNameChain={string.Join("/", changedValue.subStateMachineNameChain)}");
    }
}
