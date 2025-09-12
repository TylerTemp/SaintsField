using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public override bool IsShow => false;

        public HideIfAttribute(params object[] orCallbacks): base(orCallbacks)
        {
        }
    }
}
