using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class GetComponentByPathAttribute: GetByXPathAttribute
    {
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

        public struct Token
        {
            // ReSharper disable InconsistentNaming
            public Locate Locate;
            public string Node;
            // this is not index, but at this point it's OK.
            // e.g. [1], [last()], [*](has children), [position()>2], ...
            // at this point just number, last()
            // empty string for no index (do NOT use null)
            public string Index;
            // ReSharper enable InconsistentNaming

#if UNITY_EDITOR
            public override string ToString() => $"{Locate}::{EditorNodeToString(Node)}::{Index}";
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

        // ReSharper disable InconsistentNaming
        // public readonly Type CompType;
        public readonly IReadOnlyList<IReadOnlyList<Token>> Paths;
        public readonly IReadOnlyList<string> RawPaths;
        public readonly bool ForceResign;
        public readonly bool ResignButton = true;

        public GetComponentByPathAttribute(string path, params string[] paths)
        {
            RawPaths = paths
                .Prepend(path)
                .ToArray();
            Paths = RawPaths
                .Select(each =>
                {
                    IReadOnlyList<Token> result = ParsePath(each).ToArray();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                    Debug.Log($"ParsePath: {each} => {string.Join(", ", result)}");
#endif
                    return result;
                })
                .ToArray();

            ParseOptions(SaintsFieldConfigUtil.GetComponentByPathExp(EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker));
            ParseXPaths(paths.Prepend(path).Select(TranslatePath).ToArray());
        }

        public GetComponentByPathAttribute(EXP config, string path, params string[] paths)
        {
            ParseOptions(config);
            ParseXPaths(paths.Prepend(path).Select(TranslatePath).ToArray());
        }

        public GetComponentByPathAttribute(EGetComp config, string path, params string[] paths): this(TranslateConfig(config), path, paths)
        {
            ForceResign = config.HasFlag(EGetComp.ForceResign);
            ResignButton = !config.HasFlag(EGetComp.NoResignButton);
            RawPaths = paths
                .Prepend(path)
                .ToArray();
            Paths = RawPaths
                .Select(each =>
                {
                    IReadOnlyList<Token> result = ParsePath(each).ToArray();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                    Debug.Log($"ParsePath: {each} => {string.Join(", ", result)}");
#endif
                    return result;
                })
                .ToArray();
        }

        private static string TranslatePath(string path)
        {
            if (path.StartsWith("///"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return $"scene:://{path.Substring(3)}";
            }

            if (path.StartsWith("//"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return $"scene::/{path.Substring(2)}";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (path.StartsWith("/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return $"scene::/{path.Substring(1)}";
            }

            return path;
        }

        private static EXP TranslateConfig(EGetComp config)
        {
            EXP exp = EXP.NoPicker;
            if (config.HasFlag(EGetComp.ForceResign))
            {
                // do nothing
            }
            else
            {
                exp |= EXP.NoAutoResignToValue;
                exp |= EXP.NoAutoResignToNull;
            }

            if (config.HasFlag(EGetComp.NoResignButton))
            {
                exp |= EXP.NoResignButton;
            }

            return exp;
        }


        private static IEnumerable<Token> ParsePath(string path)
        {
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
                Match rootMatch = Regex.Match(path, $"^/([^/]+)");
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
    }
}
