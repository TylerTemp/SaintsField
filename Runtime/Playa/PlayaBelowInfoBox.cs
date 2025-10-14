using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaBelowInfoBox: BelowInfoBox
    {
        public PlayaBelowInfoBox(string content, EMessageType messageType = EMessageType.Info, string show = null, bool isCallback = false, string groupBy = "") : base(content, messageType, show, isCallback, groupBy)
        {
        }

        public PlayaBelowInfoBox(string content, bool isCallback) : base(content, isCallback)
        {
        }
    }
}
