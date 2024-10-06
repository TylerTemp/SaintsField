#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public class XPathAttrAssetPath: XPathAttrBase
    {
        public override string ToString()
        {
            return "@:asset-path:";
        }
    }
}
#endif
