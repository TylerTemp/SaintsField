#if SAINTSFIELD_SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE
using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SaintsArrowAttribute: OneDirectionBaseAttribute
    {
        public readonly float HeadLength;
        public readonly float HeadAngle;

        public SaintsArrowAttribute(
            string start = null, int startIndex = 0, string startSpace = "this",
            string end = null, int endIndex = 0, string endSpace = "this",
            EColor eColor = EColor.White, float alpha = 1f, string color = null,
            float dotted = -1f,
            float headLength = 0.5f,
            float headAngle = 20.0f
        ): base(start, startIndex, startSpace, end, endIndex, endSpace, eColor, alpha, color, dotted)
        {
            HeadLength = headLength;
            HeadAngle = headAngle;
        }
    }
}
#endif
