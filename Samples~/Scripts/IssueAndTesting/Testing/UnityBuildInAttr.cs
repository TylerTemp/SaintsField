using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class UnityBuildInAttr : SaintsMonoBehaviour
    {
        [ColorUsage(false, true)] public Color color;

        [ColorUsage(false, true)]
        [ShowInInspector]
        public Color ShowColor
        {
            get => color;
            set => color = value;
        }

        [ColorUsage(false, true)] [ShowInInspector]
        private int _showColorWrongType;

        [Multiline(6)]
        public string shortNote;

        [Multiline(6)]
        [ShowInInspector]
        public string ShowShortNote
        {
            get => shortNote;
            set => shortNote = value;
        }

        [TextArea] public string textArea;

        [TextArea]
        [ShowInInspector]
        public string ShowTextArea
        {
            get => textArea;
            set => textArea = value;
        }

        [Min(10)] public sbyte minSByte;

        [Min(10)]
        [ShowInInspector]
        public sbyte ShowMinSByte
        {
            get => minSByte;
            set => minSByte = value;
        }

        [Min(10)] public byte minByte;

        [Min(10)]
        [ShowInInspector]
        public byte ShowMinByte
        {
            get => minByte;
            set => minByte = value;
        }

        [Min(10)] public short minShort;

        [Min(10)]
        [ShowInInspector]
        public short ShowMinShort
        {
            get => minShort;
            set => minShort = value;
        }

        [Min(10)] public ushort minUShort;

        [Min(10)]
        [ShowInInspector]
        public ushort ShowMinUShort
        {
            get => minUShort;
            set => minUShort = value;
        }

        [Min(10)] public int minInt;

        [Min(10)]
        [ShowInInspector]
        public int ShowMinInt
        {
            get => minInt;
            set => minInt = value;
        }

        [Min(10)] public uint minUInt;

        [Min(10)]
        [ShowInInspector]
        public uint ShowMinUInt
        {
            get => minUInt;
            set => minUInt = value;
        }

        [Min(10)] public long minLong;

        [Min(10)]
        [ShowInInspector]
        public long ShowMinLong
        {
            get => minLong;
            set => minLong = value;
        }

        [Min(10)] public ulong minULong;

        [Min(10)]
        [ShowInInspector]
        public ulong ShowMinULong
        {
            get => minULong;
            set => minULong = value;
        }

        [Min(0.5f)] public float minFloat;

        [Min(0.5f)]
        [ShowInInspector]
        public float ShowMinFloat
        {
            get => minFloat;
            set => minFloat = value;
        }

        [Min(0.5f)] public double minDouble;

        [Min(0.5f)]
        [ShowInInspector]
        public double ShowMinDouble
        {
            get => minDouble;
            set => minDouble = value;
        }

        [Min(10)] [ShowInInspector]
        private string _showMinWrongType;

        [Range(1, 10)] public sbyte rangeSByte;

        [Range(1, 10)]
        [ShowInInspector]
        public sbyte ShowRangeSByte
        {
            get => rangeSByte;
            set => rangeSByte = value;
        }

        [Range(1, 10)] public byte rangeByte;

        [Range(1, 10)]
        [ShowInInspector]
        public byte ShowRangeByte
        {
            get => rangeByte;
            set => rangeByte = value;
        }

        [Range(1, 10)] public short rangeShort;

        [Range(1, 10)]
        [ShowInInspector]
        public short ShowRangeShort
        {
            get => rangeShort;
            set => rangeShort = value;
        }

        [Range(1, 10)] public ushort rangeUShort;

        [Range(1, 10)]
        [ShowInInspector]
        public ushort ShowRangeUShort
        {
            get => rangeUShort;
            set => rangeUShort = value;
        }

        [Range(1, 10)] public int rangeInt;

        [Range(1, 10)]
        [ShowInInspector]
        public int ShowRangeInt
        {
            get => rangeInt;
            set => rangeInt = value;
        }

        [Range(1, 10)] public uint rangeUInt;

        [Range(1, 10)]
        [ShowInInspector]
        public uint ShowRangeUInt
        {
            get => rangeUInt;
            set => rangeUInt = value;
        }

        [Range(1, 10)] public long rangeLong;

        [Range(1, 10)]
        [ShowInInspector]
        public long ShowRangeLong
        {
            get => rangeLong;
            set => rangeLong = value;
        }

        [Range(1, 10)] public ulong rangeULong;

        [Range(1, 10)]
        [ShowInInspector]
        public ulong ShowRangeULong
        {
            get => rangeULong;
            set => rangeULong = value;
        }

        [Range(0.5f, 2f)] public float rangeFloat;

        [Range(0.5f, 2f)]
        [ShowInInspector]
        public float ShowRangeFloat
        {
            get => rangeFloat;
            set => rangeFloat = value;
        }

        [Range(0.5f, 2f)] public double rangeDouble;

        [Range(0.5f, 2f)]
        [ShowInInspector]
        public double ShowRangeDouble
        {
            get => rangeDouble;
            set => rangeDouble = value;
        }

        [GradientUsage(true, ColorSpace.Linear)]
        public Gradient linearHdrGradient;

        [GradientUsage(true, ColorSpace.Linear)] [ShowInInspector]
        private Gradient _showLinearHdrGradient;

        [GradientUsage(true, ColorSpace.Linear)] [ShowInInspector]
        private int _showGradientWrongType;

        [Header("Header On Field")]
        public int headerField;

        [Header("Header On Property")]
        [ShowInInspector]
        public int ShowHeaderProp
        {
            get => headerField;
            set => headerField = value;
        }

        // `HeaderAttribute` is `AllowMultiple = true`
        [Header("First Header")]
        [Header("Second Header")]
        [ShowInInspector]
        public int ShowMultipleHeaderProp
        {
            get => headerField;
            set => headerField = value;
        }

#if UNITY_6000_0_OR_NEWER
        public enum EEnumButtons
        {
            First,
            Second,
            [System.Obsolete] Deprecated,
            Third,
        }

        [System.Flags]
        public enum EEnumButtonsFlags
        {
            None = 0,
            Up = 1,
            Down = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
        }

        [EnumButtons] public EEnumButtons enumButtons;

        [EnumButtons]
        [ShowInInspector]
        public EEnumButtons ShowEnumButtons
        {
            get => enumButtons;
            set => enumButtons = value;
        }

        // Unity hides obsolete values by default; `includeObsolete` brings `Deprecated` back
        [EnumButtons(true)]
        [ShowInInspector]
        public EEnumButtons ShowEnumButtonsObsolete
        {
            get => enumButtons;
            set => enumButtons = value;
        }

        [EnumButtons] public EEnumButtonsFlags enumButtonsFlags;

        [EnumButtons]
        [ShowInInspector]
        public EEnumButtonsFlags ShowEnumButtonsFlags
        {
            get => enumButtonsFlags;
            set => enumButtonsFlags = value;
        }
#endif

#if UNITY_2022_1_OR_NEWER
        // `InspectorOrder` is `AttributeTargets.Enum`: it decorates the enum type, not the member
        [InspectorOrder(InspectorSort.ByValue, InspectorSortDirection.Descending)]
        public enum EOrderByValueDesc
        {
            SecondItem = 2,
            ThirdItem = 3,
            FirstItem = 1,
        }

        // the parameterless form is `ByName` + `Ascending`
        [InspectorOrder]
        public enum EOrderByNameAsc
        {
            Charlie,
            Alpha,
            Bravo,
        }

        [InspectorOrder(InspectorSort.ByName, InspectorSortDirection.Descending)]
        [System.Flags]
        public enum EOrderFlags
        {
            None = 0,
            Alpha = 1,
            Charlie = 1 << 1,
            Bravo = 1 << 2,
        }

        public EOrderByValueDesc orderByValueDesc;

        [ShowInInspector]
        public EOrderByValueDesc ShowOrderByValueDesc
        {
            get => orderByValueDesc;
            set => orderByValueDesc = value;
        }

        public EOrderByNameAsc orderByNameAsc;

        [ShowInInspector]
        public EOrderByNameAsc ShowOrderByNameAsc
        {
            get => orderByNameAsc;
            set => orderByNameAsc = value;
        }

        public EOrderFlags orderFlags;

        [ShowInInspector]
        public EOrderFlags ShowOrderFlags
        {
            get => orderFlags;
            set => orderFlags = value;
        }

        // the ordering should hold in the SaintsField widgets too, not just the plain dropdown
        [EnumToggleButtons] public EOrderByNameAsc orderByNameAscButtons;

        [EnumToggleButtons] public EOrderFlags orderFlagsButtons;

        [FlagsDropdown] public EOrderFlags orderFlagsDropdown;
#endif

        [OnInspectorInit]
        private void EditorInit()
        {
            _showLinearHdrGradient = linearHdrGradient;
        }
    }
}
