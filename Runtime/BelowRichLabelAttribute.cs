namespace SaintsField
{
    public class BelowRichLabelAttribute : FullWidthRichLabelAttribute
    {
        public BelowRichLabelAttribute(string richTextXml, bool isCallback = false, string groupBy = "") : base(richTextXml, isCallback, false, groupBy)
        {
        }
    }
}
