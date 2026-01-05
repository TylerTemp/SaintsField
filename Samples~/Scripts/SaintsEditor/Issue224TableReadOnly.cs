using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class Issue224TableReadOnly : SaintsMonoBehaviour
    {
        [Serializable]
        public struct LootDrop
        {
            public float ratio;
        }

        [Serializable]
        public struct LootWeightQuantityStruct
        {
            [FieldReadOnly]
            public string DisplayName;
            public LootDrop Loot;
            [FieldReadOnly]
            public string Chance;
            public int Weight;
            public int MinQuantity;
            public int MaxQuantity;
        }
        [Table] public LootWeightQuantityStruct[] loots;
    }
}
