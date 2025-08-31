using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    public class HeaderLeftLabelAttribute: HeaderLabelAttribute
    {
        public override bool IsLeft => true;

        public HeaderLeftLabelAttribute(string label = null, string tooltip = null): base(label, tooltip)
        {
        }
    }
}
