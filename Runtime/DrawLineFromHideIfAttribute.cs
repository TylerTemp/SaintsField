using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLineFromHideIfAttribute: DrawLineFromShowIfAttribute
    {
        public override bool IsShow => false;

        public DrawLineFromHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLineFromHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
