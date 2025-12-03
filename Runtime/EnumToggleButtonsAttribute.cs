using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumToggleButtonsAttribute: PathedDropdownAttribute
    {
        public EnumToggleButtonsAttribute()
        {
        }

        [Obsolete("Use [DefaultExpand] instead")]
        public EnumToggleButtonsAttribute(bool defaultExpanded)
        {
        }
    }
}
