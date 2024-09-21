using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsXPathParser.XPathAttribute;

namespace SaintsField.SaintsXPathParser
{
    public struct XPathStep
    {
        public int SepCount;  // step starts with how many `/`
        public AxisName AxisName;
        public XPathAttrBase Attr;
        public NodeTest NodeTest;
        public IReadOnlyList<XPathPredicate> Predicates;

        public override string ToString()
        {
            return $"{string.Join("", Enumerable.Repeat('/', SepCount))}{AxisName}::{NodeTest}`{Attr}`[{string.Join("][", Predicates)}]";
        }
    }
}
