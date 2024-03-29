using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly float Min;
        public readonly string MinCallback;
        public readonly float Max;
        public readonly string MaxCallback;
        public readonly float Step;

        public readonly float MinWidth;
        public readonly float MaxWidth;

        private const float DefaultWidth = 50f;

        public MinMaxSliderAttribute(float min, float max, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = min;
            Max = max;
            MinCallback = null;
            MaxCallback = null;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }

        public MinMaxSliderAttribute(int min, int max, int step=1, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = min;
            Max = max;
            MinCallback = null;
            MaxCallback = null;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }

        public MinMaxSliderAttribute(string minCallback, string maxCallback, int step=-1, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = -1;
            Max = -1;
            MinCallback = minCallback;
            MaxCallback = maxCallback;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }

        public MinMaxSliderAttribute(float min, string maxCallback, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = min;
            Max = -1;
            MinCallback = null;
            MaxCallback = maxCallback;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }

        public MinMaxSliderAttribute(string minCallback, float max, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = -1;
            Max = max;
            MinCallback = minCallback;
            MaxCallback = null;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }
    }
}
