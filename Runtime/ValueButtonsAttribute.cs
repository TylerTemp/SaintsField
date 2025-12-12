using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ValueButtonsAttribute: PathedDropdownAttribute
    {
        public ValueButtonsAttribute(string funcName = null, EUnique unique = EUnique.None): base(funcName, unique)
        {
        }

        public ValueButtonsAttribute(EUnique unique) : base(unique) {}
    }
}
