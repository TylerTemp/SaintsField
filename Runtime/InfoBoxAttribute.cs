using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class InfoBoxAttribute: Attribute, IPlayaAttribute, IPlayaClassAttribute, IPlayaIMGUIGroupBy
    {
        // public readonly string GroupBy;
        public string GroupBy { get; }

        // public readonly bool Above;
        public readonly bool Below;
        public readonly string Content;
        public readonly EMessageType MessageType;
        public readonly bool IsCallback;
        public readonly string ShowCallback;

        public InfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, bool below=false, string groupBy="")
        {
            GroupBy = groupBy;
            Below = below;

            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content, isCallback);

            Content = contentParsed;
            IsCallback = isCallbackParsed;

            MessageType = messageType;
            ShowCallback = show;
        }

        public InfoBoxAttribute(string content, bool isCallback): this(content, EMessageType.Info, null, isCallback)
        {
        }
    }
}
