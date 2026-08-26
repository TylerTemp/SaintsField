using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLineToHideIfAttribute: DrawLineToShowIfAttribute
    {
        public override bool IsShow => false;

        public DrawLineToHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLineToHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
