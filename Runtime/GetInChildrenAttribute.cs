using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetInChildrenAttribute: GetComponentInChildrenAttribute
    {
        public GetInChildrenAttribute(Type compType = null, bool includeInactive = true, string groupBy = "")
        : base(includeInactive, compType, true, groupBy)
        {
        }

        public GetInChildrenAttribute(EXP config, Type compType = null, bool includeInactive = true, string groupBy = "")
        : base(config, includeInactive, compType, true, groupBy)
        {
        }
    }
}
