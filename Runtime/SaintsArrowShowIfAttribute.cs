using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SaintsArrowShowIfAttribute: HandleShowIfAttribute
    {
        public SaintsArrowShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public SaintsArrowShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
