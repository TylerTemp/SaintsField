using System;
using System.Collections.Generic;
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
    public class GetComponentInParentAttribute: GetComponentInParentsAttribute
    {
        public override int Limit => 1;

        public override string GroupBy { get; }

        public GetComponentInParentAttribute(Type compType = null, bool excludeSelf = false, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentInParentExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseArguments(compType, excludeSelf);
            GroupBy = groupBy;

            IncludeInactive = true;
            CompType = compType;
            ExcludeSelf = excludeSelf;

            OptimizationPayload = new GetComponentInParentsPayload(true, CompType, excludeSelf, 1);
        }

        public GetComponentInParentAttribute(EXP config, Type compType = null, bool excludeSelf = false, string groupBy = "")
        {
            ParseOptions(config);
            ParseArguments(compType, excludeSelf);
            GroupBy = groupBy;

            IncludeInactive = true;
            CompType = compType;
            ExcludeSelf = excludeSelf;

            OptimizationPayload = new GetComponentInParentsPayload(true, CompType, excludeSelf, 1);
        }

        private void ParseArguments(Type compType, bool excludeSelf)
        {
            string compFilter = GetComponentFilter(compType);
            string sepFilter = compFilter == ""? "": $"/{compFilter}";

            IEnumerable<string> andXPaths = (excludeSelf? new[]{"//parent::"}: new[]{"//parent-or-self::"})
                .Select(each => $"{each}{sepFilter}");

            XPathInfoAndList = andXPaths.Select(ePath =>
            {
                // UnityEngine.Debug.Log($"ePath={ePath}");
                return new[]
                {
                    new XPathInfo
                    {
                        IsCallback = false,
                        Callback = "",
#if UNITY_EDITOR
                        XPathSteps = XPathParser.Parse(ePath).ToArray(),
#endif
                    },
                };
            }).ToArray();
        }
    }
}
