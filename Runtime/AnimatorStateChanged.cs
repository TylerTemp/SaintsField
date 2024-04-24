using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif


namespace SaintsField
{
    public class AnimatorStateChanged
    {
#if UNITY_EDITOR
        public AnimatorControllerLayer layer;
        public UnityEditor.Animations.AnimatorState state;
#endif

        public int layerIndex;
        public AnimationClip animationClip;
        public IReadOnlyList<string> subStateMachineNameChain;
    }
}
