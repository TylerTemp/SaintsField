using System;

namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetComponentInParentsPayload: OptimizationPayload
    {
        public readonly int Limit;

        public readonly bool IncludeInactive;
        public readonly Type CompType;
        public readonly bool ExcludeSelf;

        public GetComponentInParentsPayload(bool includeInactive, Type compType, bool excludeSelf, int limit)
        {
            IncludeInactive = includeInactive;
            CompType = compType;
            ExcludeSelf = excludeSelf;
            Limit = limit;
        }
    }
}
