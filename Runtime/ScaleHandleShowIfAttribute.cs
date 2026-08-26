using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ScaleHandleShowIfAttribute: HandleShowIfAttribute
    {
        public ScaleHandleShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public ScaleHandleShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
