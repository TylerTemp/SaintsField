using UnityEngine;

namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class SpriteToggleAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string CompName;

        public SpriteToggleAttribute(string imageOrSpriteRenderer)
        {
            CompName = imageOrSpriteRenderer;
        }
    }
}
