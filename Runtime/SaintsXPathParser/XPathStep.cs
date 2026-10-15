#if UNITY_EDITOR
using System;
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
        public IReadOnlyList<XPathCollectionOperation> CollectionOperations;
        public IReadOnlyList<IReadOnlyList<XPathPredicate>> Predicates;  // and -> or

        public override string ToString()
        {
            string collections = string.Join("", (CollectionOperations ?? Array.Empty<XPathCollectionOperation>())
                .Select(operation => $"[{operation.Content}]"));
            return $"{string.Join("", Enumerable.Repeat('/', SepCount))}{Axis}::{NodeTest}`{Attr}`{collections}[{string.Join("][", (Predicates ?? Array.Empty<IReadOnlyList<XPathPredicate>>()).Select(each => string.Join(" OR ", each)))}]";
        }
    }
}
#endif
