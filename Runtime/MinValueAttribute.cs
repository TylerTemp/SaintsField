using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class MinValueAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly float Value;
        public readonly string ValueCallback;

        public MinValueAttribute(float value)
        {
            Value = value;
            ValueCallback = null;
        }

        public MinValueAttribute(string valueCallback)
        {
            Value = -1;
            ValueCallback = valueCallback;
        }
    }
}
