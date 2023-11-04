using UnityEngine;

namespace SaintsField
{
    public class FieldDrawerConfigAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public enum FieldDrawType
        {
            Inline,
            FullWidthNewLine,
            FullWidthOverlay,
        }

        public readonly FieldDrawType FieldDraw;
        // public readonly int IndentLevelIncr;

        public FieldDrawerConfigAttribute(FieldDrawType fieldDrawType)
        {
            FieldDraw = fieldDrawType;
            // IndentLevelIncr = indentLevelIncr;
        }
    }
}
