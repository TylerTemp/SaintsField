using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SaintsField.Playa;
using UnityEditor.Graphs.AnimationBlendTree;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class GetComponentByPathAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute, IPlayaArraySizeAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        // private static readonly Regex SlashContent = new Regex(@"(//?)([^/]+)");
        private static readonly Regex ContentSquareBracket = new Regex(@"([^[\]]*)\[([^\[\]]+)\]");

        public enum Locate
        {
            Child,      // "/"
            Descendant,       // "//"
            Root,  // "/"
            // Current,    // "."
            // other like /following-sibling::*, /preceding-sibling::*, ... is not supported
        }

        public enum StartAt
        {
            Current,
            Root,
            Assets,
        }

        public enum Axes
        {
            Child,
            Ancestor,
            AncestorOrSelf,
        }

        public enum IndexFilterType
        {
            None,
            Number,
            String,
            Index,  // index()
            Last,  // last()
        }

        public struct IndexFilter
        {
            public IndexFilterType IndexFilterType;
            public string Operator;  // >, <. == etc. "" means nothing
            public int Number;
            public string String;
        }

        public enum NodeAttributeType
        {
            None,
            AttributeSelector,  // component[index].attribute...
            Resource,  // resource()
            AssetDatabase,  // asset-database(),
            Guid,  // guid(),
        }

        public struct NodeAttribute
        {
            public NodeAttributeType NodeAttributeType;
            public string Value;
            public bool IsMethod;
            public IReadOnlyList<IndexFilter> IndexFilters;
        }

        public enum PredicateType
        {
            Index,
            Component,
        }

        public struct Predicate
        {
            public PredicateType PredicateType;
            public string Value;
        }

        // {Axes}::{Node}@{EStr}{Attribute.Property}[{Predicates}][{Predicates}]...
        public struct Token
        {
            public Axes Axes;
            public string Node;
            public NodeAttributeType NodeAttributeType;
            public string NodeAttribute;
            public IReadOnlyList<Predicate> Predicates;

            // ReSharper disable InconsistentNaming
            // public Locate Locate;
            // public Axes Axes;
            // public string Node;
            // this is not index, but at this point it's OK.
            // e.g. [1], [last()], [*](has children), [position()>2], ...
            // at this point just number, last()
            // empty string for no index (do NOT use null)
            // public string Index;
            // ReSharper enable InconsistentNaming

#if UNITY_EDITOR
            public override string ToString() => $"{Axes}::{EditorNodeToString(Node)}[{string.Join("][", Predicates)}]";
            private static string EditorNodeToString(string node)
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (node)
                {
                    case ".":
                        return "<CUR>";
                    case "..":
                        return "<PARENT>";
                    default:
                        return node;
                }
            }
#endif
        }

        public struct XPath
        {
            public StartAt StartAt;
            public IReadOnlyList<Token> Tokens;
        }

        // ReSharper disable InconsistentNaming
        // public readonly Type CompType;
        public readonly IReadOnlyList<XPath> Paths;
        // public readonly IReadOnlyList<string> RawPaths;
        public readonly bool ForceResign;
        public readonly bool ResignButton = true;
        // ReSharper enable InconsistentNaming

        public GetComponentByPathAttribute(string path, params string[] paths)
        {
            // RawPaths = paths
            //     .Prepend(path)
            //     .ToArray();
            Paths = paths
                .Prepend(path)
                .Select(ParsePath)
//                 .Select(each =>
//                 {
//                     IReadOnlyList<Token> result = ParsePath(each).ToArray();
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
//                     Debug.Log($"ParsePath: {each} => {string.Join(", ", result)}");
// #endif
//                     return result;
//                 })
                .ToArray();
        }

        public GetComponentByPathAttribute(EGetComp config, string path, params string[] paths): this(path, paths)
        {
            ForceResign = config.HasFlag(EGetComp.ForceResign);
            ResignButton = !config.HasFlag(EGetComp.NoResignButton);
        }

        // public GetComponentByPathAttribute(Type compType, EGetComp config, string path, params string[] paths): this(config, path, paths)
        // {
        //     CompType = compType;
        // }
        //
        // public GetComponentByPathAttribute(EGetComp config, Type compType, string path, params string[] paths): this(config, path, paths)
        // {
        //     CompType = compType;
        // }

        private static XPath ParsePath(string path)
        {
            StartAt startAt;
            string usePath;
            if (path.StartsWith("/"))
            {
                startAt = StartAt.Root;
                if (path.StartsWith("///"))  // root + descendant
                {
                    usePath = path.Substring(1);
                }
                else if (path.StartsWith("//"))
                {
                    usePath = path.Substring(2);
                }
                else
                {
                    usePath = path;
                }
            }
            else if (path.StartsWith(":"))
            {
                startAt = StartAt.Assets;
                usePath = path.Substring(1);
            }
            else
            {
                startAt = StartAt.Current;
                usePath = path;
            }

            foreach ((bool isAncestor, string step) in ChunkPath(usePath))
            {
                (Axes axes, string node, NodeAttributeType nodeAttributeType, string nodeAttribute, IReadOnlyList<Predicate> predicates) = ParseStep(step);
            }



            // "./sth" equals "sth", relative, so, (child::sth)

            // `//` means "anywhere", to say, "a//b" means any b descendant of a (child::a, descendant::b)
            // ".//sth" means `sth` anywhere under current. (descendant::sth)

            // "..//sth" (:parent, descendant::sth)

            // ".." means parent. "a/../b (child::a, :parent, child::b)

            // "/sth" means from root (root::sth)
            // "//sth" means `sth` directly under root (:root, child::sth), equals `/./sth`
            // "///sth" means `sth` anywhere under root (:root, descendant::sth), equals `/.//sth`

            // string processPath = (path.StartsWith("./") || path.StartsWith("/"))
            //     ? path
            //     : $"./{path}";

            // const string slashContentPattern = @"(//?)([^/]+)";

            string processPath;
            if (path.StartsWith("/"))
            {
                // /sth[index] => root::sth::[index]
                // //sth[index] => root::::, /sth
                // ///sth[index] => root::::, //sth
                // /./sth[index] => root::::, /.
                Match rootMatch = Regex.Match(path, "^/([^/]+)");
                string rootNode = "*";
                string rootIndex = "";
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                string sub = path.Substring(1);
                if (rootMatch.Success)
                {
                    string rootContent = rootMatch.Groups[1].Value;
                    Match rootContentMatch = ContentSquareBracket.Match(rootContent);
                    if (rootContentMatch.Success)
                    {
                        rootNode = rootContentMatch.Groups[1].Value;
                        rootIndex = rootContentMatch.Groups[2].Value.Trim();
                    }
                    else
                    {
                        rootNode = rootContent;
                    }

                    if (rootNode == ".")
                    {
                        rootNode = "*";
                    }

                    // ReSharper disable once ReplaceSubstringWithRangeIndexer
                    sub = path.Substring(rootMatch.Value.Length);
                }

                yield return new Token
                {
                    Locate = Locate.Root,
                    Node = rootNode,
                    Index = rootIndex,
                };

                processPath = sub.StartsWith("/")? sub: $"/{sub}";
            }
            else
            {
                // just because Regex
                processPath = $"/{path}";
            }

            MatchCollection matches = Regex.Matches(processPath, @"(//?)([^/]+)");
            foreach (Match match in matches)
            {
                string slash = match.Groups[1].Value;
                string content = match.Groups[2].Value;
                string index = string.Empty;

                Match contentMatch = ContentSquareBracket.Match(content);
                if (contentMatch.Success)
                {
                    content = contentMatch.Groups[1].Value;
                    index = contentMatch.Groups[2].Value.Trim();
                }

                Locate locate;
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (slash)
                {
                    case "//":
                        locate = Locate.Descendant;
                        break;
                    case "/":
                        locate = Locate.Child;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(slash), slash, null);
                }

                yield return new Token
                {
                    Locate = locate,
                    Node = content,
                    Index = index,
                };
            }
        }

        // AttributeSelector,  // component[index].attribute...
        // Resource,  // Resource()
        // AssetDatabase,  // AssetDatabase(),
        // Guid,  // Guid(),

        private static (Axes axes, string node, NodeAttributeType nodeAttributeType, string nodeAttribute, IReadOnlyList<Predicate> predicates) ParseStep(string step)
        {
            List<string> splits = step.Split("::").ToList();
            Axes axes = ExtractAxesFromStepFragments(splits);

            (string node, IEnumerable<NodeAttribute> nodeAttributes) =
                ExtractNodeInfoFromStepFragments(splits);
            // if (split.Length > 1)
            // {
            //     string axesName = split[0];
            //     string leftContent = string.Join("::", split.Skip(1));
            // }

        }

        // Child,
        // Ancestor,
        // AncestorOrSelf,
        private static Axes ExtractAxesFromStepFragments(List<string> splits)
        {
            string first = splits[0];
            Axes axes = Axes.Child;
            switch (first)
            {
                case "child":
                    break;
                case "ancestor":
                    axes = Axes.Ancestor;
                    break;
                case "ancestor-or-self":
                    axes = Axes.AncestorOrSelf;
                    break;
                default:
                    return axes;
            }

            splits.RemoveAt(0);
            return axes;
        }

        // for component:
        // node@component.info().attr.invoke()
        // no indexer, no function parameters
        // node@attribute[filter][filter]
        private static (string node, IEnumerable<NodeAttribute> nodeAttributes) ExtractNodeInfoFromStepFragments(List<string> splits)
        {
            if (splits.Count == 0 || splits[0].StartsWith("["))
            {
                return ("", Array.Empty<NodeAttribute>());
            }

            string first = splits[0];
            splits.RemoveAt(0);

            string[] splitAttribute = first.Split("@");
            string node = splitAttribute[0];
            if (splitAttribute.Length == 1)
            {
                return (node, Array.Empty<NodeAttribute>());
            }

            string nodeAttributeRaw = splitAttribute[1];

            switch (nodeAttributeRaw)
            {
                case "":
                    return (node, Array.Empty<NodeAttribute>());
                case "resource()":
                    return (node, new[]{new NodeAttribute
                    {
                        NodeAttributeType = NodeAttributeType.Resource,
                        Value = "",
                    }});
                case "asset-database()":
                    return (node, new []
                    {
                        new NodeAttribute
                        {
                            NodeAttributeType = NodeAttributeType.AssetDatabase,
                            Value = "",
                        }
                    });
                case "guid()":
                    return (node, new[]{new NodeAttribute
                    {
                        NodeAttributeType = NodeAttributeType.Guid,
                        Value = "",
                    }});
                default:
                    return (node, ParseNodeAttributeSelector(nodeAttributeRaw));
            }
        }

        // component.funcName()[0]["content"].value.Invoke()
        private static IEnumerable<NodeAttribute> ParseNodeAttributeSelector(string nodeAttributeRaw)
        {
            foreach (string nodeAttributeSegment in nodeAttributeRaw.Split('.'))
            {
                (string nodeAttributeContent, bool isMethod, IReadOnlyList<IndexFilter> indexFilters) =
                    ParseNodeWithFilter(nodeAttributeSegment);
                yield return new NodeAttribute
                {
                    NodeAttributeType = NodeAttributeType.AttributeSelector,
                    Value = nodeAttributeContent,
                    IsMethod = isMethod,
                    IndexFilters = indexFilters,
                };
            }
        }

        private static (string nodeAttributeContent, bool isMethod, IReadOnlyList<IndexFilter> indexFilters) ParseNodeWithFilter(string nodeAttributeSegment)
        {
            MatchCollection matches = ContentSquareBracket.Matches(nodeAttributeSegment);
            if (matches.Count == 0)
            {
                (string simpleNodeContent, bool simpleNodeIsMethod)  = ParseMethodCall(nodeAttributeSegment);

                return (simpleNodeContent, simpleNodeIsMethod, Array.Empty<IndexFilter>());
            }

            Match firstMatch = matches[0];
            string rootContent = firstMatch.Groups[0].Value;
            IndexFilter[] indexFilters = matches.Select(each => ParseIndexFilter(each.Groups[1].Value)).ToArray();

            (string nodeContent, bool nodeIsMethod)  = ParseMethodCall(rootContent);
            return (nodeContent, nodeIsMethod, indexFilters);
        }

        private static (string content, bool isMethod) ParseMethodCall(string segment)
        {
            string[] splitMethodCall = segment.Split("()");
            return splitMethodCall.Length == 1
                ? (segment, false)
                : (splitMethodCall[0], true);
        }

        private static readonly Regex SubNumberRegex = new Regex(@"([^\d\-\.]+)(.*)");

        private static IndexFilter ParseIndexFilter(string indexRaw)
        {
            if (indexRaw.StartsWith("\"") && indexRaw.EndsWith("\""))
            {
                return new IndexFilter
                {
                    IndexFilterType = IndexFilterType.String,
                    String = indexRaw.Substring(1, indexRaw.Length - 2),
                };
            }
            if (indexRaw.StartsWith("'") && indexRaw.EndsWith("'"))
            {
                return new IndexFilter
                {
                    IndexFilterType = IndexFilterType.String,
                    String = indexRaw.Substring(1, indexRaw.Length - 2),
                };
            }

            string[] bracketSplit = indexRaw.Split("()");

            if (bracketSplit.Length > 1)  // index() > sth etc
            {
                string funcPart = bracketSplit[0].Trim();
                if (funcPart == "last")  // [last()]
                {
                    return new IndexFilter
                    {
                        IndexFilterType = IndexFilterType.Last,
                    };
                }
                Debug.Assert(funcPart == "index", indexRaw);

                string comparePart = bracketSplit[1].Trim();
                Match match = SubNumberRegex.Match(comparePart);
                Debug.Assert(match.Success, indexRaw);
                string rawOperator = match.Groups[0].Value.Trim();
                int number = int.Parse(match.Groups[1].Value.Trim());

                return new IndexFilter
                {
                    IndexFilterType = IndexFilterType.Index,
                    Operator = rawOperator,
                    Number = number,
                };
            }

            // pure number
            int pureNumber = int.Parse(indexRaw.Trim());
            return new IndexFilter
            {
                IndexFilterType = IndexFilterType.Number,
                Number = pureNumber,
            };
        }

        private static IEnumerable<(bool ancestor, string segment)> ChunkPath(string usePath)
        {
            StringBuilder sb = new StringBuilder();
            bool hasContent = false;
            bool ancestor = false;
            foreach (char c in usePath)
            {
                if (c == '/')
                {
                    if (hasContent)
                    {
                        yield return (ancestor, sb.ToString());
                        sb.Clear();
                        hasContent = false;
                        ancestor = false;
                    }
                    else
                    {
                        Debug.Assert(!ancestor, sb.ToString());
                        ancestor = true;
                    }
                }
                else
                {
                    sb.Append(c);
                    hasContent = true;
                }
            }

            string leftContent = sb.ToString();
            if (!string.IsNullOrEmpty(leftContent))
            {
                yield return (ancestor, leftContent);
            }
        }
    }
}
