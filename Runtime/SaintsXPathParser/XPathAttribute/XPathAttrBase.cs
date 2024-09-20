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

            if (attrString == "layer")
            {
                return (new XPathAttrLayer(), "");
            }

            Debug.Assert(attrString.StartsWith('@'));
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string attrTrim = attrString.Substring(1);
            if(attrTrim.StartsWith('{'))
            {
                int endBracketIndex = attrTrim.IndexOf('}');
                string evalString = attrTrim.Substring(1, endBracketIndex - 1);
                return (new XPathAttrFakeEval(evalString), attrTrim.Substring(endBracketIndex + 1).Trim());
            }

            if(attrTrim.StartsWith("resource-path()"))
            {
                return (new XPathAttrResourcePath(), attrTrim.Substring(15).Trim());
            }

            if (attrTrim.StartsWith("asset-path()"))
            {
                return (new XPathAttrAssetPath(), attrTrim.Substring(12).Trim());
            }

            throw new ArgumentOutOfRangeException(nameof(attrString), attrString, null);
        }

    }
}
