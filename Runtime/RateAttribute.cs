using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class RateAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly int Min;
        public readonly int Max;

        public RateAttribute(int min, int max)
        {
            Debug.Assert(min >= 0);
            Debug.Assert(max > min);
            Min = min;
            Max = max;
        }
    }
}
