using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsXPathParser;
using UnityEngine;

namespace SaintsField
{
    public class SaintsPathAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool IsCallback;
        public readonly string Callback;

        public readonly IReadOnlyList<XPathStep> XPathSteps;

        public SaintsPathAttribute(string ePath)
        {
            XPathSteps = XPathParser.Parse(ePath).ToArray();
        }
    }
}
