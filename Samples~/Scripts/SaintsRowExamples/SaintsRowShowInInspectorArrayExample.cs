using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsRowExamples
{
    public class SaintsRowShowInInspectorArrayExample : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            private static readonly Color[] BaseColors = {Color.red, Color.green, Color.blue};
            private static readonly Color[][] BaseColors2 = {BaseColors, BaseColors};

            private static readonly List<Color> LisBaseColors = new List<Color>{Color.red, Color.green, Color.blue};
            private static readonly List<List<Color>> LisBaseColors2 = new List<List<Color>>{LisBaseColors, LisBaseColors};

            [Ordered] public Color NormalColor1;

            [Ordered][ShowInInspector] private static readonly Color[] StaticReadOnlyField = BaseColors;
            [Ordered][ShowInInspector] private static readonly Color[][] StaticReadOnlyField2 =
            {
                BaseColors,
                BaseColors,
            };

            [Ordered][ShowInInspector] private static readonly List<Color> LisStaticReadOnlyField = LisBaseColors;
            [Ordered][ShowInInspector] private static readonly List<List<Color>> LisStaticReadOnlyField2 = LisBaseColors2;

            [Ordered][ShowInInspector] private static Color[] StaticProp => BaseColors;
            [Ordered][ShowInInspector] private static Color[][] StaticProp2 => BaseColors2;

            [Ordered][ShowInInspector] private static List<Color> LisStaticProp => LisBaseColors;
            // [Ordered][ShowInInspector] private static List<List<Color>> LisStaticProp2 => LisBaseColors2;
            [Ordered] public Color NormalColor2;
        }

        [SaintsRow] public MyStruct myStruct;
    }
}
