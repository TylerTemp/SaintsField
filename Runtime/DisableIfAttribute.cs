using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;
using SaintsField.Playa;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class DisableIfAttribute: Attribute, IPlayaAttribute
    {
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;

        public DisableIfAttribute(params object[] by)
        {
            ConditionInfos = Parser.Parse(by.Length == 0? new object[]{true}: by).ToArray();
        }
    }
}
