#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using SaintsField.SaintsXPathParser.XPathFilter;

namespace SaintsField.SaintsXPathParser
{
    // Separators inside expressions, indexers, or quoted values are not path separators.
    public static class PathSyntax
    {
        public static IEnumerable<string> Split(string text, char separator)
        {
            int start = 0;
            Stack<char> brackets = new Stack<char>();
            char quote = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quote != '\0')
                {
                    if (c == '\\')
                    {
                        i++;
                    }
                    else if (c == quote)
                    {
                        quote = '\0';
                    }
                    continue;
                }
                if (c == '\'' || c == '"')
                {
                    quote = c;
                }
                else if (c == '(' || c == '[' || c == '{')
                {
                    brackets.Push(c);
                }
                else if (c == ')' || c == ']' || c == '}')
                {
                    char opening = c == ')' ? '(' : c == ']' ? '[' : '{';
                    if (brackets.Count == 0 || brackets.Pop() != opening)
                    {
                        throw new FormatException($"Mismatched bracket in `{text}`");
                    }
                }
                else if (brackets.Count == 0 && c == separator)
                {
                    yield return text.Substring(start, i - start);
                    start = i + 1;
                }
            }
            if (brackets.Count != 0 || quote != '\0')
            {
                throw new FormatException($"Unclosed bracket or quote in `{text}`");
            }
            yield return text.Substring(start);
        }

        public static int ClosingBracket(string text, int start)
        {
            char opening = text[start];
            char closing = opening == '(' ? ')' : opening == '[' ? ']' : '}';
            int depth = 0;
            char quote = '\0';
            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                if (quote != '\0')
                {
                    if (c == '\\')
                    {
                        i++;
                    }
                    else if (c == quote)
                    {
                        quote = '\0';
                    }
                    continue;
                }
                if (c == '\'' || c == '"')
                {
                    quote = c;
                }
                else if (c == opening)
                {
                    depth++;
                }
                else if (c == closing && --depth == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string NormalizeCallback(string path)
        {
            StringBuilder result = new StringBuilder(path);
            int depth = 0;
            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                if (c == '(' || c == '[' || c == '{')
                {
                    depth++;
                }
                else if (c == ')' || c == ']' || c == '}')
                {
                    depth--;
                }
                else if (depth == 0 && c == '.' && i > 0 && i + 1 < path.Length
                         && path[i - 1] != '.' && path[i + 1] != '.'
                         && path[i - 1] != '/' && path[i + 1] != '/')
                {
                    result[i] = '/';
                }
            }
            return result.ToString();
        }
    }

    public readonly struct XPathCollectionOperation
    {
        public readonly string Content;

        public XPathCollectionOperation(string content)
        {
            Content = content.Trim();
            if (!IsCollectionOperation(Content))
            {
                throw new FormatException($"Invalid collection operation `{Content}`");
            }
            if (Content.StartsWith("index()"))
            {
                FilterComparerBase.Parser(Content.Substring("index()".Length).Trim());
            }
        }

        public static bool IsCollectionOperation(string content)
        {
            content = content.Trim();
            return content == "*" || content == "last()" || content.StartsWith("index()") || int.TryParse(content, out _)
                   || content.Length >= 2 && (content[0] == '\'' && content[content.Length - 1] == '\''
                                             || content[0] == '"' && content[content.Length - 1] == '"');
        }
    }
}
#endif
