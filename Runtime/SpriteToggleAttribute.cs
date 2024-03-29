using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class SpriteToggleAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        // ReSharper disable once InconsistentNaming
        public readonly string CompName;

        public SpriteToggleAttribute(string imageOrSpriteRenderer=null, string groupBy = "")
        {
            CompName = imageOrSpriteRenderer;
            GroupBy = groupBy;
        }
    }
}
