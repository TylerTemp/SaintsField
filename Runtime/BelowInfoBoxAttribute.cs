using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class BelowInfoBoxAttribute: InfoBoxAttribute
    {
        public BelowInfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, string groupBy="")
        : base(content, messageType, show, isCallback, true, groupBy)
        {
        }

        public BelowInfoBoxAttribute(string content, bool isCallback): this(content, EMessageType.Info, null, isCallback)
        {
        }
    }
}
