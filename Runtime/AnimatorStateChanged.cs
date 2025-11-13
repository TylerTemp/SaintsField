using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif


// ReSharper disable once CheckNamespace
namespace SaintsField
{
    public class AnimatorStateChanged: IEqualityComparer<AnimatorStateChanged>
    {
#if UNITY_EDITOR
        // ReSharper disable once InconsistentNaming
        public AnimatorControllerLayer layer;
        // ReSharper disable once InconsistentNaming
        public UnityEditor.Animations.AnimatorState state;
#endif

        // ReSharper disable once InconsistentNaming
        public int layerIndex;
        // ReSharper disable once InconsistentNaming
        public AnimationClip animationClip;
        // ReSharper disable once InconsistentNaming
        public IReadOnlyList<string> subStateMachineNameChain;

        public bool Equals(AnimatorStateChanged x, AnimatorStateChanged y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x is null)
            {
                return false;
            }
            if (y is null)
            {
                return false;
            }
            if (x.GetType() != y.GetType())
            {
                return false;
            }
            return x.layerIndex == y.layerIndex
                   && Equals(x.animationClip, y.animationClip)
#if UNITY_EDITOR
                   && Equals(x.layer, y.layer)
                   && Equals(x.state, y.state)
#endif
                   && SubStateMachineNameChainEquals(x.subStateMachineNameChain, y.subStateMachineNameChain);
        }

        private static bool SubStateMachineNameChainEquals(IReadOnlyList<string> x, IReadOnlyList<string> y)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (y == null)
            {
                return false;
            }

            return x.SequenceEqual(y);
        }

        public int GetHashCode(AnimatorStateChanged obj)
        {
            return HashCode.Combine(obj.layer, obj.state, obj.layerIndex, obj.animationClip, obj.subStateMachineNameChain);
        }
    }
}
