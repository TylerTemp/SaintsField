namespace SaintsField
{
    public class BelowImageAttribute: ShowImageAttribute
    {
        public BelowImageAttribute(string image, int maxWidth = -1, int maxHeight = -1, string groupBy = "") : base(image, maxWidth: maxWidth, maxHeight: maxHeight, above: false,
            groupBy: groupBy)
        {
        }
    }
}
