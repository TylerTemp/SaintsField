using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class MinValueAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly float Value;
        public readonly string ValueCallback;

        public MinValueAttribute(float value, string gruopBy = "")
        {
            Value = value;
            ValueCallback = null;

            GroupBy = gruopBy;
        }

        public MinValueAttribute(string valueCallback, string gruopBy = "")
        {
            Value = -1;
            ValueCallback = RuntimeUtil.ParseCallback(valueCallback).content;

            GroupBy = gruopBy;
        }
    }
}
