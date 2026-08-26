using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SphereHandleCapHideIfAttribute: SphereHandleCapShowIfAttribute
    {
        public override bool IsShow => false;

        public SphereHandleCapHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public SphereHandleCapHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
