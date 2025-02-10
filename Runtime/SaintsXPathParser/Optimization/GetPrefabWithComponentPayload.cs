using System;

namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetPrefabWithComponentPayload: OptimizationPayload
    {
        public readonly Type CompType;

        public GetPrefabWithComponentPayload(Type compType)
        {
            CompType = compType;
        }
    }
}
