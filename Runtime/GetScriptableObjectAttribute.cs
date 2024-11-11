using System.Diagnostics;
using System.Linq;
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
        public GetScriptableObjectAttribute(string pathSuffix=null, string groupBy="")
        {
            ParseOptions(SaintsFieldConfigUtil.GetScriptableObjectExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseXPath(pathSuffix);
            GroupBy = groupBy;
        }

        public GetScriptableObjectAttribute(EXP config, string pathSuffix=null, string groupBy="")
        {
            ParseOptions(config);
            ParseXPath(pathSuffix);
            GroupBy = groupBy;
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
