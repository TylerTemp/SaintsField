using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Interfaces;
using SaintsField.SaintsXPathParser;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Wwise
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class GetBankAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool IsCallback;
        public readonly string Callback;

#if UNITY_EDITOR
        public IReadOnlyList<XPathStep> XPathSteps;
        // public override string ToString()
        // {
        //     return XPathSteps == null? Callback: string.Join("/", XPathSteps);
        // }
#endif

        public GetBankAttribute(string bank)
        {
            (Callback, IsCallback) = RuntimeUtil.ParseCallback(bank);
            if (!IsCallback)
            {
#if UNITY_EDITOR
                XPathSteps = XPathParser.Parse(bank).ToArray();
#endif
            }
        }
    }
}
