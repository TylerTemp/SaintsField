using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Condition;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute, IConditions
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }

        public ReadOnlyAttribute(params object[] by)
        {
            ConditionInfos = Parser.Parse(by).ToArray();
        }
    }
}
