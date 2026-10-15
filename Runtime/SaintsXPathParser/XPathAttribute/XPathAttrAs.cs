#if UNITY_EDITOR
using System;

namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public sealed class XPathAttrAs : XPathAttrBase
    {
        public readonly string TypeName;
        public readonly bool Negate;

        public XPathAttrAs(string typeName, bool negate = false)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new FormatException($"Missing type name in @{(negate ? "not-as" : "as")}()");
            }
            TypeName = typeName.Trim();
            Negate = negate;
        }
    }
}
#endif
