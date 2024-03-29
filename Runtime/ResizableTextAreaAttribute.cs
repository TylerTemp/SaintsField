using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ResizableTextAreaAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;

        public string GroupBy => "__LABEL_FIELD__";

        // public string GroupBy { get; }

        // public readonly bool FullWidth;

        // public ResizableTextAreaAttribute()
        // {
        //     // FullWidth = fullWidth;
        // }
    }
}
