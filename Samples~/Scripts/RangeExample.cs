using System;
using UnityEngine;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts
{
    public class RangeExample: SaintsMonoBehaviour
    {
        [MaxValue(nameof(max))] public int min;
        [MinValue(nameof(min))] public int max;

        // [PropRange(nameof(min), nameof(max), 3)] public int intRange;
        // [PropRange(nameof(min), nameof(max))] public short shortRange;
        // [PropRange(nameof(min), nameof(max))] public ushort ushortRange;
        //
        // public uint uMin = uint.MinValue;
        // public uint uMax = uint.MaxValue;
        // [PropRange(nameof(uMin), nameof(uMax))] public uint uIntRange;
        //
        // public float floatMin = float.MinValue;
        // public float floatMax = float.MaxValue;
        //
        // [PropRange(nameof(floatMin), nameof(floatMax))] public float floatRange;
        //
        // public double doubleMin = double.MinValue;
        // public double doubleMax = double.MaxValue;
        //
        // [PropRange(nameof(doubleMin), nameof(doubleMax))] public double doubleRange;

        public long longMin = long.MinValue;
        public long longMax = long.MaxValue;

        [PropRange(nameof(longMin), nameof(longMax))]
        public long longRange;

        // [ShowInInspector] private bool v => float.MaxValue > double.MaxValue;
        // [ShowInInspector] private bool v2 => float.MinValue < double.MinValue;

        // [PropRange(nameof(min), nameof(max))] public char charRange;
        // [PropRange(nameof(min), nameof(max))] public byte byteRange;

        // [PropRange(nameof(min), nameof(max))]
        // [FieldInfoBox("Test")]
        // [SepTitle("Test", EColor.Green)]
        // public float rangeFloat;
        //
        // [PropRange(nameof(min), nameof(max))] public int rangeInt;
        //
        // [PropRange(nameof(min), nameof(max), step: 0.5f)] public float rangeFloatStep;
        // [PropRange(nameof(min), nameof(max), step: 2)] public int rangeIntStep;
        //
        // [Range(0, 10)] public int normalRange;
        // [Range(0, 10)] public float normalFloatRange;
        // [Range(0, 10)] public double normalDoubleRange;
        // [Range(0, 10)] public long normalLongRange;
        //
        // [Serializable]
        // public struct MyRange
        // {
        //     public int min;
        //     public int max;
        //
        //     [PropRange(nameof(min), nameof(max))]
        //     [FieldBelowText(nameof(rangeFloat), true)]
        //     [SepTitle("Test", EColor.Green)]
        //     public float rangeFloat;
        // }
        //
        // public MyRange myRange;
        //
        // [ReadOnly]
        // [PropRange(nameof(min), nameof(max))]
        // public float rangeFloatDisabled;
        //
        // public ulong minULong = ulong.MinValue;
        // public ulong maxULong = ulong.MaxValue;
        //
        // [Range(ulong.MinValue, ulong.MaxValue)]
        // public ulong rangeULong;
        //
        // [Range(byte.MinValue, byte.MaxValue)] public byte bt;
        // [Range(uint.MinValue, uint.MaxValue)] public uint ui;

    }
}
