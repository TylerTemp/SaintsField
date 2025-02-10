using System;

namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetComponentInScenePayload: OptimizationPayload
    {
        public readonly Type CompType;
        public readonly bool IncludeInactive;

        public GetComponentInScenePayload(bool includeInactive = false, Type compType = null)
        {
            CompType = compType;
            IncludeInactive = includeInactive;
        }
    }
}
