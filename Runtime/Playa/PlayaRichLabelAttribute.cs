using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PlayaRichLabelAttribute: LabelTextAttribute
    {
        public PlayaRichLabelAttribute(string richTextXml, bool isCallback = false) : base(richTextXml, isCallback)
        {
        }
    }
}
