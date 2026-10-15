#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public sealed class XPathAttrGetComponents : XPathAttrBase
    {
        public readonly string TypeName;
        public readonly bool FirstOnly;

        public XPathAttrGetComponents(string typeName) : this(typeName, false)
        {
        }

        public XPathAttrGetComponents(string typeName, bool firstOnly)
        {
            TypeName = typeName.Trim();
            FirstOnly = firstOnly;
        }
    }
}
#endif
