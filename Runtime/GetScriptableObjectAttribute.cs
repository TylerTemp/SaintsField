using System.Diagnostics;
using System.Linq;
using SaintsField.SaintsXPathParser.Optimization;
using SaintsField.Utils;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetScriptableObjectAttribute: GetByXPathAttribute
    {
        public override string GroupBy { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly string PathSuffix;

        public GetScriptableObjectAttribute(string pathSuffix=null, string groupBy="")
        {
            PathSuffix = string.IsNullOrEmpty(pathSuffix)? null: pathSuffix + ".asset";

            ParseOptions(SaintsFieldConfigUtil.GetScriptableObjectExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseXPath(pathSuffix);
            GroupBy = groupBy;

            OptimizationPayload = new GetScriptableObjectPayload(PathSuffix);
        }

        public GetScriptableObjectAttribute(EXP config, string pathSuffix=null, string groupBy="")
        {
            ParseOptions(config);
            ParseXPath(pathSuffix);
            GroupBy = groupBy;

            OptimizationPayload = new GetScriptableObjectPayload(null);
        }

        private void ParseXPath(string pathSuffix)
        {
            string pathMatch = string.IsNullOrEmpty(pathSuffix)? "*.asset": $"*{pathSuffix}.asset";

            string ePath = $"assets:://{pathMatch}";

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
