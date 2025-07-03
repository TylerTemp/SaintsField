using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaBelowSeparatorAttribute: PlayaSeparatorAttribute
    {
        public PlayaBelowSeparatorAttribute() : base(null, below: true) {}

        public PlayaBelowSeparatorAttribute(EColor color): base(color, below: true) {}
        public PlayaBelowSeparatorAttribute(EColor color, int space): base(color, space, below: true) {}

        public PlayaBelowSeparatorAttribute(int space): base(space, below: true) {}

        public PlayaBelowSeparatorAttribute(string title, EAlign eAlign, bool isCallback=false, int space=0) : base(title, eAlign, isCallback, space, below: true) {}

        public PlayaBelowSeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0): base(title, color, eAlign, isCallback, space, below: true) {}
    }
}
