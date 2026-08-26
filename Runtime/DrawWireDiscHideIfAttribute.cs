using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawWireDiscHideIfAttribute: DrawWireDiscShowIfAttribute
    {
        public override bool IsShow => false;

        public DrawWireDiscHideIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawWireDiscHideIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
