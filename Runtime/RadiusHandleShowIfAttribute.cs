using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class RadiusHandleShowIfAttribute: HandleShowIfAttribute
    {
        public RadiusHandleShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public RadiusHandleShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
