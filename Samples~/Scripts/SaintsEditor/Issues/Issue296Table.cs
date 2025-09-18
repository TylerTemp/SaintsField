using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue296Table : SaintsMonoBehaviour
    {
        [Serializable]
        public class ValidCell
        {
            public enum Type
            {
                WorldObject,
                DisplayBiome,
            }

            public Type type;
            [ShowIf(nameof(type), Type.WorldObject)] public string worldObject;

            [ShowIf(nameof(type), Type.DisplayBiome)] public string displayBiome;

            [Serializable, Flags]
            public enum Flags
            {
                F1 = 1,
                F2 = 1 << 1,
            }
            public Flags f2;

            [FlagsTreeDropdown] public Flags f3;

            [MinMaxSlider(0, 100)] public Vector2Int range;
        }

        [Serializable]
        public class Gathering
        {
            public ValidCell cell1;
            public ValidCell cell2;
            public ValidCell cell3;
        }

        [Table] public List<Gathering> gatherings = new List<Gathering>();
    }
}
