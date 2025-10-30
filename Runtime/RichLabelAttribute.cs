using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: FieldLabelTextAttribute
    {
        public RichLabelAttribute(string richTextXml, bool isCallback = false) : base(richTextXml, isCallback)
        {
        }
    }
}
