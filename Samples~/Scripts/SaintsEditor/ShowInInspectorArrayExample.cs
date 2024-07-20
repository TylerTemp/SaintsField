using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorArrayExample : SaintsMonoBehaviour
    {
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
