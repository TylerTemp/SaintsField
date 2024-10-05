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
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class GetByXPathAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool InitSign;
        public readonly bool AutoResign;
        public readonly bool UseResignButton;
        public readonly bool UsePickerButton;
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

        public GetByXPathAttribute(EXP config, params string[] ePaths)
        {
            InitSign = !config.HasFlag(EXP.NoInitSign);
            UsePickerButton = !config.HasFlag(EXP.NoPicker);
            AutoResign = !config.HasFlag(EXP.NoAutoResign);
            if (AutoResign)
            {
                UseResignButton = false;
            }
            else
            {
                UseResignButton = !config.HasFlag(EXP.NoResignButton);
            }

            if (config.HasFlag(EXP.NoMessage))
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

        public GetByXPathAttribute(params string[] ePaths) : this(EXP.None, ePaths)
        {
        }
    }
}
