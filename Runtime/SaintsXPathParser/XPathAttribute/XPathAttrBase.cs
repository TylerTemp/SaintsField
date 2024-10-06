#if UNITY_EDITOR
using System;
using UnityEngine;

namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public abstract class XPathAttrBase
    {
        public static (XPathAttrBase xPathAttrBase, string leftContent) Parser(string attrString)
        {
            if (attrString == "last()")
            {
                return (new XPathAttrIndex(true), "");
            }

            if (attrString.StartsWith("index()"))
            {
                return (new XPathAttrIndex(false), attrString.Substring(7).Trim());
            }

            Debug.Assert(attrString.StartsWith("@"), attrString);
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string attrTrim = attrString.Substring(1);

            if (attrTrim.StartsWith("layer"))
            {
                return (new XPathAttrLayer(), attrTrim.Substring("layer".Length).Trim());
            }
            if(attrTrim.StartsWith("resource-path()"))
            {
                return (new XPathAttrResourcePath(), attrTrim.Substring("resource-path()".Length).Trim());
            }

            if (attrTrim.StartsWith("asset-path()"))
            {
                return (new XPathAttrAssetPath(), attrTrim.Substring("asset-path()".Length).Trim());
            }

            if(attrTrim.StartsWith("{"))
            {
                int endBracketIndex = attrTrim.IndexOf('}');
                string evalString = attrTrim.Substring(1, endBracketIndex - 1);
                return (new XPathAttrFakeEval(evalString), attrTrim.Substring(endBracketIndex + 1).Trim());
            }

            throw new ArgumentOutOfRangeException(nameof(attrString), attrTrim, $"invalid attr {attrString}");
        }

    }
}
#endif
