using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FieldBelowSeparatorAttribute: FieldSeparatorAttribute
    {
        public FieldBelowSeparatorAttribute(): this(null) {}

        public FieldBelowSeparatorAttribute(EColor color): this(null, color) {}
        public FieldBelowSeparatorAttribute(EColor color, int space): this(null, color, EAlign.Start, false, space) {}
        public FieldBelowSeparatorAttribute(int space): this(null, EColor.Clear, EAlign.Start, false, space) {}

        public FieldBelowSeparatorAttribute(string title, EAlign eAlign, bool isCallback=false, int space=0): this(title, EColor.Gray, eAlign, isCallback, space) {}

        public FieldBelowSeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0): base(title, color, eAlign, isCallback, space, true) {}
    }
}
