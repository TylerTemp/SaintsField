using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLineShowIfAttribute: HandleShowIfAttribute
    {
        public DrawLineShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLineShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
