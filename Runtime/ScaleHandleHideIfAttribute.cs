using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ScaleHandleHideIfAttribute: ScaleHandleShowIfAttribute
    {
        public override bool IsShow => false;

        public ScaleHandleHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public ScaleHandleHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
