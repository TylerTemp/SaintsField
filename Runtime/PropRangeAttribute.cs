using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PropRangeAttribute: PropertyAttribute, ISaintsAttribute, IAdaptable
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly string MinCallback;
        public readonly string MaxCallback;
        public readonly double Min;
        public readonly double Max;

        public readonly double Step;
        // ReSharper enable InconsistentNaming

        public PropRangeAttribute(double min, double max, double step=-1f)
        {
            Min = min;
            Max = max;
            MinCallback = null;
            MaxCallback = null;
            Step = step;
        }

        public PropRangeAttribute(string minCallback, string maxCallback, double step=-1f)
        {
            Min = -1;
            Max = -1;
            MinCallback = minCallback;
            MaxCallback = maxCallback;
            Step = step;
        }

        public PropRangeAttribute(string minCallback, double max, double step=-1f)
        {
            MinCallback = minCallback;
            MaxCallback = null;
            Max = max;
            Step = step;
        }

        public PropRangeAttribute(double min, string maxCallback, double step=-1f)
        {
            Min = min;
            MinCallback = null;
            MaxCallback = maxCallback;
            Step = step;
        }
    }
}
