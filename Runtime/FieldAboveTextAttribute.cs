using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class FieldAboveTextAttribute : FullWidthRichLabelAttribute
    {
        public FieldAboveTextAttribute(string richTextXml="<color=gray><label />", bool isCallback = false, string groupBy = "") : base(richTextXml, isCallback, true, groupBy)
        {
        }
    }
}
