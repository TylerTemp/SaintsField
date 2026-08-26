using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLineFromShowIfAttribute: HandleShowIfAttribute
    {
        public DrawLineFromShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLineFromShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
