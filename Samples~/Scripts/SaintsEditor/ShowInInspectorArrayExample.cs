using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorArrayExample : SaintsMonoBehavior
    {
        // public Color[] colors = {Color.red, Color.green, Color.blue};

        private static readonly Color[] BaseColors = {Color.red, Color.green, Color.blue};

        // [ShowInInspector] private static readonly Color[] StaticReadOnlyField = BaseColors;
        [ShowInInspector] private static readonly Color[][] StaticReadOnlyField2 =
        {
            BaseColors,
            BaseColors,
        };

        // [ShowInInspector] private static  Color[] StaticProp => BaseColors;
        [ShowInInspector] private static Color[][] StaticProp2 => new[] {
            BaseColors,
            BaseColors,
        };
    }
}
