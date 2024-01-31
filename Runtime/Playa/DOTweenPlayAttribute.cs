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

        public DOTweenPlayAttribute(string label = null, ETweenStop stopAction = ETweenStop.Rewind)
        {
            Label = label;
            DOTweenStop = stopAction;
        }

        public DOTweenPlayAttribute(ETweenStop stopAction): this(null, stopAction)
        {
        }

        public const string DOTweenPlayGroupBy = "__SAINTSFIELD_DOTWEEN_PLAY__";
        public string GroupBy => DOTweenPlayGroupBy;
    }
}
