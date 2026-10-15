#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEngine;

namespace SaintsField.SaintsXPathParser
{
    public static class XPathBracketParser
    {
        public static IEnumerable<string> Parse(string value, char startWith, char endWith)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i]))
                {
                    continue;
                }
                if (value[i] != startWith)
                {
                    throw new FormatException($"Expected `{startWith}` in `{value}`");
                }
                int end = PathSyntax.ClosingBracket(value, i);
                if (end < 0)
                {
                    throw new FormatException($"Unclosed `{startWith}` in `{value}`");
                }
                yield return value.Substring(i + 1, end - i - 1);
                i = end;
            }
        }

        public static IEnumerable<IReadOnlyList<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)>> ParseFilter(string value)
        {
            // Debug.Log($"get filter raw {value}");
            foreach (string chunk in Parse(value, '[', ']'))
            {
                // Debug.Log($"parsed chunk {value} -> {chunk}");
                List<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)> orFilters =
                    new List<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)>();

                foreach (string orFilterRaw in SplitOrFilter(chunk))
                {
                    if(int.TryParse(orFilterRaw, out int num))
                    {
                        orFilters.Add((new XPathAttrIndex(false), new FilterComparerInt(FilterComparer.Equal, num)));
                    }
                    else if (orFilterRaw == "last()")
                    {
                        orFilters.Add((new XPathAttrIndex(true), new FilterComparerInt(FilterComparer.Equal, -1)));
                    }
                    else
                    {
                        (XPathAttrBase attrBase, string left) = XPathAttrBase.Parser(orFilterRaw);
                        List<XPathCollectionOperation> operations = new List<XPathCollectionOperation>();
                        while (left.StartsWith("["))
                        {
                            int end = PathSyntax.ClosingBracket(left, 0);
                            if (end < 0)
                            {
                                throw new FormatException($"Unclosed collection index in `{value}`");
                            }
                            operations.Add(new XPathCollectionOperation(left.Substring(1, end - 1)));
                            left = left.Substring(end + 1).Trim();
                        }
                        if (operations.Count > 0)
                        {
                            attrBase = new XPathAttrCollection(attrBase, operations);
                        }
                        orFilters.Add((attrBase, FilterComparerBase.Parser(left)));
                    }
                }

                yield return orFilters;
            }
        }

        private static IEnumerable<string> SplitOrFilter(string chunk)
        {
            List<string> parts = new List<string>();
            foreach (string token in PathSyntax.Split(chunk, ' '))
            {
                if (token == "or")
                {
                    yield return string.Join(" ", parts).Trim();
                    parts.Clear();
                }
                else
                {
                    parts.Add(token);
                }
            }
            if (parts.Count > 0)
            {
                yield return string.Join(" ", parts).Trim();
            }
        }
    }
}
#endif
