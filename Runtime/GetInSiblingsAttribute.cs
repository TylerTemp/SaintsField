using System;
using System.Diagnostics;
using SaintsField.SaintsXPathParser.Optimization;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetInSiblingsAttribute: GetByXPathAttribute
    {
        public GetInSiblingsAttribute(bool includeInactive = true, Type compType = null): base(ParseXPath(includeInactive, compType))
        {
            OptimizationPayload = new GetInSiblingsPayload(includeInactive, compType);
        }

        public GetInSiblingsAttribute(EXP exp, bool includeInactive = true, Type compType = null): base(exp, ParseXPath(includeInactive, compType))
        {
            OptimizationPayload = new GetInSiblingsPayload(includeInactive, compType);
        }

        private static string ParseXPath(bool includeInactive, Type compType)
        {
            return "../*"
                   + (includeInactive ? "" : "[@{gameObject.activeInHierarchy}]")
                   + GetComponentFilter(compType);
        }
    }
}
