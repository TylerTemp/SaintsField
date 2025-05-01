using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class Issue224TableReadOnly : MonoBehaviour
    {
        [Serializable]
        public struct LootDrop
        {
            public float ratio;
        }

        [Serializable]
        public struct LootWeightQuantityStruct
        {
            [ReadOnly]
            public string DisplayName;
            public LootDrop Loot;
            [ReadOnly]
            public string Chance;
            public int Weight;
            public int MinQuantity;
            public int MaxQuantity;
        }
        [Table] public LootWeightQuantityStruct[] loots;
    }
}
