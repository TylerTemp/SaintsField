using System;
using System.Diagnostics;
using SaintsField.Utils;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaBelowRichLabelAttribute: Attribute, IPlayaAttribute, IPlayaClassAttribute
    {
        public readonly string Content;
        public readonly bool IsCallback;

        public bool Below = true;

        public PlayaBelowRichLabelAttribute(string content)
        {
            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content);

            Content = contentParsed;
            IsCallback = isCallbackParsed;
        }
    }
}
