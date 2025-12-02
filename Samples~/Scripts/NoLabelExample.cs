using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class NoLabelExample : SaintsMonoBehaviour
    {
        [NoLabel] [ProgressBar(0, 100)] public int mp;
        [NoLabel] [PairsValueButtons("<icon=lightMeter/greenLight/>", true, "<icon=lightMeter/redLight/>", false)] public bool allowed;

        [Serializable, Flags]
        public enum Direction
        {
            [InspectorName("↑")]
            Up = 1,
            [InspectorName("→")]
            Right = 1 << 1,
            [InspectorName("↓")]
            Down = 1 << 2,
            [InspectorName("←")]
            Left = 1 << 3,
        }

        [NoLabel] [EnumToggleButtons] public Direction direction;

        [NoLabel] [GetComponentInChildren] public Transform[] transArray;
    }
}
