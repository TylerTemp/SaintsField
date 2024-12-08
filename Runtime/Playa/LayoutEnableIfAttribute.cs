using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutEnableIfAttribute: LayoutReadOnlyAttribute
    {
        public LayoutEnableIfAttribute(params object[] by): base(by)
        {
        }
    }
}
