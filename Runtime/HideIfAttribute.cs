using System;
using System.Diagnostics;
using SaintsField.Condition;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public override bool IsShow => false;
        public HideIfAttribute(params object[] orCallbacks) : base(orCallbacks)
        {
        }
    }
}
