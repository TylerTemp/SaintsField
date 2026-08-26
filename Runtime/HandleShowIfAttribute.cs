using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public abstract class HandleShowIfAttribute: PropertyAttribute, ISaintsAttribute
    {
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public virtual bool IsShow => true;

        protected HandleShowIfAttribute(EMode eMode, params object[] andCallbacks)
        {
            ConditionInfos = Parser.Parse(andCallbacks.Prepend(eMode).ToArray()).ToArray();
        }

        protected HandleShowIfAttribute(params object[] andCallbacks)
        {
            ConditionInfos = andCallbacks.Length > 0
                ? Parser.Parse(andCallbacks).ToArray()
                : Parser.Parse(new object[]{true}).ToArray();
        }

        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";
    }
}
