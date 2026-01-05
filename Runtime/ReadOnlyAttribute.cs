using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ReadOnlyAttribute: DisableIfAttribute
    {
        public ReadOnlyAttribute(params object[] by): base(by) {}
    }
}
