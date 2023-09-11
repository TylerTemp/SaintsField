using System;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [Serializable]
    public struct AnimState
    {
        public int layerIndex;
        public int stateNameHash;
        public string stateName;
        public float stateSpeed;
        public AnimationClip animationClip;

        public override string ToString() => $"[{layerIndex}] {stateName}{(animationClip? $" ({animationClip.name})": "")}";

        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((AnimState) obj);
        }

        public bool Equals(AnimState other)
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
                hashCode = (hashCode * 397) ^ (stateSpeed.GetHashCode());
                return hashCode;
            }
        }
    }
}
