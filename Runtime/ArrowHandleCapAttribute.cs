using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ArrowHandleCapAttribute: OneDirectionBaseAttribute
    {
        public ArrowHandleCapAttribute(
            string start = null, int startIndex = 0, string startSpace = "this",
            string end = null, int endIndex = 0, string endSpace = "this",
            EColor eColor = EColor.White, float alpha = 1f, string color = null,
            float dotted = -1f
        ): base(start, startIndex, startSpace, end, endIndex, endSpace, eColor, alpha, color, dotted)
        {
        }
    }
}
