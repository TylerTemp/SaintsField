using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayAttribute: Attribute, IPlayaSaintsEditorOnlyAttribute, IPlayaMethodAttribute, ISaintsLayout
    {
        // ReSharper disable InconsistentNaming
        public readonly string Label;
        public readonly ETweenStop DOTweenStop;
        // ReSharper enable InconsistentNaming

        public const string DOTweenPlayGroupBy = "__SAINTSFIELD_DOTWEEN_PLAY__";
        public string LayoutBy { get; }
        public ELayout Layout => 0;
        public bool KeepGrouping { get; }

        public float MarginTop => -1f;
        public float MarginBottom => -1f;
        public float PaddingLeft { get; }
        public float PaddingRight { get; }

        public DOTweenPlayAttribute(string label = null, ETweenStop stopAction = ETweenStop.Rewind, string groupBy="",
            bool keepGrouping = false, float paddingLeft = 0, float paddingRight = 0)
        {
            Label = label;
            DOTweenStop = stopAction;

            LayoutBy = string.IsNullOrEmpty(groupBy)? DOTweenPlayGroupBy: $"{groupBy}/{DOTweenPlayGroupBy}";
            KeepGrouping = keepGrouping;
            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
        }

        public DOTweenPlayAttribute(ETweenStop stopAction, string groupBy=""): this(null, stopAction, groupBy)
        {
        }

        public DOTweenPlayAttribute(string label, string groupBy): this(label, ETweenStop.Rewind, groupBy)
        {
        }
    }
}
