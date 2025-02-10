using System;

namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetComponentPayload: OptimizationPayload
    {
        public readonly Type CompType;

        public GetComponentPayload(Type compType)
        {
            CompType = compType;
        }
    }
}
