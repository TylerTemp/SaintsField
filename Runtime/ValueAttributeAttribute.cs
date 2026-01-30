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
        public ValueAttributeAttribute(Type decorator, params object[] parameters) : base(1, decorator, parameters)
        {
        }

        public ValueAttributeAttribute(int depth, Type decorator, params object[] parameters) : base(depth, decorator, parameters)
        {
        }
    }
}
