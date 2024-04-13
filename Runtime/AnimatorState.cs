using System;
using SaintsField.Animate;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public struct AnimatorState: IAnimationClip, ILayerIndex, IStateNameHash, IStateName, IStateSpeed, IStateTag, ISubStateMachineNameChain
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
                   && stateNameHash == other.stateNameHash
                   && ReferenceEquals(animationClip, other.animationClip);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = layerIndex;
                hashCode = (hashCode * 397) ^ stateNameHash;
                hashCode = (hashCode * 397) ^ (animationClip != null ? animationClip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ stateSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ (stateTag != null ? stateTag.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
