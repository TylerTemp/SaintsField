using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SphereHandleCapShowIfAttribute: HandleShowIfAttribute
    {
        public SphereHandleCapShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public SphereHandleCapShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
