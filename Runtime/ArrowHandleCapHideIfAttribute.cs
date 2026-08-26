using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ArrowHandleCapHideIfAttribute: ArrowHandleCapShowIfAttribute
    {
        public override bool IsShow => false;

        public ArrowHandleCapHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public ArrowHandleCapHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
