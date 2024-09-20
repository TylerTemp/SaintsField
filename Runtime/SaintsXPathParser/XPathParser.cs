using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaintsField.SaintsXPathParser.XPathAttribute;

// ReSharper disable ReplaceSubstringWithRangeIndexer


namespace SaintsField.SaintsXPathParser
{
    public static class XPathParser
    {
        public static IEnumerable<XPathStep> Parse(string xPath)
        {
            foreach ((int stepSep, string stepContent) in SplitXPath(xPath))
            {
                (string axisNameRaw, string attrRaw, string nodeTestRaw, string predicatesRaw) = SplitStep(stepContent);
                AxisName axisName = ParseAxisName(axisNameRaw);
                XPathAttrBase attr = string.IsNullOrEmpty(attrRaw) ? null : XPathAttrBase.Parser(attrRaw);
                NodeTest nodeTest = ParseNodeTest(nodeTestRaw);
                IReadOnlyList<XPathPredicate> predicates = string.IsNullOrEmpty(predicatesRaw)
                    ? Array.Empty<XPathPredicate>()
                    : XPathBracketParser
                        .ParseFilter(predicatesRaw)
                        .Select(each => new XPathPredicate
                        {
                            Attr = each.attrBase,
                            FilterComparer = each.filterComparerBase,
                        })
                        .ToArray();

                yield return new XPathStep
                {
                    SepCount = stepSep,
                    AxisName = axisName,
                    NodeTest = nodeTest,
                    Attr = attr,
                    Predicates = predicates,
                };
            }
        }

        private static NodeTest ParseNodeTest(string nodeTestRaw)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (nodeTestRaw)
            {
                case null:
                case "" :
                    return NodeTest.None;
                case "::ancestor":
                    return NodeTest.Ancestor;
                case "::ancestor-inside-prefab":
                    return NodeTest.AncestorInsidePrefab;
                case "::ancestor-or-self":
                    return NodeTest.AncestorOrSelf;
                case "::ancestor-or-self-inside-prefab":
                    return NodeTest.AncestorOrSelfInsidePrefab;
                case "::parent":
                    return NodeTest.Parent;
                case "::parent-or-self":
                    return NodeTest.ParentOrSelf;
                case "::parent-or-self-inside-prefab":
                    return NodeTest.ParentOrSelfInsidePrefab;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeTestRaw), nodeTestRaw, null);
            }
        }

        private static IEnumerable<(int stepSep, string stepContent)> SplitXPath(string xPath)
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

        private static (string axisName, string attr, string nodeTest, string predicates) SplitStep(string step)
        {
            int nodeTestSepIndex = step.IndexOf("::", StringComparison.Ordinal);
            int attrSepIndex = step.IndexOf('@');

            if (nodeTestSepIndex == -1 && attrSepIndex == -1)
            {
                return (step, "", "", "");
            }

            if (nodeTestSepIndex != -1 && attrSepIndex != -1)
            {
                if (nodeTestSepIndex < attrSepIndex)
                {
                    attrSepIndex = -1;
                }
                else
                {
                    nodeTestSepIndex = -1;
                }
            }

            if (nodeTestSepIndex != -1)
            {
                string axisName = step.Substring(0, nodeTestSepIndex);
                string nodeTestAndPredicates = step.Substring(nodeTestSepIndex);
                (string nodeTest, string predicates) = SplitPredicates(nodeTestAndPredicates);
                return (axisName, "", nodeTest, predicates);
            }
            else
            {
                string axisName = step.Substring(0, attrSepIndex);
                string attrAndPredicates = step.Substring(attrSepIndex);
                (string attr, string predicates) = SplitPredicates(attrAndPredicates);
                return (axisName, attr, "", predicates);
            }
        }

        private static (string preText, string predicates) SplitPredicates(string text)
        {
            int squareQuoteStart = text.IndexOf('[', StringComparison.Ordinal);
            if (squareQuoteStart == -1)
            {
                return (text, "");
            }

            string preText = text.Substring(0, squareQuoteStart);
            string predicatesText = text.Substring(squareQuoteStart);

            return (preText, predicatesText);
        }

        private static AxisName ParseAxisName(string axisNameRaw)
        {
            // ReSharper disable once MergeIntoLogicalPattern
            if (axisNameRaw == "" || axisNameRaw == "*")
            {
                return new AxisName
                {
                    NameAny = true,
                };
            }

            List<string> rawFragments = axisNameRaw.Split('*').ToList();
            if (rawFragments.Count == 1)
            {
                return new AxisName
                {
                    ExactMatch = axisNameRaw,
                };
            }

            bool startsWithFragment = !axisNameRaw.StartsWith("*");

            string startsWithStr = null;
            if (startsWithFragment)
            {
                startsWithStr = rawFragments[0];
                rawFragments.RemoveAt(0);
            }

            if(rawFragments.Count == 0)
            {
                return new AxisName
                {
                    StartsWith = startsWithStr,
                };
            }

            int lastIndex = rawFragments.Count - 1;
            string endsLeftValue = rawFragments[lastIndex];
            rawFragments.RemoveAt(lastIndex);
            bool endsWithFragment = !endsLeftValue.EndsWith("*");

            return new AxisName
            {
                StartsWith = startsWithStr,
                EndsWith = endsWithFragment? endsLeftValue: null,
                Contains = rawFragments,
            };
        }
    }
}

// ReSharper enable ReplaceSubstringWithRangeIndexer
