using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class FieldBelowInfoBoxAttribute: FieldInfoBoxAttribute
    {
        public FieldBelowInfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, string groupBy="")
        : base(content, messageType, show, isCallback, true, groupBy)
        {
        }

        public FieldBelowInfoBoxAttribute(string content, bool isCallback): this(content, EMessageType.Info, null, isCallback)
        {
        }
    }
}
