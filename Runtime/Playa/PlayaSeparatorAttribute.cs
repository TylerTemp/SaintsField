using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaSeparatorAttribute: Attribute, IPlayaAttribute, ISeparatorAttribute
    {
        public string Title { get; }
        public Color Color { get; }
        public EAlign EAlign { get; }
        public bool IsCallback { get; }
        public int Space { get; }
        public bool Below { get; }

        public PlayaSeparatorAttribute(): this(null) {}

        public PlayaSeparatorAttribute(EColor color): this(null, color) {}
        public PlayaSeparatorAttribute(EColor color, int space): this(null, color, EAlign.Start, false, space) {}
        public PlayaSeparatorAttribute(EColor color, bool below): this(null, color, EAlign.Start, false, 0, below) {}
        public PlayaSeparatorAttribute(EColor color, int space, bool below): this(null, color, EAlign.Start, false, space, below) {}

        public PlayaSeparatorAttribute(int space): this(null, EColor.Clear, EAlign.Start, false, space) {}
        public PlayaSeparatorAttribute(int space, bool below): this(null, EColor.Clear, EAlign.Start, false, space, below) {}

        public PlayaSeparatorAttribute(string title, EAlign eAlign, bool isCallback=false, int space=0, bool below=false) : this(title, EColor.Gray, eAlign, isCallback, space, below) {}

        public PlayaSeparatorAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, bool isCallback=false, int space=0, bool below=false)
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
