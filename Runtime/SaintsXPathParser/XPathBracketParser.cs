#if UNITY_EDITOR
using System.Collections.Generic;
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

        public static IEnumerable<(XPathAttrBase attrBase, FilterComparerBase filterComparerBase)> ParseFilter(string value)
        {
            // Debug.Log($"get filter raw {value}");
            foreach (string chunk in Parse(value, '[', ']'))
            {
                if(int.TryParse(chunk, out int num))
                {
                    // Debug.Log($"yield index attribute {num}");
                    yield return (new XPathAttrIndex(false), new FilterComparerInt(FilterComparer.Equal, num));
                    continue;
                }

                if (chunk == "last()")
                {
                    yield return (new XPathAttrIndex(true), new FilterComparerInt(FilterComparer.Equal, -1));
                    continue;
                }

                (XPathAttrBase attrBase, string left) = XPathAttrBase.Parser(chunk);
                // Debug.Log($"yield {attrBase} {left}");
                yield return (attrBase, FilterComparerBase.Parser(left));
            }
        }
    }
}
#endif
