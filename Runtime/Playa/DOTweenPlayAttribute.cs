using System;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayAttribute: Attribute, ISaintsMethodAttribute, ISaintsGroup
    {
        // ReSharper disable InconsistentNaming
        public readonly string Label;
        public readonly ETweenStop DOTweenStop;
        // ReSharper enable InconsistentNaming

        public DOTweenPlayAttribute(string label = null, ETweenStop stopAction = ETweenStop.Rewind, string groupBy="")
        {
            Label = label;
            DOTweenStop = stopAction;

            GroupBy = string.IsNullOrEmpty(groupBy)? DOTweenPlayGroupBy: $"{groupBy}/{DOTweenPlayGroupBy}";
        }

        public DOTweenPlayAttribute(ETweenStop stopAction, string groupBy=""): this(null, stopAction, groupBy)
        {
        }

        public const string DOTweenPlayGroupBy = "__SAINTSFIELD_DOTWEEN_PLAY__";
        public string GroupBy { get; }
        public ELayout Layout => 0;
    }
}
