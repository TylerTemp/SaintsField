using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderLeftDrawAttribute: HeaderDrawAttribute
    {
        public override bool IsLeft => true;

        public HeaderLeftDrawAttribute(string groupBy = null) : base(groupBy)
        {
        }
    }
}
