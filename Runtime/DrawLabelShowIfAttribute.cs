using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawLabelShowIfAttribute: HandleShowIfAttribute
    {
        public DrawLabelShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawLabelShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
