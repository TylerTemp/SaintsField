using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaHideIfAttribute: PlayaShowIfAttribute
    {
        public override bool IsShow => false;

        public PlayaHideIfAttribute(params object[] orCallbacks): base(orCallbacks)
        {
        }
    }
}
