#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaintsField.SaintsXPathParser.XPathAttribute;
using UnityEngine;

// ReSharper disable ReplaceSubstringWithRangeIndexer


namespace SaintsField.SaintsXPathParser
{
    public static class XPathParser
    {
        public static IEnumerable<XPathStep> Parse(string xPath)
        {
            foreach ((int stepSep, string stepContent) in SplitXPath(xPath))
            {
                (Axis axis, string leftStepPart) = SplitAxisFromStep(stepContent);
                (string nodeTestRaw, string attrRaw, string predicatesRaw) = SplitStep(leftStepPart);

                NodeTest nodeTest = ParseNodeTest(nodeTestRaw);
                XPathAttrBase attr = null;
                if(!string.IsNullOrEmpty(attrRaw))
                {
                    (XPathAttrBase xPathAttrBase, string leftContent) = XPathAttrBase.Parser(attrRaw);
                    Debug.Assert(leftContent == "", attrRaw);
                    attr = xPathAttrBase;
                }

                IReadOnlyList<IReadOnlyList<XPathPredicate>> predicates = string.IsNullOrEmpty(predicatesRaw)
                    ? Array.Empty<IReadOnlyList<XPathPredicate>>()
                    : XPathBracketParser
                        .ParseFilter(predicatesRaw)
                        .Select(each => each
                            .Select(item => new XPathPredicate
                            {
                                Attr = item.attrBase,
                                FilterComparer = item.filterComparerBase,
                            })
                            .ToArray()
                        )
                        .ToArray();

                yield return new XPathStep
                {
                    SepCount = stepSep,
                    NodeTest = nodeTest,
                    Axis = axis,
                    Attr = attr,
                    Predicates = predicates,
                };
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
                        sepCount = 1;
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

        private static (Axis axis, string leftStepPart) SplitAxisFromStep(string step)
        {
            if(step.StartsWith("ancestor::"))
            {
                return (Axis.Ancestor, step.Substring("ancestor::".Length));
            }
            if(step.StartsWith("ancestor-inside-prefab::"))
            {
                return (Axis.AncestorInsidePrefab, step.Substring("ancestor-inside-prefab::".Length));
            }
            if(step.StartsWith("ancestor-or-self::"))
            {
                return (Axis.AncestorOrSelf, step.Substring("ancestor-or-self::".Length));
            }
            if(step.StartsWith("ancestor-or-self-inside-prefab::"))
            {
                return (Axis.AncestorOrSelfInsidePrefab, step.Substring("ancestor-or-self-inside-prefab::".Length));
            }
            if(step.StartsWith("parent::"))
            {
                return (Axis.Parent, step.Substring("parent::".Length));
            }
            if(step.StartsWith("parent-or-self::"))
            {
                return (Axis.ParentOrSelf, step.Substring("parent-or-self::".Length));
            }
            if(step.StartsWith("parent-or-self-inside-prefab::"))
            {
                return (Axis.ParentOrSelfInsidePrefab, step.Substring("parent-or-self-inside-prefab::".Length));
            }
            if(step.StartsWith("scene::"))
            {
                return (Axis.Scene, step.Substring("scene::".Length));
            }
            if(step.StartsWith("prefab::"))
            {
                return (Axis.Prefab, step.Substring("prefab::".Length));
            }
            if(step.StartsWith("resources::"))
            {
                return (Axis.Resources, step.Substring("resources::".Length));
            }
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(step.StartsWith("asset::"))
            {
                return (Axis.Asset, step.Substring("asset::".Length));
            }
            return (Axis.None, step);
        }

        // for parent::nodeTest@attr[predicates], this parse `nodeTest@attr[@attr > 1]` part
        private static (string nodeTest, string attr, string predicates) SplitStep(string step)
        {
            int attrSepIndex = step.IndexOf('@');
            int bracketSepIndex = step.IndexOf('[');
            if (bracketSepIndex != -1 && bracketSepIndex < attrSepIndex)
            {
                attrSepIndex = -1;
            }

            string noPredicatesPart = step;
            string predicates = "";
            if (bracketSepIndex != -1)
            {
                noPredicatesPart = step.Substring(0, bracketSepIndex);
                predicates = step.Substring(bracketSepIndex);
            }

            string nodeTest = noPredicatesPart;
            string attr = "";
            // ReSharper disable once InvertIf
            if (attrSepIndex != -1)
            {
                nodeTest = noPredicatesPart.Substring(0, attrSepIndex);
                attr = noPredicatesPart.Substring(attrSepIndex);
            }

            return (nodeTest, attr, predicates);
        }

        private static (string preText, string predicates) SplitPredicates(string text)
        {
            int squareQuoteStart = text.IndexOf("[", StringComparison.Ordinal);
            if (squareQuoteStart == -1)
            {
                return (text, "");
            }

            string preText = text.Substring(0, squareQuoteStart);
            string predicatesText = text.Substring(squareQuoteStart);

            return (preText, predicatesText);
        }

        private static NodeTest ParseNodeTest(string axisNameRaw)
        {
            switch (axisNameRaw)
            {
                case "":
                    return new NodeTest
                    {
                        NameEmpty = true,
                    };
                case "*":
                    return new NodeTest
                    {
                        NameAny = true,
                    };
            }

            List<string> rawFragments = axisNameRaw.Split('*').ToList();
            if (rawFragments.Count == 1)
            {
                return new NodeTest
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
                return new NodeTest
                {
                    StartsWith = startsWithStr,
                };
            }

            int lastIndex = rawFragments.Count - 1;
            string endsLeftValue = rawFragments[lastIndex];
            rawFragments.RemoveAt(lastIndex);
            bool endsWithFragment = !endsLeftValue.EndsWith("*");

            return new NodeTest
            {
                StartsWith = startsWithStr,
                EndsWith = endsWithFragment? endsLeftValue: null,
                Contains = rawFragments,
            };
        }
    }
}

// ReSharper enable ReplaceSubstringWithRangeIndexer
#endif
