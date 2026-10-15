#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsXPathParser.XPathAttribute;

namespace SaintsField.SaintsXPathParser
{
    public static class PathParser
    {
        private static readonly Dictionary<string, IReadOnlyList<XPathStep>> Cache =
            new Dictionary<string, IReadOnlyList<XPathStep>>();

        public static IReadOnlyList<XPathStep> Parse(string path, bool callback = false)
        {
            string key = (callback ? "callback:" : "xpath:") + path;
            if (Cache.TryGetValue(key, out IReadOnlyList<XPathStep> cached))
            {
                return cached;
            }

            List<XPathStep> steps = new List<XPathStep>();
            int separators = 0;
            foreach (string chunk in PathSyntax.Split(callback ? PathSyntax.NormalizeCallback(path) : path, '/'))
            {
                if (chunk.Length == 0)
                {
                    separators++;
                    continue;
                }
                steps.Add(ParseStep(chunk, separators, callback));
                separators = 1;
            }
            return Cache[key] = steps.ToArray();
        }

        private static XPathStep ParseStep(string text, int separators, bool callback)
        {
            (Axis axis, string rest) = XPathParser.SplitAxisFromStep(text);
            int attrStart = rest.IndexOf('@');
            int bracketStart = rest.IndexOf('[');
            if (bracketStart >= 0 && bracketStart < attrStart)
            {
                attrStart = -1;
            }

            int nameEnd = attrStart >= 0 ? attrStart : bracketStart >= 0 ? bracketStart : rest.Length;
            string name = rest.Substring(0, nameEnd);
            XPathAttrBase attr = null;
            string suffix = rest.Substring(nameEnd);
            if (attrStart >= 0)
            {
                (attr, suffix) = XPathAttrBase.Parser(suffix);
            }
            else if (name.EndsWith("()", StringComparison.Ordinal))
            {
                attr = new XPathAttrFakeEval(name);
                name = "";
            }

            List<XPathCollectionOperation> collections = new List<XPathCollectionOperation>();
            bool collectionSuffix = attr != null || callback && name != ".." && name != ".";
            while (collectionSuffix && suffix.StartsWith("["))
            {
                int end = PathSyntax.ClosingBracket(suffix, 0);
                if (end < 0)
                {
                    throw new FormatException($"Unclosed collection index in `{text}`");
                }
                string content = suffix[1..end];
                if (!XPathCollectionOperation.IsCollectionOperation(content)
                    || attr is XPathAttrGetComponents && content != "*")
                {
                    break;
                }
                collections.Add(new XPathCollectionOperation(content));
                suffix = suffix[(end + 1)..].Trim();
            }

            if (suffix != "" && !suffix.StartsWith("["))
            {
                throw new FormatException($"Invalid path suffix `{suffix}` in `{text}`");
            }

            return new XPathStep
            {
                SepCount = separators,
                Axis = axis,
                NodeTest = XPathParser.ParseNodeTest(name),
                Attr = attr,
                CollectionOperations = collections,
                Predicates = string.IsNullOrEmpty(suffix)
                    ? Array.Empty<IReadOnlyList<XPathPredicate>>()
                    : XPathBracketParser.ParseFilter(suffix)
                        .Select(group => (IReadOnlyList<XPathPredicate>)group.Select(item => new XPathPredicate
                        {
                            Attr = item.attrBase,
                            FilterComparer = item.filterComparerBase,
                        }).ToArray()).ToArray(),
            };
        }
    }
}
#endif
