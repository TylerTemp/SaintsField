#if SAINTSFIELD_CODE_ANALYSIS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;

namespace SaintsField.Editor
{
    public class MemberInfoComparerCodeAnalysis : IComparer<MemberInfo>, IComparer
    {
        public static MemberInfoComparerCodeAnalysis GetComparer(Type systemType)
        {
            IReadOnlyList<CodeAnalysisUtils.MemberContainer> codeAnalysisMembers =
                ScriptInfoUtils.GetMembersCorrectOrder(systemType);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (codeAnalysisMembers.Count != 0)
            {
                return new MemberInfoComparerCodeAnalysis(codeAnalysisMembers);
            }
            return null;
        }

        private readonly IReadOnlyList<CodeAnalysisUtils.MemberContainer> _codeAnalysisMembers;

        private MemberInfoComparerCodeAnalysis(IReadOnlyList<CodeAnalysisUtils.MemberContainer> codeAnalysisMembers)
        {
            _codeAnalysisMembers = codeAnalysisMembers;
        }

        public int Compare(MemberInfo a, MemberInfo b)
        {
            int length = _codeAnalysisMembers.Count;
            if (length == 0)
            {
                return 0;  // keep order
            }

            int aIndex = FindMemberIndex(a, _codeAnalysisMembers);
            // Debug.Log($"MemberOrderComparer {a.Name} index {aIndex}");
            int bIndex = FindMemberIndex(b, _codeAnalysisMembers);
            // Debug.Log($"MemberOrderComparer {b.Name} index {bIndex}");

            // if (aIndex == -1 || bIndex == -1)
            // {
            //     // Debug.Log($"{a.Name} -> {aIndex}; {b.Name} -> {bIndex} return 0");
            //     return 0;
            // }

            if (aIndex == bIndex)
            {
                return 0;
            }
            if (aIndex == -1)
            {
                return 1;
            }
            if (bIndex == -1)
            {
                return -1;
            }

            // Debug.Log($"MemberOrderComparer {a.Name} -> {aIndex}; {b.Name} -> {bIndex}");
            return aIndex - bIndex;
            // return bIndex - aIndex;
        }

        private static int FindMemberIndex(MemberInfo memberInfo,
            IReadOnlyList<CodeAnalysisUtils.MemberContainer> codeAnalysisMembers)
        {
            // Debug.Log($"looking for member {memberInfo.Name}");

            int fallbackIndex = -1;

            for (int index = 0; index < codeAnalysisMembers.Count; index++)
            {
                CodeAnalysisUtils.MemberContainer memberContainer = codeAnalysisMembers[index];

                if (memberContainer.Name != memberInfo.Name && RuntimeUtil.GetAutoPropertyName(memberContainer.Name) != memberInfo.Name)
                {
                    // Debug.Log($"{memberInfo.Name} not found, continue");
                    continue;
                }

                if(memberInfo.MemberType != MemberTypes.Method)  // field or property, just name is enough
                {
                    // Debug.Log($"return {memberInfo.Name} as {index}");
                    return index;
                }

                if (memberContainer.Type != CodeAnalysisUtils.MemberType.Method)
                {
                    // Debug.Log($"{memberInfo.Name} not method ({memberContainer.Type}), continue");
                    continue;
                }

                MethodInfo methodInfo = (MethodInfo)memberInfo;

                if (fallbackIndex == -1)
                {
                    fallbackIndex = index;  // If nothing matches, use the first matched method order
                }

                // string methodInfoReturnTypeString = ReflectUtils.StringifyType(methodInfo.ReturnType);
                // if (methodInfoReturnTypeString != memberContainer.ReturnType)
                if (!TypeStringEqual(methodInfo.ReturnType, memberContainer.ReturnType))
                {
                    // Debug.Log($"{memberInfo.Name} not matched return type {methodInfo.ReturnType}->{memberContainer.ReturnType}, continue");
                    continue;
                }

                if (methodInfo.GetParameters().Length != memberContainer.Arguments.Count)
                {
                    // Debug.Log($"{memberInfo.Name} not matched argument length {string.Join<ParameterInfo>(", ", methodInfo.GetParameters())}->{string.Join(", ", memberContainer.Arguments)}, continue");
                    continue;
                }

                bool allMatch = true;
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (int paramIndex = 0; paramIndex < parameterInfos.Length; paramIndex++)
                {
                    // string methodInfoParamTypeString = ReflectUtils.StringifyType(parameterInfos[paramIndex].ParameterType);
                    // string containerParamTypeString = memberContainer.Arguments[paramIndex];
                    // Debug.Log($"[{paramIndex}] methodInfoParamTypeString={methodInfoParamTypeString}, containerParamTypeString={containerParamTypeString}");
                    // if(methodInfoParamTypeString != containerParamTypeString)
                    // ReSharper disable once InvertIf
                    if(!TypeStringEqual(parameterInfos[paramIndex].ParameterType, memberContainer.Arguments[paramIndex]))
                    {
                        // Debug.Log($"{memberInfo.Name} [{paramIndex}] not matched argument {parameterInfos[paramIndex].ParameterType} -> {memberContainer.Arguments[paramIndex]}, continue");
                        allMatch = false;
                        break;
                    }
                }

                if(allMatch)
                {
                    // Debug.Log($"return {memberInfo.Name} as {index}");
                    return index;
                }
            }

            return fallbackIndex;
        }

        public int Compare(object x, object y)
        {
            if (x is MemberInfo xM && y is MemberInfo yM)
            {
                return Compare(xM, yM);
            }

            return 0;
        }

        private static bool TypeStringEqual(Type type, string str)
        {
            if (type.ToString() == str)
            {
                return true;
            }

            if (type.IsArray)
            {
                if (!str.EndsWith("[]"))
                {
                    return false;
                }

                Type elementType = type.GetElementType();
                string subStr = str.Substring(0, str.Length - 2);
                // Debug.Log($"{elementType}, {subStr}");
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return TypeStringEqual(elementType, subStr);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                if(str.StartsWith("List<") && str.EndsWith(">"))
                {
                    string subStr = str.Substring("List<".Length, str.Length - "List<".Length - 1);
                    Type elementType = type.GetGenericArguments()[0];
                    return TypeStringEqual(elementType, subStr);
                }

                return false;
            }

            string reparsedTypeString = ReflectUtils.StringifyType(type);
            if (reparsedTypeString == str)
            {
                return true;
            }

            string prefixDot = $".{str}";
            // Debug.Log($"Dot: {type} -> {prefixDot}: {type.ToString().EndsWith(prefixDot)}");
            if (type.ToString().EndsWith(prefixDot))
            {
                return true;
            }

            string prefixPlus = $"+{str}";
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (type.ToString().EndsWith(prefixPlus))
            {
                return true;
            }

            return false;
        }
    }
}
#endif
