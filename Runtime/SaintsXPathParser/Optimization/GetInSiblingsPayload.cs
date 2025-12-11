using System;

namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetInSiblingsPayload: OptimizationPayload
    {
        public readonly bool IncludeInactive;
        public readonly Type CompType;

        public GetInSiblingsPayload(bool includeInactive, Type compType)
        {
            IncludeInactive = includeInactive;
            CompType = compType;
        }
    }
}
