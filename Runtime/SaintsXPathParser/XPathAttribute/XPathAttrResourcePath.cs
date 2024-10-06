#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public class XPathAttrResourcePath: XPathAttrBase
    {
        public override string ToString()
        {
            return "@:resource-path:";
        }
    }
}
#endif
