using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class PrimitiveBoundsHandleHideIfAttribute: PrimitiveBoundsHandleShowIfAttribute
    {
        public override bool IsShow => false;

        public PrimitiveBoundsHandleHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public PrimitiveBoundsHandleHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
