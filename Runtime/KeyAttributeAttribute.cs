using System;
using System.Diagnostics;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true)]
    public class KeyAttributeAttribute: InjectAttributeBase
    {
        public KeyAttributeAttribute(Type decorator, params object[] parameters) : base(decorator, parameters)
        {
        }
    }
}
