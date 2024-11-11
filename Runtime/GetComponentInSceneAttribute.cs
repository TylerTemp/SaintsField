using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.Utils;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentInSceneAttribute: GetByXPathAttribute
    {
        public override string GroupBy { get; }

        public GetComponentInSceneAttribute(bool includeInactive = false, Type compType = null, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentInSceneExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseArguments(includeInactive, compType);
            GroupBy = groupBy;
        }

        private void ParseArguments(bool includeInactive, Type compType)
        {
            string compFilter = GetComponentFilter(compType);
            string activeFilter = includeInactive ? "" : "[@{gameObject.activeSelf}]";
            string allFilter = $"{activeFilter}{compFilter}";
            // string sepFilter = allFilter == ""? "": $"/{allFilter}";

            string ePath = $"scene:://*{allFilter}";
            // UnityEngine.Debug.Log(ePath);


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
