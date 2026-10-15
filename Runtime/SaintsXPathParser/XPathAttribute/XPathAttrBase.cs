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

            if (attrTrim.StartsWith("get-components(") || attrTrim.StartsWith("get-component("))
            {
                bool firstOnly = attrTrim.StartsWith("get-component(");
                int start = firstOnly ? "get-component(".Length : "get-components(".Length;
                int end = PathSyntax.ClosingBracket(attrTrim, start - 1);
                if (end < 0)
                {
                    throw new FormatException($"Unclosed component selector in `{attrString}`");
                }
                return (new XPathAttrGetComponents(attrTrim.Substring(start, end - start), firstOnly), attrTrim.Substring(end + 1).Trim());
            }

            if (attrTrim.StartsWith("as(") || attrTrim.StartsWith("not-as("))
            {
                bool negate = attrTrim.StartsWith("not-as(");
                int start = negate ? "not-as(".Length : "as(".Length;
                int end = PathSyntax.ClosingBracket(attrTrim, start - 1);
                if (end < 0)
                {
                    throw new FormatException($"Unclosed type selector in `{attrString}`");
                }
                return (new XPathAttrAs(attrTrim.Substring(start, end - start), negate), attrTrim.Substring(end + 1).Trim());
            }

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
                int endBracketIndex = PathSyntax.ClosingBracket(attrTrim, 0);
                if (endBracketIndex < 0)
                {
                    throw new FormatException($"Unclosed expression in `{attrString}`");
                }
                string evalString = attrTrim.Substring(1, endBracketIndex - 1);
                return (new XPathAttrFakeEval(evalString), attrTrim.Substring(endBracketIndex + 1).Trim());
            }

            throw new ArgumentOutOfRangeException(nameof(attrString), attrTrim, $"Invalid attribute `{attrString}`");
        }

    }
}
#endif
