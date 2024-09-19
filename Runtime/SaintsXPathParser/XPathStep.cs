using System.Collections.Generic;
using SaintsField.SaintsXPathParser.XPathAttribute;

namespace SaintsField.SaintsXPathParser
{
    public struct XPathStep
    {
        public bool Descendant;  // step starts with `//`
        public AxisName AxisName;
        public NodeTest NodeTest;
        public XPathAttrBase Attr;
        public IReadOnlyList<XPathPredicate> Predicates;
    }
}
