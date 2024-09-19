using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;

namespace SaintsField.SaintsXPathParser
{
    public struct XPathPredicate
    {
        public XPathAttrBase Attr;
        public FilterComparer FilterComparer;
    }
}
