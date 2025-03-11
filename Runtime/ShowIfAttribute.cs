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
    public class ShowIfAttribute: PropertyAttribute, ISaintsAttribute, IVisibilityAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";

        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        public virtual bool IsShow => true;

        public ShowIfAttribute(params object[] andCallbacks)
        {
            ConditionInfos = andCallbacks.Length > 0
                ? Parser.Parse(andCallbacks).ToArray()
                : Parser.Parse(new object[]{true}).ToArray();
        }
    }
}
