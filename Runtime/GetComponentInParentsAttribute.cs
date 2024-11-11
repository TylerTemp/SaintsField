using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Utils;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentInParentsAttribute: GetByXPathAttribute
    {
        public override string GroupBy { get; }

        public GetComponentInParentsAttribute(bool includeInactive = false, Type compType = null, bool excludeSelf = false, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentInParentsExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseArguments(includeInactive, compType, excludeSelf);
            GroupBy = groupBy;
        }

        public GetComponentInParentsAttribute(EXP config, bool includeInactive = false, Type compType = null, bool excludeSelf = false, string groupBy = "")
        {
            ParseOptions(config);
            ParseArguments(includeInactive, compType, excludeSelf);
            GroupBy = groupBy;
        }

        private void ParseArguments(bool includeInactive, Type compType, bool excludeSelf)
        {
            string compFilter = GetComponentFilter(compType);
            string activeFilter = includeInactive ? "" : "[@{gameObject.activeSelf}]";
            string allFilter = $"{activeFilter}{compFilter}";
            string sepFilter = allFilter == ""? "": $"/{allFilter}";

            IEnumerable<string> andXPaths = (excludeSelf? new[]{"//ancestor::"}: new[]{"//ancestor-or-self::"})
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
