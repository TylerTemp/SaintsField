using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class BelowInfoBox: InfoBoxAttribute
    {
        public BelowInfoBox(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, string groupBy=""): base(content, messageType, show, isCallback, true, groupBy)
        {
        }

        public BelowInfoBox(string content, bool isCallback): base(content, isCallback)
        {
        }
    }
}
