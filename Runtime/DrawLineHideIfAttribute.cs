using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLineHideIfAttribute: DrawLineShowIfAttribute
    {
        public override bool IsShow => false;

        public DrawLineHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLineHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
