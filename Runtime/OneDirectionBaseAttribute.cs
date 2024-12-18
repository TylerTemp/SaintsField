using UnityEngine;

namespace SaintsField
{
    public abstract class OneDirectionBaseAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string Start;
        public readonly int StartIndex;
        public readonly Space StartSpace;
        public readonly string End;
        public readonly int EndIndex;
        public readonly Space EndSpace;
        public readonly EColor EColor;
        public readonly float ColorAlpha;

        public OneDirectionBaseAttribute(
            string start = null, int startIndex = 0, Space startSpace = Space.World,
            string end = null, int endIndex = 0, Space endSpace = Space.World,
            EColor color = EColor.White, float colorAlpha = 1f,
            float headLength = 0.5f,
            float headAngle = 20.0f
        )
        {
            Start = start;
            StartIndex = startIndex;
            StartSpace = startSpace;
            End = end;
            EndIndex = endIndex;
            EndSpace = endSpace;

            EColor = color;
            ColorAlpha = colorAlpha;
        }
    }
}
