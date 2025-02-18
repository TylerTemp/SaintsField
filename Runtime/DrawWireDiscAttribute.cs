using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DrawWireDiscAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly EColor EColor;
        public readonly float Radius;
        public readonly string Space;

        public DrawWireDiscAttribute(EColor eColor, float radius, string space = null)
        {
            EColor = eColor;
            Radius = radius;
            Space = space;
        }

        public DrawWireDiscAttribute(float radius, string space = null): this(EColor.White, radius, space)
        {
        }
    }
}
