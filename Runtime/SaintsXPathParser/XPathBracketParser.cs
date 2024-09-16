using System.Collections.Generic;
using System.Text;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;

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
            foreach (string chunk in Parse(value, '[', ']'))
            {
                if (chunk == "last()")
                {
                    yield return (
                        new XPathAttrIndex(),
                        new FilterComparerInt(FilterComparer.Equal, -1)
                    );
                }
                string[] parts = chunk.Split(' ', 2);

                string attributePart = parts[0];
                XPathAttrBase attrBase = XPathAttrBase.Parser(attributePart);

                if (parts.Length == 1)
                {
                    yield return (attrBase, null);
                    continue;
                }

                yield return (attrBase, FilterComparerBase.Parser(parts[1].Trim()));
            }
        }
    }
}
