using SaintsField.Playa;

namespace SaintsField.Samples.Scripts
{
    public class RangeExample: SaintsMonoBehaviour
    {
        [MaxValue(nameof(intMax))] public int intMin = int.MinValue;
        [MinValue(nameof(intMin))] public int intMax = int.MaxValue;
        [PropRange(nameof(intMin), nameof(intMax), 3)] public int intRange;
        [ShowInInspector, PropRange(nameof(intMin), nameof(intMax), 3)]
        public int ShowIntRange
        {
            get => intRange;
            set => intRange = value;
        }
        [PropRange(0, 10), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public int intRangeAdapt;

        public byte byteMin = byte.MinValue;
        public byte byteMax = byte.MaxValue;
        [PropRange(nameof(byteMin), nameof(byteMax))] public byte byteRange;
        [ShowInInspector, PropRange(nameof(byteMin), nameof(byteMax))]
        private byte ShowByteRange
        {
            get => byteRange;
            set => byteRange = value;
        }

        public sbyte sbyteMin = sbyte.MinValue;
        public sbyte sbyteMax = sbyte.MaxValue;
        [PropRange(nameof(sbyteMin), nameof(sbyteMax))] public sbyte sbyteRange;
        [ShowInInspector, PropRange(nameof(sbyteMin), nameof(sbyteMax))]
        private sbyte ShowSbyteRange
        {
            get => sbyteRange;
            set => sbyteRange = value;
        }

        public short shortMin = short.MinValue;
        public short shortMax = short.MaxValue;
        [PropRange(nameof(shortMin), nameof(shortMax))] public short shortRange;
        [ShowInInspector, PropRange(nameof(shortMin), nameof(shortMax))]
        private short ShowShortRange
        {
            get => shortRange;
            set => shortRange = value;
        }

        public ushort uShortMin = ushort.MinValue;
        public ushort uShortMax = ushort.MaxValue;
        [PropRange(nameof(uShortMin), nameof(uShortMax))] public ushort ushortRange;
        [ShowInInspector, PropRange(nameof(uShortMin), nameof(uShortMax))]
        private ushort ShowUShortRange
        {
            get => ushortRange;
            set => ushortRange = value;
        }

        public uint uIntMin = uint.MinValue;
        public uint uIntMax = uint.MaxValue;
        [PropRange(nameof(uIntMin), nameof(uIntMax))] public uint uIntRange;
        [PropRange(0, 10), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public uint uIntAdapt;
        [ShowInInspector, PropRange(nameof(uIntMin), nameof(uIntMax))] public uint ShowUIntRange
        {
            get => uIntRange;
            set => uIntRange = value;
        }

        public float floatMin = float.MinValue;
        public float floatMax = float.MaxValue;
        [PropRange(nameof(floatMin), nameof(floatMax))] public float floatRange;
        [PropRange(0, 1, 0.1), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public float floatAdapt;

        [ShowInInspector, PropRange(nameof(floatMin), nameof(floatMax))]
        public float ShowFloatRange
        {
            get => floatRange;
            set => floatRange = value;
        }

        public double doubleMin = double.MinValue / 2;
        public double doubleMax = double.MaxValue / 2;

        [PropRange(nameof(doubleMin), nameof(doubleMax))] public double doubleRange;
        [PropRange(0, 1, 0.1d), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public double doubleAdapt;
        [ShowInInspector, PropRange(nameof(doubleMin), nameof(doubleMax))]
        public double ShowDoubleRange
        {
            get => doubleRange;
            set => doubleRange = value;
        }

        public long longMin = long.MinValue / 2;
        public long longMax = long.MaxValue / 2;
        [PropRange(nameof(longMin), nameof(longMax))] public long longRange;
        [PropRange(0, 10), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public long longAdapt;
        [ShowInInspector, PropRange(nameof(longMin), nameof(longMax))]
        public long ShowLongRange
        {
            get => longRange;
            set => longRange = value;
        }

        public ulong ulongMin = ulong.MinValue;
        public ulong ulongMax = ulong.MaxValue;

        [PropRange(nameof(ulongMin), nameof(ulongMax)), BelowText("<field/>")] public ulong ulongRange;
        [PropRange(0, 10), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public ulong ulongAdapt;
        [ShowInInspector, PropRange(nameof(ulongMin), nameof(ulongMax))]
        public ulong ShowULongRange
        {
            get => ulongRange;
            set => ulongRange = value;
        }

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
