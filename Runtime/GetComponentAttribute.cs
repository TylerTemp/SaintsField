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
    public class GetComponentAttribute: GetByXPathAttribute
    {
        public override string GroupBy { get; }

        public GetComponentAttribute(Type compType = null, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentExp(EXP.NoPicker | EXP.NoAutoResignToNull));
            ParseXPath(compType);
            GroupBy = groupBy;
        }

        public GetComponentAttribute(EXP exp, Type compType = null, string groupBy = "")
        {
            ParseOptions(exp);
            ParseXPath(compType);
            GroupBy = groupBy;
        }

        private void ParseXPath(Type compType)
        {
            string toParse = GetComponentFilter(compType);

            XPathInfoAndList = new[] {
                new[]
                {
                    new XPathInfo
                    {
                        IsCallback = false,
                        Callback = "",
#if UNITY_EDITOR
                        XPathSteps = XPathParser.Parse(toParse).ToArray(),
#endif
                    },
                }
            };
        }
    }
}
