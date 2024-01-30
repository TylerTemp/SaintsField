using System;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayAttribute: Attribute, ISaintsMethodAttribute, ISaintsGroup
    {
        // ReSharper disable InconsistentNaming
        public readonly string Label;
        public readonly EDOTweenStop DOTweenStop;
        // ReSharper enable InconsistentNaming

        public DOTweenPlayAttribute(string label = null, EDOTweenStop stopAction = EDOTweenStop.Rewind)
        {
            Label = label;
            DOTweenStop = stopAction;
        }

        public const string DOTweenPlayGroupBy = "__SAINTSFIELD_DOTWEEN_PLAY__";
        public string GroupBy => DOTweenPlayGroupBy;
    }
}
