using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SliderHandleShowIfAttribute: HandleShowIfAttribute
    {
        public SliderHandleShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public SliderHandleShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
