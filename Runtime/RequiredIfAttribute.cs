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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class RequiredIfAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }

        public RequiredIfAttribute(params object[] andCallbacks)
        {
            ConditionInfos = andCallbacks.Length > 0
                ? Parser.Parse(andCallbacks).ToArray()
                : Parser.Parse(new object[]{true}).ToArray();
        }
    }
}
