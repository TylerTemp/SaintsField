using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DrawLineToAttribute: DrawLineAttribute
    {
        public DrawLineToAttribute(
            string target = null, int targetIndex = 0, string targetSpace = "this",
            string space = "this",
            EColor eColor = EColor.White, string color = null
        ) : base(null, 0, space, target, targetIndex, targetSpace, eColor, color)
        {
        }
    }
}
