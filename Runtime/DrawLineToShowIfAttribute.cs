using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLineToShowIfAttribute: HandleShowIfAttribute
    {
        public DrawLineToShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLineToShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
