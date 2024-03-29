using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class PropRangeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly string MinCallback;
        public readonly string MaxCallback;
        public readonly float Min;
        public readonly float Max;

        public readonly float Step;
        // ReSharper enable InconsistentNaming

        public PropRangeAttribute(float min, float max, float step=-1f)
        {
            Min = min;
            Max = max;
            MinCallback = null;
            MaxCallback = null;
            Step = step;
        }

        public PropRangeAttribute(string minCallback, string maxCallback, float step=-1f)
        {
            Min = -1;
            Max = -1;
            MinCallback = minCallback;
            MaxCallback = maxCallback;
            Step = step;
        }

        public PropRangeAttribute(string minCallback, float max, float step=-1f)
        {
            MinCallback = minCallback;
            MaxCallback = null;
            Max = max;
            Step = step;
        }

        public PropRangeAttribute(float min, string maxCallback, float step=-1f)
        {
            Min = min;
            MinCallback = null;
            MaxCallback = maxCallback;
            Step = step;
        }
    }
}
