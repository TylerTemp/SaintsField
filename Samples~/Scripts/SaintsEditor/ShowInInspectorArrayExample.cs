using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorArrayExample : SaintsMonoBehavior
    {
        private static readonly Color[] BaseColors = {Color.red, Color.green, Color.blue};
        private static readonly Color[][] BaseColors2 = {BaseColors, BaseColors};

        [ShowInInspector] private static readonly Color[] StaticReadOnlyField = BaseColors;
        [ShowInInspector] private static readonly Color[][] StaticReadOnlyField2 =
        {
            BaseColors,
            BaseColors,
        };

        [ShowInInspector] private static  Color[] StaticProp => BaseColors;
        [ShowInInspector] private static Color[][] StaticProp2 => BaseColors2;
    }
}
