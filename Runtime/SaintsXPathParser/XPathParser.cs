using System.Collections.Generic;
using System.Text;


namespace SaintsField.SaintsXPathParser
{
    public static class XPathParser
    {
        public static IEnumerable<XPathStep> Parse(string xPath)
        {
            foreach ((int stepSep, string stepContent) in SplitSteps(xPath))
            {

            }

            return null;
        }

        private static IEnumerable<(int stepSep, string stepContent)> SplitSteps(string xPath)
        {
            StringBuilder stepBuilder = new StringBuilder();

            StringBuilder quoteBuilder = null;
            char quoteType = '\0';

            Queue<char> chars = new Queue<char>(xPath);

            int sepCount = 0;
            bool hasContent = false;

            while (chars.Count > 0)
            {
                char curChar = chars.Dequeue();
                if (curChar == '/')
                {
                    if (hasContent)  // `content/`
                    {
                        yield return (sepCount, stepBuilder.ToString());
                        sepCount = 0;
                        hasContent = false;
                        stepBuilder.Clear();
                        quoteBuilder = null;
                        quoteType = '\0';
                    }
                    else  // continued `/`
                    {
                        sepCount += 1;
                    }
                }
                else
                {
                    hasContent = true;
                    bool isSingleQuote = curChar == '\'';
                    bool isDoubleQuote = curChar == '"';
                    bool inSingleQuote = quoteType == '\'';
                    bool inDoubleQuote = quoteType == '"';

                    bool matchedQuote = (isSingleQuote && inSingleQuote) || (isDoubleQuote && inDoubleQuote);
                    if (isSingleQuote || isDoubleQuote)
                    {
                        if (quoteBuilder == null)  // new quote
                        {
                            quoteType = curChar;
                            quoteBuilder = new StringBuilder();
                            quoteBuilder.Append(curChar);
                        }
                        else  // still in quote
                        {
                            if (matchedQuote)  // same quote, now close it
                            {
                                quoteBuilder.Append(curChar);
                                stepBuilder.Append(quoteBuilder.ToString());
                                quoteBuilder = null;
                            }
                            else  // keep quoting
                            {
                                quoteBuilder.Append(curChar);
                            }
                        }
                    }
                    else  // not in any quote
                    {
                        stepBuilder.Append(curChar);
                    }
                }
            }

            if (hasContent)
            {
                if (quoteBuilder != null)
                {
                    stepBuilder.Append(quoteBuilder.ToString());
                }
                yield return (sepCount, stepBuilder.ToString());
            }
        }
    }
}
