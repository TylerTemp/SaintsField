using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DrawWireDiscShowIfAttribute: HandleShowIfAttribute
    {
        public DrawWireDiscShowIfAttribute(EMode eMode, params object[] andCallbacks): base(eMode, andCallbacks)
        {
        }

        public DrawWireDiscShowIfAttribute(params object[] andCallbacks): base(andCallbacks)
        {
        }
    }
}
