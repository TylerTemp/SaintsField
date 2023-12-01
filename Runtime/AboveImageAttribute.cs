namespace SaintsField
{
    public class AboveImageAttribute: ShowImageAttribute
    {
        public AboveImageAttribute(string image, int maxWidth = -1, int maxHeight = -1, string groupBy = "") : base(image, maxWidth: maxWidth, maxHeight: maxHeight, above: true,
            groupBy: groupBy)
        {
        }
    }
}
