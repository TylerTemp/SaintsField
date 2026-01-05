using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class EnableIfAttribute: DisableIfAttribute
    {
        public EnableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
