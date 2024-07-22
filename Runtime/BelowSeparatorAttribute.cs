using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class BelowSeparatorAttribute: SeparatorAttribute
    {
        public BelowSeparatorAttribute(): this(null) {}

        public BelowSeparatorAttribute(EColor color): this(null, color) {}
        public BelowSeparatorAttribute(EColor color, int space): this(null, color, EAlign.Start, false, space) {}
        public BelowSeparatorAttribute(int space): this(null, EColor.Clear, EAlign.Start, false, space) {}

        public BelowSeparatorAttribute(string title, EAlign eAlign, bool isCallback=false, int space=0, bool below=false): this(title, EColor.Gray, eAlign, isCallback, space) {}

        public BelowSeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0): base(title, color, eAlign, isCallback, space, true) {}
    }
}
