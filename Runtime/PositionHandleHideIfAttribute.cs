using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class PositionHandleHideIfAttribute: PositionHandleShowIfAttribute
    {
        public override bool IsShow => false;

        public PositionHandleHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public PositionHandleHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
