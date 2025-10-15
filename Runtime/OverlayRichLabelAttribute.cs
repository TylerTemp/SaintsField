namespace SaintsField
{
    public class OverlayRichLabelAttribute: OverlayTextAttribute
    {
        public OverlayRichLabelAttribute(string richTextXml, bool isCallback = false, bool end = false,
            float padding = 5f, string groupBy = "") :
            base(richTextXml, isCallback, end, padding, groupBy)
        {

        }
    }
}
