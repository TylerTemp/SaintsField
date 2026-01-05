using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FieldEnableIfAttribute: FieldReadOnlyAttribute
    {
        public FieldEnableIfAttribute(params object[] by) : base(by)
        {
        }
    }
}
