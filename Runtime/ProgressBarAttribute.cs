using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ProgressBarAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly float Min;
        public readonly string MinCallback;
        public readonly float Max;
        public readonly string MaxCallback;
        public readonly float Step;

        public readonly EColor Color;
        public readonly string ColorCallback;

        public readonly EColor BackgroundColor;
        public readonly string BackgroundColorCallback;

        public readonly string TitleCallback;
        // ReSharper enable InconsistentNaming

        public ProgressBarAttribute(): this(0, 100, -1) { }
        public ProgressBarAttribute(EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null)
            : this(0, 100, -1, color, backgroundColor, colorCallback, backgroundColorCallback) { }

        public ProgressBarAttribute(float max): this(0, max, -1) { }
        public ProgressBarAttribute(float max, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null)
            : this(0, max, -1, color, backgroundColor, colorCallback, backgroundColorCallback) { }

        public ProgressBarAttribute(float max, float step): this(0, max, step) { }

        public ProgressBarAttribute(string maxCallback): this(0, maxCallback) { }
        public ProgressBarAttribute(string maxCallback, float step): this(0, maxCallback, step) { }

        public ProgressBarAttribute(string maxCallback = null, float step = -1, EColor color = EColor.OceanicSlate,
            EColor backgroundColor = EColor.CharcoalGray, string colorCallback = null,
            string backgroundColorCallback = null)
            : this(0, maxCallback, step, color, backgroundColor, colorCallback, backgroundColorCallback)
        {
        }

        public ProgressBarAttribute(float max=100, float step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null)
            : this(0, max, step, color, backgroundColor, colorCallback, backgroundColorCallback)
        {
        }

        public ProgressBarAttribute(float min=0, float max=100, float step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
        {
            Min = min;
            MinCallback = null;
            Max = max;
            MaxCallback = null;
            Step = step;
            Color = color;
            ColorCallback = colorCallback;
            BackgroundColor = backgroundColor;
            BackgroundColorCallback = backgroundColorCallback;
            TitleCallback = titleCallback;
        }

        public ProgressBarAttribute(float min=0, string maxCallback=null, float step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
        {
            Min = min;
            MinCallback = null;
            // Max = max;
            MaxCallback = maxCallback;
            Step = step;
            Color = color;
            ColorCallback = colorCallback;
            BackgroundColor = backgroundColor;
            BackgroundColorCallback = backgroundColorCallback;
            TitleCallback = titleCallback;
        }

        public ProgressBarAttribute(string minCallback=null, float max=100, float step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
        {
            // Min = min;
            MinCallback = minCallback;
            Max = max;
            MaxCallback = null;
            Step = step;
            Color = color;
            ColorCallback = colorCallback;
            BackgroundColor = backgroundColor;
            BackgroundColorCallback = backgroundColorCallback;
            TitleCallback = titleCallback;
        }

        public ProgressBarAttribute(string minCallback=null, string maxCallback=null, float step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
        {
            // Min = min;
            MinCallback = minCallback;
            // Max = max;
            MaxCallback = maxCallback;
            Step = step;
            Color = color;
            ColorCallback = colorCallback;
            BackgroundColor = backgroundColor;
            BackgroundColorCallback = backgroundColorCallback;
            TitleCallback = titleCallback;
        }
    }
}
