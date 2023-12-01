namespace SaintsField
{
    public class AboveImageAttribute: ShowImageAttribute
    {
        public AboveImageAttribute(string image, int maxWidth = -1, int maxHeight = -1, EAlign align = EAlign.Start, string groupBy = "") : base(image, maxWidth, maxHeight, align,true, groupBy)
        {
        }
    }
}
