using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetByXPathAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool IsCallback;
        public readonly string Callback;

        public readonly bool ForceResign;
        public readonly bool ResignButton = true;
        public readonly bool ErrorMessage = true;

#if UNITY_EDITOR
        public readonly IReadOnlyList<XPathStep> XPathSteps;
#endif

        public GetByXPathAttribute(EGetComp config, string ePath)
        {
            ForceResign = config.HasFlag(EGetComp.ForceResign);
            ResignButton = !config.HasFlag(EGetComp.NoResignButton);
            ErrorMessage = !config.HasFlag(EGetComp.NoMessage);

#if UNITY_EDITOR
            XPathSteps = XPathParser.Parse(ePath).ToArray();
#endif
        }

        public GetByXPathAttribute(string ePath) : this(EGetComp.ForceResign, ePath)
        {
        }
    }
}
