using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class RotationHandleHideIfAttribute: RotationHandleShowIfAttribute
    {
        public override bool IsShow => false;

        public RotationHandleHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public RotationHandleHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
