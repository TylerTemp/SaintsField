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
            Mask1 = 1,
            Mask2 = 1 << 1,
            Mask3 = 1 << 2,
            Mask4 = 1 << 3,
            Mask5 = 1 << 4,
            // Mask6 = 1 << 5,
        }

        [EnumFlags] [RichLabel("<icon=star.png /><label />")]
        public BitMask myMask;
        [EnumFlags, RichLabel(null), OnValueChanged(nameof(ValueChanged))] public BitMask myMask2;

        private void ValueChanged() => Debug.Log(myMask2);

        [Serializable]
        public struct MyStruct
        {
            [EnumFlags, BelowRichLabel(nameof(myMask), true)] public BitMask myMask;
        }

        public MyStruct myStruct;

        [ReadOnly]
        [EnumFlags]
        public BitMask myMaskDisabled;

        [ReadOnly]
        [EnumFlags]
        [RichLabel("<icon=star.png /><label />")]
        public BitMask myMaskDisabledLabel;

    }
}
