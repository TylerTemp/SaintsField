using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EnumToggleButtonsAttribute: PathedDropdownAttribute
    {
        public readonly bool NoFold;

        public EnumToggleButtonsAttribute(bool noFold=false)
        {
            NoFold = noFold;
        }
    }
}
