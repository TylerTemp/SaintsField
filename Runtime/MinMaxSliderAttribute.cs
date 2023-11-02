using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly float Min;
        public readonly float Max;
        public readonly float Step;

        public readonly float MinWidth;
        public readonly float MaxWidth;

        private const float DefaultWidth = 50f;

        public MinMaxSliderAttribute(float min, float max, float step=-1f, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = min;
            Max = max;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }

        public MinMaxSliderAttribute(int min, int max, int step=1, float minWidth=DefaultWidth, float maxWidth=DefaultWidth)
        {
            Min = min;
            Max = max;
            Step = step;

            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }
    }
}
