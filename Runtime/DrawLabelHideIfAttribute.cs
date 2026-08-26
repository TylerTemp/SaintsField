using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLabelHideIfAttribute: DrawLabelShowIfAttribute
    {
        public override bool IsShow => false;

        public DrawLabelHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLabelHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
