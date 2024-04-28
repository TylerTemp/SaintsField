using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AboveImageAttribute: ShowImageAttribute
    {
        public AboveImageAttribute(string image = null, int maxWidth = -1, int maxHeight = -1, EAlign align = EAlign.FieldStart, string groupBy = "") : base(image, maxWidth, maxHeight, align,true, groupBy)
        {
        }
    }
}
