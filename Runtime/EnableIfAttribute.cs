using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfAttribute: ReadOnlyAttribute
    {
        public EnableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
