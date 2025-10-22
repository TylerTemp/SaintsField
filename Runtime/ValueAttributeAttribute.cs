using System;
using System.Diagnostics;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ValueAttributeAttribute: InjectAttributeBase
    {
        public ValueAttributeAttribute(Type decorator, params object[] parameters) : base(decorator, parameters)
        {
        }
    }
}
