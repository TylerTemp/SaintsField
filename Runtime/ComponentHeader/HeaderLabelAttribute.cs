using System;
using System.Diagnostics;
using JetBrains.Annotations;
using SaintsField.Utils;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    public class HeaderLabelAttribute: AbsComponentHeaderAttribute
    {
        public readonly bool IsCallback;
        public readonly string Label;
        public readonly string Tooltip;
        public override string GroupBy => "";
        public override bool IsLeft => false;

        public HeaderLabelAttribute(string label = null, string tooltip = null)
        {
            (Label, IsCallback) = RuntimeUtil.ParseCallback(label);
            Tooltip = tooltip;
        }
    }
}
