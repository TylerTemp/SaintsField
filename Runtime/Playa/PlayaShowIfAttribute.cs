using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;
using SaintsField.Interfaces;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaShowIfAttribute: Attribute, IPlayaAttribute, IVisibilityAttribute
    {
        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        public virtual bool IsShow => true;

        public PlayaShowIfAttribute(params object[] andCallbacks)
        {
            ConditionInfos = andCallbacks.Length == 0
                ? Parser.Parse(new object[]{true}).ToArray()
                : Parser.Parse(andCallbacks).ToArray();
        }
    }
}
