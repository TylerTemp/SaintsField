#if UNITY_EDITOR
using System.Collections.Generic;

namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public sealed class XPathAttrCollection : XPathAttrBase
    {
        public readonly XPathAttrBase Inner;
        public readonly IReadOnlyList<XPathCollectionOperation> Operations;

        public XPathAttrCollection(XPathAttrBase inner, IReadOnlyList<XPathCollectionOperation> operations)
        {
            Inner = inner;
            Operations = operations;
        }
    }
}
#endif
