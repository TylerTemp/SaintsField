using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;
using System;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorArrayExample : SaintsMonoBehaviour
    {
        [Serializable]
        public enum EnumN {
            One,
            Two,
            Three,
        }

        [ShowInInspector] private EnumN em;

        [Serializable, Flags]
        public enum EnumF {
            One = 1,
            Two = 1 << 1,
            Three = 1 << 2,
        }

        [ShowInInspector] private EnumF ef;

        private static readonly Color[] BaseColors = {Color.red, Color.green, Color.blue};
        private static readonly Color[][] BaseColors2 = {BaseColors, BaseColors};

        private static readonly List<Color> LisBaseColors = new List<Color>{Color.red, Color.green, Color.blue};
        private static readonly List<List<Color>> LisBaseColors2 = new List<List<Color>>{LisBaseColors, LisBaseColors};

        [ShowInInspector] private static readonly Color[] StaticReadOnlyField = BaseColors;
        [ShowInInspector] private static readonly Color[][] StaticReadOnlyField2 =
        {
            BaseColors,
            BaseColors,
        };

        [ShowInInspector] private static readonly List<Color> LisStaticReadOnlyField = LisBaseColors;
        [ShowInInspector] private static readonly List<List<Color>> LisStaticReadOnlyField2 = LisBaseColors2;

        [ShowInInspector] private static Color[] StaticProp => BaseColors;
        [ShowInInspector] private static Color[][] StaticProp2 => BaseColors2;

        [ShowInInspector] private static List<Color> LisStaticProp => LisBaseColors;
        [ShowInInspector] private static List<List<Color>> LisStaticProp2 => LisBaseColors2;
    }
}
