using System;
using System.Diagnostics;
using System.Linq;
using SaintsField.SaintsXPathParser.Optimization;
using SaintsField.Utils;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentAttribute: GetByXPathAttribute
    {
        public override string GroupBy { get; }

        // public readonly Type CompType;
        // ReSharper disable once InconsistentNaming
        public new const EXP DefaultEXP = EXP.NoPicker | EXP.NoAutoResignToNull;

        public GetComponentAttribute(Type compType = null, string groupBy = "")
        {
            ParseOptions(SaintsFieldConfigUtil.GetComponentExp(DefaultEXP));
            ParseXPath(compType);
            GroupBy = groupBy;

            OptimizationPayload = new GetComponentPayload(compType);
        }

        public GetComponentAttribute(EXP exp, Type compType = null, string groupBy = "")
        {
            ParseOptions(exp);
            ParseXPath(compType);
            // CompType = compType;
            GroupBy = groupBy;

            OptimizationPayload = new GetComponentPayload(compType);
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
                },
            };
        }
    }
}
