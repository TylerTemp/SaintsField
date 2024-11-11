#if UNITY_2021_3_OR_NEWER
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ReferencePickerAttribute : PropertyAttribute, ISaintsAttribute
    {
        // public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        // public string GroupBy => "__LABEL_FIELD__";
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool HideLabel;

        public ReferencePickerAttribute(bool hideLabel=false) => HideLabel = hideLabel;
    }
}
#endif
