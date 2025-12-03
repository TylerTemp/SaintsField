using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorEnumToggleButtons : SaintsMonoBehaviour
    {
        [Serializable, Flags]
        public enum BitMask
        {
            None = 0,  // this will be replaced for all/none button
            [FieldLabelText("M<color=red>1</color>")]
            Mask1 = 1,
            [FieldLabelText("M<color=green>2</color>")]
            Mask2 = 1 << 1,
            [FieldLabelText("M<color=blue>3</color>")]
            Mask3 = 1 << 2,
            [FieldLabelText("M4")]
            Mask4 = 1 << 3,
            Mask5 = 1 << 4,
            MaskLongLongLongLong = 1 << 5,
            MaskLongLongLongLong2 = 1 << 6,
            Mask7 = 1 << 7,
            Mask8 = 1 << 8,
        }

        // [RichLabel("<icon=star.png /><label />")]
        [EnumToggleButtons]
        public BitMask myMask;

        [ShowInInspector, EnumToggleButtons]
        private BitMask ShowMyMask
        {
            get => myMask;
            set => myMask = value;
        }

        [Serializable]
        public enum Bit
        {
            None,
            [FieldLabelText("M<color=red>1</color>")]
            Enum1,
            [FieldLabelText("M<color=green>2</color>")]
            Enum2,
            [FieldLabelText("M<color=blue>3</color>")]
            Opt3,
            [FieldLabelText("M4")]
            Opt4,
            Opt5,
            OptLongLongLongLong,
            OptLongLongLongLong2,
            Opt7,
            Opt8,
        }

        // [RichLabel("<icon=star.png /><label />")]
        [EnumToggleButtons]
        public Bit myBit;

        [ShowInInspector, EnumToggleButtons]
        private Bit ShowMyBit
        {
            get => myBit;
            set => myBit = value;
        }
    }
}
