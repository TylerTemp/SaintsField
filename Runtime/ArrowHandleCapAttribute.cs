using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ArrowHandleCapAttribute: OneDirectionBaseAttribute
    {
        public ArrowHandleCapAttribute(
            string start = null, int startIndex = 0, string startSpace = "this",
            string end = null, int endIndex = 0, string endSpace = "this",
            EColor eColor = EColor.White, string color = null
        ): base(start, startIndex, startSpace, end, endIndex, endSpace, eColor, color)
        {
        }
    }
}
