using System;
using System.Diagnostics;
using SaintsField.Utils;
using Debug = UnityEngine.Debug;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaInfoBoxAttribute: Attribute, IPlayaAttribute, IPlayaIMGUIGroupBy
    {
        // public readonly string GroupBy;
        public string GroupBy { get; }

        // public readonly bool Above;
        public readonly bool Below;
        public readonly string Content;
        public readonly EMessageType MessageType;
        public readonly bool IsCallback;
        public readonly string ShowCallback;

        public PlayaInfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, bool below=false, string groupBy="")
        {
            GroupBy = groupBy;
            Below = below;

            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content, isCallback);

            Content = contentParsed;
            IsCallback = isCallbackParsed;

            MessageType = messageType;
            ShowCallback = show;
        }

        public PlayaInfoBoxAttribute(string content, bool isCallback): this(content, EMessageType.Info, null, isCallback)
        {
        }
    }
}
