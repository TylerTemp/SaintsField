using System;
using System.Diagnostics;
using JetBrains.Annotations;
using SaintsField.Utils;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderButtonAttribute: AbsComponentHeaderAttribute
    {
        public readonly string Label;
        public readonly bool IsCallback;
        public readonly string Tooltip;

        public virtual bool IsGhost => false;

        public override string GroupBy => "";
        public override bool IsLeft => false;

        public HeaderButtonAttribute(string label = null, string tooltip = null)
        {
            (Label, IsCallback) = RuntimeUtil.ParseCallback(label);
            Tooltip = tooltip;
        }
    }
}
