namespace SaintsField
{
    public class AboveRichLabelAttribute : FullWidthRichLabelAttribute
    {
        public AboveRichLabelAttribute(string richTextXml, bool isCallback = false, string groupBy = "") : base(richTextXml, isCallback, true, groupBy)
        {
        }
    }
}
