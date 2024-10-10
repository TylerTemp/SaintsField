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

        // ReSharper disable once InconsistentNaming
        // public readonly Type CompType;

        public GetComponentAttribute(Type compType = null, string groupBy = "")
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

            ParseOptions(EXP.None);

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
