using System;

namespace SaintsField
{
    public class GetComponentInParentAttribute: GetComponentInParentsAttribute
    {
        public override int Limit => 1;

        public GetComponentInParentAttribute(Type compType = null, string groupBy = ""): base(compType, groupBy)
        {
        }
    }
}
