using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.I2Loc
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class LocalizedStringPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";
    }
}
