using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class RotationHandleShowIfAttribute: HandleShowIfAttribute
    {
        public RotationHandleShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public RotationHandleShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
