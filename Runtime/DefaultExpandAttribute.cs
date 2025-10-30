using System;
using System.Diagnostics;
using SaintsField.Playa;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class DefaultExpandAttribute: Attribute, IPlayaAttribute
    {

    }
}
