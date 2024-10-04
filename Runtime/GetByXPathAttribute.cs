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

        public readonly bool AutoResign;
        public readonly bool UseResignButton;
        public readonly bool UseErrorMessage;

        public struct XPathInfo
        {
            public bool IsCallback;
            public string Callback;
#if UNITY_EDITOR
            public IReadOnlyList<XPathStep> XPathSteps;
#endif
        }

        public readonly IReadOnlyList<XPathInfo> XPathInfoList;

        public GetByXPathAttribute(EGetComp config, params string[] ePaths)
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

            XPathInfoList = ePaths.Length == 0
                ? new[]
                {
                    new XPathInfo
                    {
                        IsCallback = false,
                        Callback = "",
#if UNITY_EDITOR
                        XPathSteps = XPathParser.Parse(".").ToArray(),
#endif
                    },
                }
                : ePaths
                    .Select(ePath =>
                    {
                        (string callback, bool actualIsCallback) = RuntimeUtil.ParseCallback(ePath, false);

                        return new XPathInfo
                        {
                            IsCallback = actualIsCallback,
                            Callback = callback,
    #if UNITY_EDITOR
                            XPathSteps = XPathParser.Parse(ePath).ToArray(),
    #endif
                        };
                    })
                    .ToArray();
        }

        public GetByXPathAttribute(params string[] ePaths) : this(EGetComp.None, ePaths)
        {
        }
    }
}
