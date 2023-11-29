using UnityEngine;

namespace SaintsField
{
    public class PostFieldRichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly string RichTextXml;
        public readonly bool IsCallback;
        public readonly float Padding;

        public PostFieldRichLabelAttribute(string richTextXml, bool isCallback=false, float padding=5f, string groupBy="")
        {
            RichTextXml = richTextXml;
            IsCallback = isCallback;
            Padding = padding;
            GroupBy = groupBy;
        }
    }
}
