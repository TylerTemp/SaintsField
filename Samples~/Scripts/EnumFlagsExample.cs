using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class EnumFlagsExample: MonoBehaviour
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

        // [Space(60)]

        [EnumToggleButtons, FieldLabelText(null), OnValueChanged(nameof(ValueChanged))] public BitMask myMask2;
        private void ValueChanged() => Debug.Log(myMask2);

        [Serializable]
        public struct MyStruct
        {
            [EnumToggleButtons, FieldBelowText(nameof(myMask), true)] public BitMask myMask;
        }

        public MyStruct myStruct;

        [ReadOnly]
        [EnumToggleButtons]
        public BitMask myMaskDisabled;

        [ReadOnly]
        [EnumToggleButtons]
        [FieldLabelText("<icon=star.png /><label />")]
        public BitMask myMaskDisabledLabel;

        [Serializable]
        public enum EnumNormal
        {
            First,
            Second,
            [FieldLabelText("<color=lime><label /></color>")]
            Third,
        }

        [EnumToggleButtons] public EnumNormal enumNormal;

        [Serializable]
        public enum EnumExpand
        {
            Value1,
            Value2,
            Value3,
            Value4,
            Value5,
            Value6,
            Value7,
            Value8,
            Value9,
            Value10,
        }

        [EnumToggleButtons, FieldDefaultExpand] public EnumExpand enumExpand;

        [Serializable]
        public enum EnumLabelField
        {
            [FieldLabelText("<color=red><label/></color>")]
            None,
            [FieldLabelText("<color=blue>1st</color> (<label/>)")]
            First,
            [InspectorName("<color=green>2nd</color> (<label/>)")]
            Second,
        }

        [EnumToggleButtons, FieldDefaultExpand] public EnumLabelField labelField;
    }
}
