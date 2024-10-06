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
                // Debug.Log($"parsed chunk {value} -> {chunk}");
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
            StringBuilder quote = null;
            char quoteChar = '\0';

            StringBuilder orChunk = new StringBuilder();

            // Debug.Log($"get string: {chunk}");

            List<char> chars = new List<char>(chunk);

            int i = 0;
            while (i < chars.Count)
            {
                char curChar = chars[i];
                // Debug.Log($"get char {i}: {curChar}");
                if (curChar == '\'' || curChar == '"' && quoteChar == curChar)
                {
                    if(quote == null)  // start quoting
                    {
                        quote = new StringBuilder();
                        quote.Append(curChar);
                        quoteChar = curChar;
                        // Debug.Log($"start quote {quoteChar}");
                    }
                    else  // end quoting
                    {
                        Debug.Assert(quote != null);
                        // Debug.Log($"end quote {quoteChar}: {curChar}");
                        quote.Append(curChar);
                        orChunk.Append(quote.ToString());
                        quote = null;
                        quoteChar = '\0';
                    }
                }
                else
                {
                    bool curIsEmptySpace = curChar == ' ';
                    bool has3MoreChars = i + 3 < chars.Count;
                    if(quote == null && curIsEmptySpace && has3MoreChars && chars[i + 1] == 'o' && chars[i + 2] == 'r' && chars[i + 3] == ' ')
                    {
                        string orResult = orChunk.ToString().Trim();
                        // Debug.Log($"yield or chunk: {orResult}");
                        yield return orResult;
                        orChunk.Clear();
                        i += 3;
                    }
                    else
                    {
                        if (quote == null)
                        {
                            // Debug.Log($"append chunk: {curChar}");
                            orChunk.Append(curChar);
                        }
                        else
                        {
                            // Debug.Log($"append quote {quoteChar}: {curChar}");
                            quote.Append(curChar);
                        }
                    }
                }

                i++;
            }

            string orChunkLeft = orChunk.ToString();
            if (!string.IsNullOrWhiteSpace(orChunkLeft))
            {
                // Debug.Log($"yield left or chunk: {orChunkLeft}");
                yield return orChunkLeft;
            }
        }
    }
}
#endif
