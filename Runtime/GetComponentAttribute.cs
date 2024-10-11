using System;
using System.Diagnostics;
using System.Linq;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentAttribute: GetByXPathAttribute
    {
        public override string GroupBy => _groupBy;
        private string _groupBy;

        public GetComponentAttribute(Type compType = null, string groupBy = "")
        {
            ParseOptions(EXP.None);
            ParseXPath(compType);
            _groupBy = groupBy;
        }

        public GetComponentAttribute(EXP exp, Type compType = null, string groupBy = "")
        {
            ParseOptions(exp);
            ParseXPath(compType);
            _groupBy = groupBy;
        }

        private void ParseXPath(Type compType)
        {
            string toParse;
            if (compType == null)
            {
                toParse = ".";
            }
            else
            {
                string nameSpace = compType.Namespace;
                string typeName = compType.Name;
                toParse = $"[@GetComponent({nameSpace}.{typeName})]";
            }

            XPathInfoAndList = new[] { new[]
            {
                new XPathInfo
                {
                    IsCallback = false,
                    Callback = "",
#if UNITY_EDITOR
                    XPathSteps = XPathParser.Parse(toParse).ToArray(),
#endif
                },
            } };
        }
    }
}
