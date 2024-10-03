#if UNITY_EDITOR
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
            StringBuilder stringBuilder = new StringBuilder();
            bool inBracket = false;
            foreach (char c in value)
            {
                if (c == startWith)
                {
                    inBracket = true;
                    stringBuilder.Clear();
                }
                else if (c == endWith)
                {
                    inBracket = false;
                    yield return stringBuilder.ToString();
                }
                else if (inBracket)
                {
                    stringBuilder.Append(c);
                }
            }
        }

        public static IEnumerable<IReadOnlyList<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)>> ParseFilter(string value)
        {
            // Debug.Log($"get filter raw {value}");
            foreach (string chunk in Parse(value, '[', ']'))
            {
                List<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)> orFilters =
                    new List<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)>();

                foreach (string orFilterRaw in SplitOrFilter(chunk))
                {
                    if(int.TryParse(orFilterRaw, out int num))
                    {
                        orFilters.Add((new XPathAttrIndex(false), new FilterComparerInt(FilterComparer.Equal, num)));
                    }
                    else if (chunk == "last()")
                    {
                        orFilters.Add((new XPathAttrIndex(true), new FilterComparerInt(FilterComparer.Equal, -1)));
                    }
                    else
                    {
                        (XPathAttrBase attrBase, string left) = XPathAttrBase.Parser(chunk);
                        orFilters.Add((attrBase, FilterComparerBase.Parser(left)));
                    }
                }

                yield return orFilters;
            }
        }

        private static IEnumerable<string> SplitOrFilter(string chunk)
        {
            StringBuilder singleQuote = null;
            StringBuilder doubleQuote = null;

            StringBuilder orChunk = new StringBuilder();

            List<char> chars = new List<char>(chunk);

            int i = 0;
            while (i < chars.Count)
            {
                char curChar = chars[i];
                if (curChar == '\'')
                {
                    if(doubleQuote == null)
                    {
                        if (singleQuote == null)
                        {
                            singleQuote = new StringBuilder();
                            singleQuote.Append(curChar);
                        }
                        else
                        {
                            singleQuote.Append(curChar);
                            orChunk.Append(singleQuote.ToString());
                            singleQuote = null;
                        }
                    }
                    else
                    {
                        doubleQuote.Append(curChar);
                    }
                }
                else if (curChar == '"')
                {
                    if(singleQuote == null)
                    {
                        if (doubleQuote == null)
                        {
                            doubleQuote = new StringBuilder();
                            doubleQuote.Append(curChar);
                        }
                        else
                        {
                            doubleQuote.Append(curChar);
                            orChunk.Append(doubleQuote.ToString());
                            doubleQuote = null;
                        }
                    }
                    else
                    {
                        singleQuote.Append(curChar);
                    }
                }
                else
                {
                    bool curIsEmptySpace = curChar == ' ';
                    bool has3MoreChars = i + 3 < chars.Count;
                    if(curIsEmptySpace && has3MoreChars && chars[i + 1] == 'o' && chars[i + 2] == 'r' && chars[i + 3] == ' ')
                    {
                        if (singleQuote != null)
                        {
                            orChunk.Append(singleQuote.ToString());
                            singleQuote = null;
                        }
                        else if (doubleQuote != null)
                        {
                            orChunk.Append(doubleQuote.ToString());
                            doubleQuote = null;
                        }

                        yield return orChunk.ToString().Trim();
                        orChunk.Clear();
                        i += 3;
                    }
                    else
                    {
                        orChunk.Append(curChar);
                    }
                }

                i++;
            }

            string orChunkLeft = orChunk.ToString();
            if (!string.IsNullOrWhiteSpace(orChunkLeft))
            {
                yield return orChunkLeft;
            }
        }
    }
}
#endif
