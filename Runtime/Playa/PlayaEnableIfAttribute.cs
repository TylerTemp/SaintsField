using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    // ReSharper disable once UnusedType.Global
    public class PlayaEnableIfAttribute: EnableIfAttribute
    {
        public PlayaEnableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
