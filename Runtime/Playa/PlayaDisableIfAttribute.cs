using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaDisableIfAttribute: Attribute, IPlayaAttribute
    {
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;

        public PlayaDisableIfAttribute(params object[] by)
        {
            ConditionInfos = Parser.Parse(by).ToArray();
        }

        public override string ToString()
        {
            return $"<PlayaDisableIf conditions={string.Join(", ", ConditionInfos)}>";
        }
    }
}
