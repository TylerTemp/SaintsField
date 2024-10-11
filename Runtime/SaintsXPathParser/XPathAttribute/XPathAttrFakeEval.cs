#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaintsField.SaintsXPathParser.XPathFilter;
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

            public IReadOnlyList<FilterComparerBase> ExecuteIndexer;

            public override string ToString()
            {
                return $"`{ExecuteType}=>{ExecuteString}[{string.Join(" ", ExecuteIndexer)}]`";
            }
        }

        // public readonly string ComponentName;
        // public XPathAttrIndexComparer IndexComparer;

        public readonly IReadOnlyList<ExecuteFragment> ExecuteFragments;

        public XPathAttrFakeEval(string evalString)
        {
            // Debug.Log($"evalString={evalString}");
            List<ExecuteFragment> executeFragments = new List<ExecuteFragment>();

            Queue<string> evalFragmentQuery = new Queue<string>(evalString.Split('.'));
            while (evalFragmentQuery.Count > 0)
            {
                string fragmentStr = evalFragmentQuery.Dequeue();
                bool isGetComponent = fragmentStr.StartsWith("GetComponent(");
                if (isGetComponent || fragmentStr.StartsWith("GetComponents("))
                {
                    // Debug.Log($"fragmentStr={fragmentStr}");
                    // ReSharper disable once ReplaceSubstringWithRangeIndexer
                    string getComponentLeftPart = fragmentStr.Substring(fragmentStr.IndexOf('(') + 1);
                    while (!getComponentLeftPart.Contains(")"))
                    {
                        getComponentLeftPart += "." + evalFragmentQuery.Dequeue();
                    }
                    // Debug.Log($"getComponentLeftPart={getComponentLeftPart}");

                    (string getComponentTarget, string leftFragmentStr) = ReadUntilEndBracket(getComponentLeftPart, evalFragmentQuery);
                    // Debug.Log($"leftFragmentStr={leftFragmentStr}");

                    FilterComparerBase[] leftFilter = leftFragmentStr.StartsWith("[")
                        ? XPathBracketParser.ParseFilter(leftFragmentStr)
                            .Select(each => each[0].filterComparerBase)
                            .ToArray()
                        : Array.Empty<FilterComparerBase>();

                    if (isGetComponent)
                    {
                        Debug.Assert(getComponentTarget != "", fragmentStr);
                        executeFragments.Add(new ExecuteFragment
                        {
                            ExecuteType = ExecuteType.GetComponents,
                            ExecuteString = getComponentTarget,
                            ExecuteIndexer = leftFilter.Prepend(new FilterComparerInt(FilterComparer.Equal, 0)).ToArray(),
                        });
                    }
                    else // GetComponents
                    {
                        executeFragments.Add(new ExecuteFragment
                        {
                            ExecuteType = ExecuteType.GetComponents,
                            ExecuteString = string.IsNullOrEmpty(getComponentTarget)? null: getComponentTarget,
                            ExecuteIndexer = leftFilter,
                        });
                    }
                }
                else
                {
                    int indexerIndex = fragmentStr.IndexOf('(');
                    string nameFragment;
                    string filterFragment;
                    if (indexerIndex == -1)
                    {
                        nameFragment = fragmentStr;
                        filterFragment = null;
                    }
                    else
                    {
                        nameFragment = fragmentStr.Substring(0, indexerIndex);
                        filterFragment = fragmentStr.Substring(indexerIndex);
                    }

                    FilterComparerBase[] leftFilter = filterFragment ==  null
                        ? Array.Empty<FilterComparerBase>()
                        : XPathBracketParser.ParseFilter(filterFragment)
                            .Select(each => each[0].filterComparerBase)
                            .ToArray();

                    if(nameFragment == "rectComponent")
                    {
                        executeFragments.Add(new ExecuteFragment
                        {
                            ExecuteType = ExecuteType.GetComponents,
                            ExecuteString = typeof(RectTransform).FullName,
                            ExecuteIndexer = leftFilter.Prepend(new FilterComparerInt(FilterComparer.Equal, 0)).ToArray(),
                        });
                    }
                    else if (nameFragment.EndsWith("()"))
                    {
                        executeFragments.Add(new ExecuteFragment
                        {
                            ExecuteType = ExecuteType.Method,
                            // ReSharper disable once ReplaceSubstringWithRangeIndexer
                            ExecuteString = nameFragment.Substring(0, nameFragment.Length - 2),
                            ExecuteIndexer = leftFilter,
                        });
                    }
                    else
                    {
                        executeFragments.Add(new ExecuteFragment
                        {
                            ExecuteType = ExecuteType.FieldOrProperty,
                            ExecuteString = nameFragment,
                            ExecuteIndexer = leftFilter,
                        });
                    }
                }
            }

            ExecuteFragments = executeFragments;
        }

        private static (string getComponentTarget, string leftFragmentStr) ReadUntilEndBracket(string getComponentLeftPart, Queue<string> evalFragmentQuery)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Debug.Log($"getComponentLeftPart={getComponentLeftPart}");
            Queue<char> chars = new Queue<char>(getComponentLeftPart);
            while(chars.Count > 0)
            {
                char c = chars.Dequeue();
                if (c == ')')
                {
                    // Debug.Log($"return full chars as we got {c}");
                    return (stringBuilder.ToString(), new string(chars.ToArray()));
                }

                // Debug.Log($"append char {c}");

                stringBuilder.Append(c);
            }

            while (evalFragmentQuery.Count > 0)
            {
                string fragmentStr = evalFragmentQuery.Dequeue();
                int endBracketIndex = fragmentStr.IndexOf(')');
                if (endBracketIndex != -1)
                {
                    stringBuilder.Append(fragmentStr.Substring(0, endBracketIndex));
                    return (stringBuilder.ToString(), fragmentStr.Substring(endBracketIndex + 1));
                }

                stringBuilder.Append(fragmentStr);
            }

            throw new ArgumentException($"No end bracket found in `{getComponentLeftPart}` and rest parts");
        }

        public override string ToString()
        {
            return $"@:Eval{{{string.Join(".", ExecuteFragments)}}}:";
        }
    }
}
#endif
