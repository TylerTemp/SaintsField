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
            string target = null, int targetIndex = 0, string targetSpace = "this",
            string space = "this",
            EColor eColor = EColor.White, string color = null
        ) : base(target, targetIndex, targetSpace, null, 0, space, eColor, color)
        {
        }
    }
}
