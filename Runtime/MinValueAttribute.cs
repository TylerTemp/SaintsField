using System;
using UnityEngine;

namespace SaintsField
{
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
