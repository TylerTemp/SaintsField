using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderDrawAttribute: AbsComponentHeaderAttribute
    {
        public override string GroupBy { get; }
        public override bool IsLeft => false;

        public HeaderDrawAttribute(string groupBy = null)
        {
            GroupBy = groupBy;
        }
    }
}
