using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class FieldBelowTextAttribute : FullWidthRichLabelAttribute
    {
        public FieldBelowTextAttribute(string richTextXml = "<label />", bool isCallback = false, string groupBy = "") : base(richTextXml, isCallback, false, groupBy)
        {
        }
    }
}
