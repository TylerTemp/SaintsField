using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.SaintsXPathParser.Optimization;
using SaintsField.Utils;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetPrefabWithComponentAttribute: GetByXPathAttribute
    {
        public override string GroupBy { get; }

        // ReSharper disable once NotAccessedField.Global
        public readonly Type CompType;

        public GetPrefabWithComponentAttribute(Type compType = null, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetPrefabWithComponentExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseXPath(compType);
            GroupBy = groupBy;

            CompType = compType;

            OptimizationPayload = new GetPrefabWithComponentPayload(compType);
        }

        public GetPrefabWithComponentAttribute(EXP config, Type compType = null, string groupBy = "")
        {
            ParseOptions(config);
            ParseXPath(compType);
            GroupBy = groupBy;

            OptimizationPayload = new GetPrefabWithComponentPayload(compType);
        }

        private void ParseXPath(Type compType)
        {
            string compFilter = GetComponentFilter(compType);
            // string sepFilter = compFilter == ""? "": $"/{compFilter}";

            string ePath = $"assets:://*.prefab{compFilter}";

            XPathInfoAndList = new[]
            {
                new[]
                {
                    new XPathInfo
                    {
                        IsCallback = false,
                        Callback = "",
#if UNITY_EDITOR
                        XPathSteps = XPathParser.Parse(ePath).ToArray(),
#endif
                    },
                },
            };

        }
    }
}
