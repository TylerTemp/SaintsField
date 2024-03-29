using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class CurveRangeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly Vector2 Min;
        public readonly Vector2 Max;
        public readonly EColor Color;

        public CurveRangeAttribute(Vector2 min, Vector2 max, EColor color = EColor.Green)
        {
            Min = min;
            Max = max;
            Color = color;
        }

        public CurveRangeAttribute(EColor color = EColor.Green)
            : this(Vector2.zero, Vector2.one, color)
        {
        }

        public CurveRangeAttribute(float minX = 0f, float minY = 0f, float maxX = 1f, float maxY = 1f, EColor color = EColor.Green)
            : this(new Vector2(minX, minY), new Vector2(maxX, maxY), color)
        {
        }
    }
}
