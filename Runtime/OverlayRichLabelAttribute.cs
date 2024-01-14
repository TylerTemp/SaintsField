using UnityEngine;

namespace SaintsField
{
    public class OverlayRichLabelAttribute: RichLabelAttribute
    {
        public override SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public override string GroupBy { get; }

        public readonly bool End;
        public readonly float Padding;

        public OverlayRichLabelAttribute(string richTextXml, bool isCallback=false, bool end=false, float padding=5f, string groupBy=""): base(richTextXml, isCallback)
        {
            End = end;
            Padding = padding;
            GroupBy = groupBy;
        }
    }
}
