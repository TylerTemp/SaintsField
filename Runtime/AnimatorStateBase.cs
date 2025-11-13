using System;
using SaintsField.Animate;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public struct AnimatorStateBase: ILayerIndex, IStateNameHash, IStateName, IStateSpeed, IStateTag, ISubStateMachineNameChain
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
        public string[] subStateMachineNameChain { get; private set; }

        public AnimatorStateBase(int layerIndex, int stateNameHash, string stateName, float stateSpeed = 1f, string stateTag = "", string[] subStateMachineNameChain = null)
        {
            this.layerIndex = layerIndex;
            this.stateNameHash = stateNameHash;
            this.stateName = stateName ?? string.Empty;
            this.stateSpeed = stateSpeed;
            this.stateTag = stateTag ?? string.Empty;
            this.subStateMachineNameChain = subStateMachineNameChain ?? Array.Empty<string>();
        }

        public override string ToString() => $"[{layerIndex}] {stateName}";

        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((AnimatorStateBase) obj);
        }

        public bool Equals(AnimatorStateBase other)
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
    }
}
