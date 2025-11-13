using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Animate;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public struct AnimatorState: IAnimationClip, ILayerIndex, IStateNameHash, IStateName, IStateSpeed, IStateTag, ISubStateMachineNameChain
        , IEqualityComparer<AnimatorState>
    {
        [field: SerializeField]
        public int layerIndex { get; private set; }
        [field: SerializeField]
        public int stateNameHash { get; private set; }

        [field: SerializeField]
        public string stateName { get; private set; }
        [field: SerializeField]
        public float stateSpeed { get; private set; }

        [field: SerializeField]
        public string stateTag { get; private set; }

        [field: SerializeField]
        public AnimationClip animationClip { get; private set; }

        [field: SerializeField]
        public string[] subStateMachineNameChain { get; private set; }

        public AnimatorState(int layerIndex, int stateNameHash, string stateName, float stateSpeed, string stateTag, AnimationClip animationClip, string[] subStateMachineNameChain)
        {
            this.layerIndex = layerIndex;
            this.stateNameHash = stateNameHash;
            this.stateName = stateName;
            this.stateSpeed = stateSpeed;
            this.stateTag = stateTag;
            this.animationClip = animationClip;
            this.subStateMachineNameChain = subStateMachineNameChain;
        }

        public override string ToString() => $"[{layerIndex}] {stateName}{(animationClip? $" ({animationClip.name})": "")}";

        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((AnimatorState) obj);
        }

        public bool Equals(AnimatorState other)
        {
            return layerIndex == other.layerIndex
                   && stateNameHash == other.stateNameHash;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = layerIndex;
                hashCode = (hashCode * 397) ^ stateNameHash;
                return hashCode;
            }
        }

        public bool Equals(AnimatorState x, AnimatorState y)
        {
            return x.layerIndex == y.layerIndex
                   && x.stateNameHash == y.stateNameHash
                   && x.stateName == y.stateName
                   && x.stateSpeed.Equals(y.stateSpeed)
                   && x.stateTag == y.stateTag
                   && Equals(x.animationClip, y.animationClip)
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

        public static bool operator ==(AnimatorState lhs, AnimatorState rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(AnimatorState lhs, AnimatorState rhs)
        {
            return !lhs.Equals(rhs);
        }

        public int GetHashCode(AnimatorState obj)
        {
            return HashCode.Combine(obj.layerIndex, obj.stateNameHash, obj.stateName, obj.stateSpeed, obj.stateTag, obj.animationClip, obj.subStateMachineNameChain);
        }
    }
}
