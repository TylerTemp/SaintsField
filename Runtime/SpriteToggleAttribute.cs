using UnityEngine;

namespace SaintsField
{
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
