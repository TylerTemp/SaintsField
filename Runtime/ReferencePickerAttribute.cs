#if UNITY_2021_3_OR_NEWER
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ReferencePickerAttribute : PropertyAttribute, ISaintsAttribute
    {
#if UNITY_2022_2_OR_NEWER
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
#else
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";
#endif

        public readonly bool HideLabel;

        public ReferencePickerAttribute(bool hideLabel=false) => HideLabel = hideLabel;
    }
}
#endif
