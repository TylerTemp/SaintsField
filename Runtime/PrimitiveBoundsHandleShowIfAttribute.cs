using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class PrimitiveBoundsHandleShowIfAttribute: HandleShowIfAttribute
    {
        public PrimitiveBoundsHandleShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public PrimitiveBoundsHandleShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
