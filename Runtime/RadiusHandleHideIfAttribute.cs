using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class RadiusHandleHideIfAttribute: RadiusHandleShowIfAttribute
    {
        public override bool IsShow => false;

        public RadiusHandleHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public RadiusHandleHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
