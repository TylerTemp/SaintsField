using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class CompInfoBoxAttribute: Attribute, IPlayaAttribute
    {
        public readonly string Content;
        public readonly EMessageType MessageType;
        public readonly bool IsCallback;
        public readonly string ShowCallback;

        public CompInfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null)
        {
            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content);

            Content = contentParsed;
            IsCallback = isCallbackParsed;

            MessageType = messageType;
            ShowCallback = show;
        }
    }
}
