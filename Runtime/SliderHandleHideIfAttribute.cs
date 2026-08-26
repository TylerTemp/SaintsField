using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SliderHandleHideIfAttribute: SliderHandleShowIfAttribute
    {
        public override bool IsShow => false;

        public SliderHandleHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public SliderHandleHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
