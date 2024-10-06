using System;
using System.Diagnostics;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ArraySizeAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute, IPlayaArraySizeAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly int Min;
        public readonly int Max;

        public ArraySizeAttribute(int size=-1, int min=-1, int max=-1, string groupBy = "")
        {
            if (size >= 0 && min == -1 && max == -1)
            {
                Min = Max = size;
            }
            else if (size >= 0 && min >= 0 && max == -1)
            {
                Min = size;
                Max = min;
            }
            else
            {
                Min = min;
                Max = max;
            }
            GroupBy = groupBy;
        }
    }
}
