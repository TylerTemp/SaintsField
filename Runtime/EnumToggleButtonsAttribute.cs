using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumToggleButtonsAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        public EnumToggleButtonsAttribute()
        {
        }

        [Obsolete("Use [DefaultExpand] instead")]
        public EnumToggleButtonsAttribute(bool defaultExpanded)
        {
        }
    }
}
