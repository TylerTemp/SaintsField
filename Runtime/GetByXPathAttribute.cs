using System.Collections.Generic;
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
    public class GetByXPathAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool IsCallback;
        public readonly string Callback;

        public readonly bool AutoResign;
        public readonly bool UseResignButton;
        public readonly bool UseErrorMessage;

#if UNITY_EDITOR
        public readonly IReadOnlyList<XPathStep> XPathSteps;
#endif

        public GetByXPathAttribute(EGetComp config, string ePath, bool isCallback)
        {
            AutoResign = !config.HasFlag(EGetComp.NoAutoResign);
            if (AutoResign)
            {
                UseResignButton = false;
            }
            else
            {
                UseResignButton = !config.HasFlag(EGetComp.NoResignButton);
            }

            if (config.HasFlag(EGetComp.NoMessage))
            {
                UseErrorMessage = false;
            }
            else
            {
                UseErrorMessage = !UseResignButton;
            }

            (string callback, bool actualIsCallback) = RuntimeUtil.ParseCallback(ePath, isCallback);

            Callback = callback;
            IsCallback = actualIsCallback;

#if UNITY_EDITOR
            if(!IsCallback)
            {
                XPathSteps = XPathParser.Parse(ePath).ToArray();
            }
#endif
        }

        public GetByXPathAttribute(string ePath) : this(EGetComp.None, ePath, false)
        {
        }

        public GetByXPathAttribute(string ePath, bool isCallback) : this(EGetComp.None, ePath, isCallback)
        {
        }
    }
}
