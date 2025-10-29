using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PostFieldRichLabelAttribute: EndTextAttribute
    {
        public PostFieldRichLabelAttribute(string richTextXml, bool isCallback = false, float padding = 5, string groupBy = "") : base(richTextXml, isCallback, padding, groupBy)
        {
        }
    }
}
