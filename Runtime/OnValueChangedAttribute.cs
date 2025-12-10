using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class OnValueChangedAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute
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
