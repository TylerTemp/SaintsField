using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DrawLineAttribute: OneDirectionBaseAttribute
    {
        public DrawLineAttribute(
            string start = null, int startIndex = 0, Space startSpace = Space.World,
            string end = null, int endIndex = 0, Space endSpace = Space.World,
            EColor color = EColor.White, float colorAlpha = 1f
        ): base(start, startIndex, startSpace, end, endIndex, endSpace, color, colorAlpha)
        {
        }
    }
}
