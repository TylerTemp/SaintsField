#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public class XPathAttrFakeEval: XPathAttrBase
    {
        // @{GetComponent(MyComponent)...}
        // @{GetComponents(MyComponent)...}
        // @{GetComponents(MyComponent)[0]...}
        // @{GetComponents(MyComponent)[last()]...}
        // @{GetComponents(MyComponent)[index() > 1]...}
        // but `@{` and `}` are removed already

        // GetComponent(MyComponent) equals to GetComponents(MyComponent)[0]

        public enum ExecuteType
        {
            FieldOrProperty,
            Method,
            GetComponents,
            // Index,
            // DictionaryKey,
        }

        public struct ExecuteFragment
        {
            public ExecuteType ExecuteType;
            public string ExecuteString;

            public IReadOnlyList<XPathCollectionOperation> CollectionOperations;

            public override string ToString()
            {
                return $"`{ExecuteType}=>{ExecuteString}[{string.Join("][", CollectionOperations.Select(operation => operation.Content))}]`";
            }
        }

        // public readonly string ComponentName;
        // public XPathAttrIndexComparer IndexComparer;

        public readonly IReadOnlyList<ExecuteFragment> ExecuteFragments;

        public XPathAttrFakeEval(string evalString)
        {
            List<ExecuteFragment> fragments = new List<ExecuteFragment>();
            foreach (string part in PathSyntax.Split(evalString, '.'))
            {
                string text = part.Trim();
                int call = text.IndexOf('(');
                int bracket = text.IndexOf('[');
                bool hasCall = call >= 0 && (bracket < 0 || call < bracket);
                int nameEnd = hasCall ? call : bracket >= 0 ? bracket : text.Length;
                string name = text.Substring(0, nameEnd);
                if (name == "")
                {
                    throw new FormatException($"Missing member name in `{evalString}`");
                }
                string suffix = text.Substring(nameEnd);
                string typeName = null;
                ExecuteType executeType = ExecuteType.FieldOrProperty;
                bool firstComponent = name == "GetComponent";
                if (hasCall)
                {
                    int close = PathSyntax.ClosingBracket(text, call);
                    if (close < 0)
                    {
                        throw new FormatException($"Unclosed method call in `{evalString}`");
                    }
                    string args = text.Substring(call + 1, close - call - 1).Trim();
                    suffix = text.Substring(close + 1);
                    if (name == "GetComponent" || name == "GetComponents")
                    {
                        executeType = ExecuteType.GetComponents;
                        typeName = args;
                    }
                    else
                    {
                        if (args != "")
                        {
                            throw new NotSupportedException($"Method arguments are not supported in `{evalString}`");
                        }
                        executeType = ExecuteType.Method;
                    }
                }
                else if (name == "rectComponent")
                {
                    executeType = ExecuteType.GetComponents;
                    typeName = typeof(RectTransform).FullName;
                    firstComponent = true;
                }

                List<XPathCollectionOperation> operations = new List<XPathCollectionOperation>();
                if (firstComponent)
                {
                    operations.Add(new XPathCollectionOperation("0"));
                }
                foreach (string operation in XPathBracketParser.Parse(suffix, '[', ']'))
                {
                    operations.Add(new XPathCollectionOperation(operation));
                }
                fragments.Add(new ExecuteFragment
                {
                    ExecuteType = executeType,
                    ExecuteString = executeType == ExecuteType.GetComponents ? typeName : name,
                    CollectionOperations = operations,
                });
            }
            ExecuteFragments = fragments;
        }
        public override string ToString()
        {
            return $"@:Eval{{{string.Join(".", ExecuteFragments)}}}:";
        }
    }
}
#endif
