using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class OnValueChangedAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string Callback;

        public OnValueChangedAttribute(string callback)
        {
            Callback = RuntimeUtil.ParseCallback(callback).content;
        }
    }
}
