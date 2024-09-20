using System.Collections.Generic;
using SaintsField.SaintsXPathParser.XPathAttribute;

namespace SaintsField.SaintsXPathParser
{
    public struct XPathStep
    {
        public int SepCount;  // step starts with how many `/`
        public AxisName AxisName;
        public NodeTest NodeTest;
        public XPathAttrBase Attr;
        public IReadOnlyList<XPathPredicate> Predicates;
    }
}
