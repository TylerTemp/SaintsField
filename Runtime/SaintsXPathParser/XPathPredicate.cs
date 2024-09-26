#if UNITY_EDITOR
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;

namespace SaintsField.SaintsXPathParser
{
    public struct XPathPredicate
    {
        public XPathAttrBase Attr;
        public FilterComparerBase FilterComparer;

        public override string ToString()
        {
            return $"`{Attr} {FilterComparer}`";
        }
    }
}
#endif
