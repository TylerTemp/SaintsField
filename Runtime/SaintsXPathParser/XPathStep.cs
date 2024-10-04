#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsXPathParser.XPathAttribute;

namespace SaintsField.SaintsXPathParser
{
    public struct XPathStep
    {
        public int SepCount;  // step starts with how many `/`
        public Axis Axis;
        public NodeTest NodeTest;
        public XPathAttrBase Attr;
        public IReadOnlyList<IReadOnlyList<XPathPredicate>> Predicates;  // and -> or

        public override string ToString()
        {
            return $"{string.Join("", Enumerable.Repeat('/', SepCount))}{Axis}::{NodeTest}`{Attr}`[{string.Join("][", Predicates.Select(each => string.Join(" OR ", each)))}]";
        }
    }
}
#endif
