using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorArrayExample : SaintsMonoBehavior
    {
        public Color[] colors = {Color.red, Color.green, Color.blue};

        [ShowInInspector] private static readonly Color[] StaticReadOnlyField = {Color.red, Color.green, Color.blue};
        [ShowInInspector] private static readonly Color[][] StaticReadOnlyField2 =
        {
            StaticReadOnlyField,
            StaticReadOnlyField,
        };

        [ShowInInspector] private Color[] StaticProp => StaticReadOnlyField;
        [ShowInInspector] private Color[][] StaticProp2 => new[] {
            StaticReadOnlyField,
            StaticReadOnlyField,
        };
    }
}
