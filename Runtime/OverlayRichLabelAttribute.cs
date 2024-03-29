using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class OverlayRichLabelAttribute: RichLabelAttribute
    {
        public override SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public override string GroupBy { get; }

        // ReSharper disable InconsistentNaming
        public readonly bool End;
        public readonly float Padding;
        // ReSharper enable InconsistentNaming

        public OverlayRichLabelAttribute(string richTextXml, bool isCallback=false, bool end=false, float padding=5f, string groupBy=""): base(richTextXml, isCallback)
        {
            End = end;
            Padding = padding;
            GroupBy = groupBy;
        }
    }
}
