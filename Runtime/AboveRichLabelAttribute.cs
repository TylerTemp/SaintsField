using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AboveRichLabelAttribute: FieldAboveTextAttribute
    {
        public AboveRichLabelAttribute(string richTextXml="<color=gray><label />", bool isCallback = false, string groupBy = "") : base(richTextXml, isCallback, groupBy)
        {
        }
    }
}
