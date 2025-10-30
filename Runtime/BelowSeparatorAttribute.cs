using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class BelowSeparatorAttribute: SeparatorAttribute
    {
        public BelowSeparatorAttribute() : base(null, below: true) {}

        public BelowSeparatorAttribute(EColor color): base(color, below: true) {}
        public BelowSeparatorAttribute(EColor color, int space): base(color, space, below: true) {}

        public BelowSeparatorAttribute(int space): base(space, below: true) {}

        public BelowSeparatorAttribute(string title, EAlign eAlign, bool isCallback=false, int space=0) : base(title, eAlign, isCallback, space, below: true) {}

        public BelowSeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0): base(title, color, eAlign, isCallback, space, below: true) {}
    }
}
