using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    // ReSharper disable once UnusedType.Global
    public class PlayaShowIfAttribute: ShowIfAttribute
    {
        public PlayaShowIfAttribute(params object[] andCallbacks) : base(andCallbacks)
        {
        }
    }
}
