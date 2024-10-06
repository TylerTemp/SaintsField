#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public class XPathAttrIndex: XPathAttrBase
    {
        public readonly bool Last;

        public XPathAttrIndex(bool last)
        {
            Last = last;
        }

        public override string ToString()
        {
            return $"@:index{(Last ? "{last()}" : "")}:";
        }
    }
}
#endif
