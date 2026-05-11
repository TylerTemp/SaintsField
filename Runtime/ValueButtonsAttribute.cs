using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ValueButtonsAttribute: PathedDropdownAttribute
    {
        public readonly bool NoFold;

        public ValueButtonsAttribute(string funcName = null, EUnique unique = EUnique.None, bool noFold=false): base(funcName, unique)
        {
            NoFold = noFold;
        }

        public ValueButtonsAttribute(EUnique unique) : this(null, unique) {}
        public ValueButtonsAttribute(string funcName) : this(funcName, EUnique.None) {}
        public ValueButtonsAttribute(bool noFold) : this(null, EUnique.None, noFold) {}
    }
}
