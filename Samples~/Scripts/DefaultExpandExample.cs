using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class DefaultExpandExample : MonoBehaviour
    {
        [Serializable]
        public struct SaintsRowStruct
        {
            [LayoutStart("Hi", ELayout.TitleBox)]
            public string s1;
            public string s2;

        }

        [DefaultExpand]
        public SaintsRowStruct defaultStruct;

        [DefaultExpand, SaintsRow] public SaintsRowStruct row;

        [DefaultExpand, GetScriptableObject, Expandable] public Scriptable so;

        [Serializable, Flags]
        public enum BitMask
        {
            None = 0,  // this will be replaced for all/none button
            [RichLabel("M<color=red>1</color>")]
            Mask1 = 1,
            [RichLabel("M<color=green>2</color>")]
            Mask2 = 1 << 1,
            [RichLabel("M<color=blue>3</color>")]
            Mask3 = 1 << 2,
            [RichLabel("M4")]
            Mask4 = 1 << 3,
            Mask5 = 1 << 4,
        }

        [DefaultExpand, EnumToggleButtons] public BitMask mask;
    }
}
