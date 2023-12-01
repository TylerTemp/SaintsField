namespace SaintsField
{
    public class BelowImageAttribute: ShowImageAttribute
    {
        public BelowImageAttribute(string image, int maxWidth = -1, int maxHeight = -1, EAlign align = EAlign.Start, string groupBy = "") : base(image, maxWidth, maxHeight, align, false, groupBy)
        {
        }
    }
}
