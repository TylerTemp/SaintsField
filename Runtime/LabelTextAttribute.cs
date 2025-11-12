using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LabelTextAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly string RichTextXml;
        public readonly bool IsCallback;
        // ReSharper enable InconsistentNaming

        public LabelTextAttribute(string richTextXml, bool isCallback=false)
        {
            (string parsedContent, bool parsedIsCallback) = RuntimeUtil.ParseCallback(richTextXml, isCallback);
            RichTextXml = parsedContent;
            IsCallback = parsedIsCallback;
        }
    }
}
