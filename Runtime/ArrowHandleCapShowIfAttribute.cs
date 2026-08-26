using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ArrowHandleCapShowIfAttribute: HandleShowIfAttribute
    {
        public ArrowHandleCapShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public ArrowHandleCapShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
