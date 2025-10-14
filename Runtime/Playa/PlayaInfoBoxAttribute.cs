using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class PlayaInfoBoxAttribute: InfoBoxAttribute
    {
        public PlayaInfoBoxAttribute(string content, EMessageType messageType = EMessageType.Info, string show = null, bool isCallback = false, bool below = false, string groupBy = "") : base(content, messageType, show, isCallback, below, groupBy)
        {
        }

        public PlayaInfoBoxAttribute(string content, bool isCallback) : base(content, isCallback)
        {
        }
    }
}
