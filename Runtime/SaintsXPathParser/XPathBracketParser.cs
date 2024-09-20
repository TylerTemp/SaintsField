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
                if(int.TryParse(value, out int num))
                {
                    yield return (new XPathAttrIndex(false), new FilterComparerInt(FilterComparer.Equal, num));
                    continue;
                }

                (XPathAttrBase attrBase, string left) = XPathAttrBase.Parser(chunk);
                yield return (attrBase, FilterComparerBase.Parser(left));
            }
        }
    }
}
