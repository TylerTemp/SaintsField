using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class CurveRangeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly Vector2 Min;
        public readonly Vector2 Max;
        public readonly EColor Color;

        private const EColor DefaultColor = EColor.Lime;

        public CurveRangeAttribute(Vector2 min, Vector2 max, EColor color = DefaultColor)
        {
            Min = min;
            Max = max;
            Color = color;
        }

        public CurveRangeAttribute(EColor color)
            : this(Vector2.zero, Vector2.one, color)
        {
        }

        public CurveRangeAttribute(float minX = 0f, float minY = 0f, float maxX = 1f, float maxY = 1f, EColor color = DefaultColor)
            : this(new Vector2(minX, minY), new Vector2(maxX, maxY), color)
        {
        }
    }
}
