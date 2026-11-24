using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class UnityBuildInAttr : SaintsMonoBehaviour
    {
        [ColorUsage(false, true)] public Color color;

        [ColorUsage(false, true)] [ShowInInspector]
        public Color ShowColor;

        [ColorUsage(false, true)] [ShowInInspector]
        private int _showColorWrongType;

        [Multiline(6)]
        public string shortNote;

        [Multiline(6)] [ShowInInspector] public string ShowShortNote;

        [TextArea] public string textArea;

        [TextArea] [ShowInInspector] public string ShowTextArea;
        [Min(10)] public sbyte minSByte;

        [Min(10)] [ShowInInspector] public sbyte ShowMinSByte;

        [Min(10)] public byte minByte;

        [Min(10)] [ShowInInspector] public byte ShowMinByte;

        [Min(10)] public short minShort;

        [Min(10)] [ShowInInspector] public short ShowMinShort;

        [Min(10)] public ushort minUShort;

        [Min(10)] [ShowInInspector] public ushort ShowMinUShort;

        [Min(10)] public int minInt;

        [Min(10)] [ShowInInspector] public int ShowMinInt;

        [Min(10)] public uint minUInt;

        [Min(10)] [ShowInInspector] public uint ShowMinUInt;

        [Min(10)] public long minLong;

        [Min(10)] [ShowInInspector] public long ShowMinLong;

        [Min(10)] public ulong minULong;

        [Min(10)] [ShowInInspector] public ulong ShowMinULong;

        [Min(0.5f)] public float minFloat;

        [Min(0.5f)] [ShowInInspector] public float ShowMinFloat;

        [Min(0.5f)] public double minDouble;

        [Min(0.5f)] [ShowInInspector] public double ShowMinDouble;

        [Min(10)] [ShowInInspector]
        private string _showMinWrongType;

        [Range(1, 10)] public sbyte rangeSByte;

        [Range(1, 10)] [ShowInInspector] public sbyte ShowRangeSByte;

        [Range(1, 10)] public byte rangeByte;

        [Range(1, 10)] [ShowInInspector] public byte ShowRangeByte;
        [Range(1, 10)] public short rangeShort;

        [Range(1, 10)] [ShowInInspector] public short ShowRangeShort;

        [Range(1, 10)] public ushort rangeUShort;

        [Range(1, 10)] [ShowInInspector] public ushort ShowRangeUShort;

        [Range(1, 10)] public int rangeInt;

        [Range(1, 10)] [ShowInInspector] public int ShowRangeInt;

        [Range(1, 10)] public uint rangeUInt;

        [Range(1, 10)] [ShowInInspector] public uint ShowRangeUInt;

        [Range(1, 10)] public long rangeLong;

        [Range(1, 10)] [ShowInInspector] public long ShowRangeLong;

        [Range(1, 10)] public ulong rangeULong;

        [Range(1, 10)] [ShowInInspector] public ulong ShowRangeULong;

        [Range(0.5f, 2f)] public float rangeFloat;

        [Range(0.5f, 2f)] [ShowInInspector] public float ShowRangeFloat;

        [Range(0.5f, 2f)] public double rangeDouble;

        [Range(0.5f, 2f)] [ShowInInspector] public double ShowRangeDouble;

        [GradientUsage(true, ColorSpace.Linear)]
        public Gradient linearHdrGradient;

        [GradientUsage(true, ColorSpace.Linear)] [ShowInInspector]
        private Gradient _showLinearHdrGradient;

        [GradientUsage(true, ColorSpace.Linear)] [ShowInInspector]
        private int _showGradientWrongType;

        [Header("Header On Field")]
        public int headerField;

        [Header("Header On Property")] [ShowInInspector]
        public int ShowHeaderProp;

        // `HeaderAttribute` is `AllowMultiple = true`
        [Header("First Header")] [Header("Second Header")] [ShowInInspector]
        public int ShowMultipleHeaderProp;

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
