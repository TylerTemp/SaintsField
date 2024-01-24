#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine;

namespace SaintsField
{
    public class ReferenceAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
    }
}
#endif
