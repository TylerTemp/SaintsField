using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class PositionHandleShowIfAttribute: HandleShowIfAttribute
    {
        public PositionHandleShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public PositionHandleShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
