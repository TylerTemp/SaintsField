using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PlayaRichLabelAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly string RichTextXml;
        public readonly bool IsCallback;
        // ReSharper enable InconsistentNaming

        public PlayaRichLabelAttribute(string richTextXml, bool isCallback=false)
        {
            RichTextXml = richTextXml;
            IsCallback = isCallback;
        }
    }
}
