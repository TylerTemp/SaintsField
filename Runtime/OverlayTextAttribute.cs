using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class OverlayTextAttribute: FieldLabelTextAttribute
    {
        public override SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public override string GroupBy { get; }

        // ReSharper disable InconsistentNaming
        public readonly bool End;
        public readonly float Padding;
        // ReSharper enable InconsistentNaming

        public OverlayTextAttribute(string richTextXml, bool isCallback=false, bool end=false, float padding=5f, string groupBy=""): base(richTextXml, isCallback)
        {
            End = end;
            Padding = padding;
            GroupBy = groupBy;
        }
    }
}
