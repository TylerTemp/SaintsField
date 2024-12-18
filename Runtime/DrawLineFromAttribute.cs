using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DrawLineFromAttribute: DrawLineAttribute
    {
        public DrawLineFromAttribute(
            string target = null, int targetIndex = 0, Space targetSpace = Space.World,
            Space space = Space.World,
            EColor color = EColor.White, float colorAlpha = 1f
        ) : base(target, targetIndex, targetSpace, null, 0, space, color, colorAlpha)
        {
        }
    }
}
