using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class SepTitleAttribute : PropertyAttribute, ISeparatorAttribute
    {
        public string Title { get; }
        public Color Color { get; }
        public EAlign EAlign { get; }
        public bool IsCallback { get; }
        public int Space { get; }
        public bool Below { get; }

        public SepTitleAttribute(): this(null) {}

        public SepTitleAttribute(EColor color): this(null, color) {}
        public SepTitleAttribute(EColor color, int space): this(null, color, EAlign.Start, space) {}
        public SepTitleAttribute(int space): this(null, EColor.Clear, EAlign.Start, space) {}
        public SepTitleAttribute(string title, EAlign eAlign, int space=0) : this(title, EColor.Gray, eAlign, space) {}
        public SepTitleAttribute(string title, EColor color=EColor.Gray, EAlign eAlign=EAlign.Start, int space=0)
        {
            Debug.Assert(eAlign != EAlign.FieldStart, $"{EAlign.FieldStart} is not supported");

            // (string titleParsed, bool titleIsCallback) = RuntimeUtil.ParseCallback(title, isCallback);

            Title = title;
            IsCallback = false;
            Color = color.GetColor();
            EAlign = eAlign;
            Space = space;
            Below = false;
        }
    }
}
