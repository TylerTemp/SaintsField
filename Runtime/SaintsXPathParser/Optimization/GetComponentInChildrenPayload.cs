using System;

namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetComponentInChildrenPayload: OptimizationPayload
    {
        public readonly Type CompType;
        public readonly bool IncludeInactive;
        public readonly bool ExcludeSelf;

        public GetComponentInChildrenPayload(Type compType, bool includeInactive, bool excludeSelf)
        {
            CompType = compType;
            IncludeInactive = includeInactive;
            ExcludeSelf = excludeSelf;
        }
    }
}
