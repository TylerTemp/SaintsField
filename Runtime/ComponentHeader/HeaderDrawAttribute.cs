using System;
using System.Diagnostics;
#if SAINTSFIELD_IMPLICIT_USE
using JetBrains.Annotations;
#endif

namespace SaintsField.ComponentHeader
{
#if SAINTSFIELD_IMPLICIT_USE
    [MeansImplicitUse]  // https://github.com/TylerTemp/SaintsField/pull/171#issuecomment-3680042013
#endif
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
