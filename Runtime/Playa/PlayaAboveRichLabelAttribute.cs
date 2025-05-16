using System;
using System.Diagnostics;
using SaintsField.Utils;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]

    public class PlayaAboveRichLabelAttribute: Attribute, IPlayaAttribute, IPlayaClassAttribute
    {
        public readonly string Content;
        public readonly bool IsCallback;

        public PlayaAboveRichLabelAttribute(string content)
        {
            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content);

            Content = contentParsed;
            IsCallback = isCallbackParsed;
        }
    }
}
