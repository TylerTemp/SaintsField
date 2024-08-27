using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class InfoBoxAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        // public readonly bool Above;
        public readonly bool Below;
        public readonly string Content;
        public readonly EMessageType MessageType;
        public readonly bool IsCallback;
        public readonly string ShowCallback;

        // above is kept for compatibility reason...
        public InfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, bool below=false, string groupBy="")
        {
            GroupBy = groupBy;
            Below = below;

            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content, isCallback);

            Content = contentParsed;
            IsCallback = isCallbackParsed;

            // Above = above;
            MessageType = messageType;
            ShowCallback = show;
        }

        public InfoBoxAttribute(string content, bool isCallback): this(content, EMessageType.Info, null, isCallback)
        {
        }
    }
}
