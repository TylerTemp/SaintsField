using System;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue278EnumUnderNotInt : SaintsMonoBehaviour
    {
        [Serializable, Flags]
        public enum UnderUnsignedInt : uint
        {
            NoWonder = 0,
            One = 1,
            Two = 1 << 1,
            Three = 1 << 2,
        }

        public UnderUnsignedInt defaultPickerUInt;
        [TreeDropdown] public UnderUnsignedInt treePickerSingleUInt;
        [FlagsTreeDropdown] public UnderUnsignedInt treePickerUInt;
        [EnumToggleButtons] public UnderUnsignedInt togglePickerUInt;
        [ValueButtons] public UnderUnsignedInt valuePickerUInt;

        [Serializable, Flags]
        public enum UnderByte : byte
        {
            NoWonder = 0,
            One = 1,
            Two = 1 << 1,
            Three = 1 << 2,
        }

        public UnderByte defaultPickerByte;
        [TreeDropdown] public UnderByte treePickerSingleByte;
        [FlagsTreeDropdown] public UnderByte treePickerByte;
        [EnumToggleButtons] public UnderByte togglePickerByte;
        [ValueButtons] public UnderByte valuePickerByte;
    }
}
