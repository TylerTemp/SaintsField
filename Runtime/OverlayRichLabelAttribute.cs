using UnityEngine;

namespace SaintsField
{
    public class OverlayRichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly string RichTextXml;
        public readonly bool IsCallback;
        public readonly bool End;
        public readonly float Padding;

        public OverlayRichLabelAttribute(string richTextXml, bool isCallback=false, bool end=false, float padding=5f, string groupBy="")
        {
            RichTextXml = richTextXml;
            IsCallback = isCallback;
            End = end;
            Padding = padding;
            GroupBy = groupBy;
        }
    }
}
