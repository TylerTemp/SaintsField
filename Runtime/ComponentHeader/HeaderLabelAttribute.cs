using System;
using System.Diagnostics;
#if SAINTSFIELD_IMPLICIT_USE
using JetBrains.Annotations;
#endif
using SaintsField.Utils;

namespace SaintsField.ComponentHeader
{
#if SAINTSFIELD_IMPLICIT_USE
    [MeansImplicitUse]  // https://github.com/TylerTemp/SaintsField/pull/171#issuecomment-3680042013
#endif
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
