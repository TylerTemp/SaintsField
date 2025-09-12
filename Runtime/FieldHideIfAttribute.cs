using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class FieldHideIfAttribute: FieldShowIfAttribute
    {
        public override bool IsShow => false;
        public FieldHideIfAttribute(params object[] orCallbacks) : base(orCallbacks)
        {
        }
    }
}
