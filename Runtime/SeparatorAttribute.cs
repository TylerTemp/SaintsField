using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SeparatorAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string Title;
        public readonly EColor Color;
        public readonly EAlign EAlign;
        public readonly bool IsCallback;
        public readonly int Space;

        public readonly bool Below;

        public SeparatorAttribute(): this(null) {}

        public SeparatorAttribute(EColor color): this(null, color) {}
        public SeparatorAttribute(EColor color, int space): this(null, color, EAlign.Start, false, space) {}
        public SeparatorAttribute(EColor color, bool below): this(null, color, EAlign.Start, false, 0, below) {}
        public SeparatorAttribute(EColor color, int space, bool below): this(null, color, EAlign.Start, false, space, below) {}

        public SeparatorAttribute(int space): this(null, EColor.Clear, EAlign.Start, false, space) {}
        public SeparatorAttribute(int space, bool below): this(null, EColor.Clear, EAlign.Start, false, space, below) {}

        public SeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0, bool below=false)
        {
            Title = title;
            Color = color;
            EAlign = eAlign;
            IsCallback = isCallback;
            Space = space;
            Below = below;
        }
    }
}
