#if UNITY_2021_3_OR_NEWER
using UnityEngine;

namespace SaintsField
{
    public class ReferencePickerAttribute : PropertyAttribute, ISaintsAttribute
    {
        // public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        // public string GroupBy => "__LABEL_FIELD__";
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";
    }
}
#endif
