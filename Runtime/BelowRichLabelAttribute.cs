using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class BelowRichLabelAttribute: FieldBelowTextAttribute
    {
        public BelowRichLabelAttribute(string richTextXml = "<label />", bool isCallback = false, string groupBy = "") : base(
            richTextXml, isCallback, groupBy)
        {
        }
    }
}
