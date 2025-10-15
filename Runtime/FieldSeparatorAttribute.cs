using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FieldSeparatorAttribute : PropertyAttribute, ISaintsAttribute, ISeparatorAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public string Title { get; }
        public Color Color { get; }
        public EAlign EAlign { get; }
        public bool IsCallback { get; }
        public int Space { get; }

        public bool Below { get; }

        public FieldSeparatorAttribute(): this(null) {}

        public FieldSeparatorAttribute(EColor color): this(null, color) {}
        public FieldSeparatorAttribute(EColor color, int space): this(null, color, EAlign.Start, false, space) {}
        public FieldSeparatorAttribute(EColor color, bool below): this(null, color, EAlign.Start, false, 0, below) {}
        public FieldSeparatorAttribute(EColor color, int space, bool below): this(null, color, EAlign.Start, false, space, below) {}

        public FieldSeparatorAttribute(int space): this(null, EColor.Clear, EAlign.Start, false, space) {}
        public FieldSeparatorAttribute(int space, bool below): this(null, EColor.Clear, EAlign.Start, false, space, below) {}

        public FieldSeparatorAttribute(string title, EAlign eAlign, bool isCallback=false, int space=0, bool below=false) : this(title, EColor.Gray, eAlign, isCallback, space, below) {}

        public FieldSeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0, bool below=false)
        {
            Debug.Assert(eAlign != EAlign.FieldStart, $"{EAlign.FieldStart} is not supported");

            (string titleParsed, bool titleIsCallback) = RuntimeUtil.ParseCallback(title, isCallback);

            Title = titleParsed;
            IsCallback = titleIsCallback;
            Color = color.GetColor();
            EAlign = eAlign;
            Space = space;
            Below = below;
        }
    }
}
