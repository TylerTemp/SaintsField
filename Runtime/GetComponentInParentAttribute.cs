using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentInParentAttribute: GetComponentInParentsAttribute
    {
        public override int Limit => 1;

        public GetComponentInParentAttribute(Type compType = null, bool excludeSelf = false, string groupBy = "")
            : base(true, compType, excludeSelf, groupBy)
        {
        }
    }
}
