using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SaintsArrowHideIfAttribute: SaintsArrowShowIfAttribute
    {
        public override bool IsShow => false;

        public SaintsArrowHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public SaintsArrowHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
