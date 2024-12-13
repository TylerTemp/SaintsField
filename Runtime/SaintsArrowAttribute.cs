using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class SaintsArrowAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string StartTarget;
        public readonly int StartIndex;
        public readonly string EndTarget;
        public readonly int EndIndex;
        public readonly Space Space;
        public readonly EColor EColor;

        public SaintsArrowAttribute(string startTarget = null, int startIndex = 0, string endTarget = null, int endIndex = 0, Space space = Space.World, EColor color = EColor.White)
        {
            StartTarget = startTarget;
            StartIndex = startIndex;
            EndTarget = endTarget;
            EndIndex = endIndex;
            Space = space;
            EColor = color;
        }
    }
}
