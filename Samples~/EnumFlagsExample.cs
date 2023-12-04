using System;
using UnityEngine;

namespace SaintsField.Samples
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
            // Mask4 = 1 << 3,
            // Mask5 = 1 << 4,
            // Mask6 = 1 << 5,
        }

        [EnumFlags] public BitMask myMask;
        // [EnumFlags, RichLabel(null)] public BitMask myMask2;

        // private void ValueChanged() => Debug.Log(myMask);
    }
}
