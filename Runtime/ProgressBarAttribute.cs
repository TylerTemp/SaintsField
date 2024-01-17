using UnityEngine;

namespace SaintsField
{
    public class ProgressBarAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly double Min;
        public readonly string MinCallback;
        public readonly double Max;
        public readonly string MaxCallback;
        public readonly double Step;

        public readonly EColor Color;
        public readonly string ColorCallback;

        public readonly EColor BackgroundColor;
        public readonly string BackgroundColorCallback;

        public readonly string TitleCallback;

        public ProgressBarAttribute(): this(0, 100, -1) { }
        public ProgressBarAttribute(double max): this(0, max, -1) { }
        public ProgressBarAttribute(double max, double step): this(0, max, step) { }

        public ProgressBarAttribute(string maxCallback): this(0, maxCallback) { }
        public ProgressBarAttribute(string maxCallback, double step): this(0, maxCallback, step) { }

        public ProgressBarAttribute(string maxCallback = null, double step = -1, EColor color = EColor.OceanicSlate,
            EColor backgroundColor = EColor.CharcoalGray, string colorCallback = null,
            string backgroundColorCallback = null)
            : this(0, maxCallback, step, color, backgroundColor, colorCallback, backgroundColorCallback)
        {
        }

        public ProgressBarAttribute(double max=100, double step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null)
            : this(0, max, step, color, backgroundColor, colorCallback, backgroundColorCallback)
        {
        }

        public ProgressBarAttribute(double min=0, double max=100, double step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
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

        public ProgressBarAttribute(double min=0, string maxCallback=null, double step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
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

        public ProgressBarAttribute(string minCallback=null, double max=100, double step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
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

        public ProgressBarAttribute(string minCallback=null, string maxCallback=null, double step=-1, EColor color=EColor.OceanicSlate, EColor backgroundColor=EColor.CharcoalGray, string colorCallback=null, string backgroundColorCallback=null, string titleCallback=null)
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
