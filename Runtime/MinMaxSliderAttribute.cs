using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class MinMaxSliderAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly float Min;
        public readonly string MinCallback;
        public readonly float Max;
        public readonly string MaxCallback;
        public readonly float Step;

        public readonly float MinWidth;
        public readonly float MaxWidth;

        public readonly bool FreeInput;
        // ReSharper enable InconsistentNaming

        private const float DefaultWidth = 50f;

        public MinMaxSliderAttribute(float min, float max, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth, bool free=false)
        {
            Min = min;
            Max = max;
            MinCallback = null;
            MaxCallback = null;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
            FreeInput = free;
        }

        public MinMaxSliderAttribute(int min, int max, int step=-1, float minWidth=DefaultWidth, float maxWidth=DefaultWidth, bool free=false)
        {
            Min = min;
            Max = max;
            MinCallback = null;
            MaxCallback = null;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
            FreeInput = free;
        }

        public MinMaxSliderAttribute(string minCallback, string maxCallback, float step=-1, float minWidth=DefaultWidth, float maxWidth=DefaultWidth, bool free=false)
        {
            Min = -1;
            Max = -1;
            MinCallback = minCallback;
            MaxCallback = maxCallback;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
            FreeInput = free;
        }

        public MinMaxSliderAttribute(float min, string maxCallback, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth, bool free=false)
        {
            Min = min;
            Max = -1;
            MinCallback = null;
            MaxCallback = maxCallback;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
            FreeInput = free;
        }

        public MinMaxSliderAttribute(string minCallback, float max, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth, bool free=false)
        {
            Min = -1;
            Max = max;
            MinCallback = minCallback;
            MaxCallback = null;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
            FreeInput = free;
        }
    }
}
