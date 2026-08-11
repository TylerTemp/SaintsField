using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEngine;
#if SAINTSFIELD_DEBUG
using Unity.Profiling;
#endif

namespace SaintsField.Editor
{
    public class MemberInfoComparerPreParsed : IComparer<MemberInfo>, IComparer
    {
        private const string GeneratedProviderTypeName =
            "SaintsFieldSourceParser.Generated.__SaintsFieldMemberOrderProvider_v1";
        private const string GeneratedProviderMethodName = "GetMembers";

        private delegate (int MemberType, string Name, string ReturnType, string[] ArgumentTypes)[]
            GetGeneratedMembers(string typeName);

        private static readonly Dictionary<Type, MemberInfoComparerPreParsed> TypeToPreParsedComparer =
            new Dictionary<Type, MemberInfoComparerPreParsed>();
        private static readonly Dictionary<Assembly, GetGeneratedMembers> AssemblyToProvider =
            new Dictionary<Assembly, GetGeneratedMembers>();

        public static MemberInfoComparerPreParsed GetComparer(Type systemType)
        {
#if SAINTSFIELD_DEBUG
            using ProfilerMarker.AutoScope autoScope = new ProfilerMarker("MemberInfoComparerPreParsed.GetComparer").Auto();
#endif
            if (TypeToPreParsedComparer.TryGetValue(systemType, out MemberInfoComparerPreParsed cache))
            {
                return cache;
            }

            Assembly assembly = systemType.GetTypeInfo().Assembly;
            GetGeneratedMembers provider = GetProvider(assembly);
            if (provider == null)
            {
                return TypeToPreParsedComparer[systemType] = null;
            }

            string typeName = GetTypeMetadataName(systemType);
            (int MemberType, string Name, string ReturnType, string[] ArgumentTypes)[] generatedMembers =
                provider(typeName);
            (bool found, MemberInfoPreParsedCache.MemberContainer[] memberContainers) =
                TryCreateMemberContainers(generatedMembers);
            if (!found)
            {
                return TypeToPreParsedComparer[systemType] = null;
            }

            string cacheKey = $"{assembly.FullName}|{typeName}";
            MemberInfoPreParsedCache.instance.nameToMemberIdToOrder.Remove(cacheKey);
            return TypeToPreParsedComparer[systemType] =
                new MemberInfoComparerPreParsed(cacheKey, memberContainers);
        }

        private static GetGeneratedMembers GetProvider(Assembly assembly)
        {
            if (AssemblyToProvider.TryGetValue(assembly, out GetGeneratedMembers cachedProvider))
            {
                return cachedProvider;
            }

            Type providerType = assembly.GetType(GeneratedProviderTypeName, false);
            MethodInfo providerMethod = providerType?.GetMethod(
                GeneratedProviderMethodName,
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null);

            GetGeneratedMembers provider = providerMethod == null
                ? null
                : (GetGeneratedMembers)Delegate.CreateDelegate(typeof(GetGeneratedMembers), providerMethod);
            AssemblyToProvider[assembly] = provider;
            return provider;
        }

        private static string GetTypeMetadataName(Type systemType)
        {
            Type typeDefinition = systemType.IsGenericType
                ? systemType.GetGenericTypeDefinition()
                : systemType;

            Stack<string> containingTypeNames = new Stack<string>();
            for (Type current = typeDefinition; current != null; current = current.DeclaringType)
            {
                containingTypeNames.Push(current.Name);
            }

            string typeName = string.Join(".", containingTypeNames);
            return string.IsNullOrEmpty(typeDefinition.Namespace)
                ? typeName
                : $"{typeDefinition.Namespace}.{typeName}";
        }

        private static (bool Found, MemberInfoPreParsedCache.MemberContainer[] Result) TryCreateMemberContainers(
            (int MemberType, string Name, string ReturnType, string[] ArgumentTypes)[] generatedMembers)
        {
            if (generatedMembers == null)
            {
                return (false, null);
            }

            MemberInfoPreParsedCache.MemberContainer[] memberContainers =
                new MemberInfoPreParsedCache.MemberContainer[generatedMembers.Length];
            for (int index = 0; index < generatedMembers.Length; index++)
            {
                (int MemberType, string Name, string ReturnType, string[] ArgumentTypes) generatedMember =
                    generatedMembers[index];
                MemberInfoPreParsedCache.MemberType memberType =
                    (MemberInfoPreParsedCache.MemberType)generatedMember.MemberType;
                if (memberType == MemberInfoPreParsedCache.MemberType.Method)
                {
                    memberContainers[index] = new MemberInfoPreParsedCache.MemberContainer(
                        generatedMember.Name,
                        generatedMember.ArgumentTypes ?? Array.Empty<string>(),
                        generatedMember.ReturnType);
                }
                else
                {
                    memberContainers[index] =
                        new MemberInfoPreParsedCache.MemberContainer(memberType, generatedMember.Name);
                }
            }

            return (true, memberContainers);
        }

        private readonly string _cacheKey;
        private readonly IReadOnlyList<MemberInfoPreParsedCache.MemberContainer> _memberContainers;
        private SaintsDictionary<string, int> _memberIdToOrderCache;

        private MemberInfoComparerPreParsed(string cacheKey, IReadOnlyList<MemberInfoPreParsedCache.MemberContainer> memberContainers)
        {
            _cacheKey = cacheKey;
            if (MemberInfoPreParsedCache.instance.nameToMemberIdToOrder.TryGetValue(cacheKey,
                    out SaintsDictionary<string, int> memberIdToOrder))
            {
                _memberIdToOrderCache = memberIdToOrder;
            }
            _memberContainers = memberContainers;
        }

        // private readonly Dictionary<MemberInfo, int> _cachedMemberInfoToIndex = new Dictionary<MemberInfo, int>();

        public int Compare(MemberInfo x, MemberInfo y)
        {
            Debug.Assert(x != null);
            Debug.Assert(y != null);

            int aIndex = GetMemberInfoIndex(x);
            // Debug.Log($"MemberOrderComparer {a.Name} index {aIndex}");
            int bIndex = GetMemberInfoIndex(y);
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

        private int GetMemberInfoIndex(MemberInfo x)
        {
            string aId = MemberInfoPreParsedCache.GetMemberInfoEssentialId(x);
            if (_memberIdToOrderCache != null)
            {
// #if SAINTSFIELD_DEBUG
//                 Debug.Log($"Use cached field order for {aId}");
// #endif
                lock(MemberInfoPreParsedCache.instance)
                {
                    if (_memberIdToOrderCache.TryGetValue(aId, out int aValue))
                    {
                        return aValue;
                    }
                }
            }

            int aIndex = FindMemberIndex(x, _memberContainers);
            lock(MemberInfoPreParsedCache.instance)
            {
                if (_memberIdToOrderCache == null)
                {
                    _memberIdToOrderCache = MemberInfoPreParsedCache.instance.nameToMemberIdToOrder[_cacheKey] =
                        new SaintsDictionary<string, int>
                        {
                            { aId, aIndex },
                        };

                }
                else
                {
                    _memberIdToOrderCache[aId] = aIndex;
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MEMBER_ORDER
            Debug.Log($"Use new field order for {aId}@{aIndex}");
#endif
            return aIndex;
        }

        private static int FindMemberIndex(MemberInfo memberInfo,
            IReadOnlyList<MemberInfoPreParsedCache.MemberContainer> codeAnalysisMembers)
        {
            // Debug.Log($"looking for member {memberInfo.Name}");

            int fallbackIndex = -1;

            for (int index = 0; index < codeAnalysisMembers.Count; index++)
            {
                MemberInfoPreParsedCache.MemberContainer memberContainer = codeAnalysisMembers[index];

                if (memberContainer.name != memberInfo.Name && !RuntimeUtil.IsAutoPropertyNoAlloc(memberContainer.name, memberInfo.Name))
                {
                    // Debug.Log($"{memberInfo.Name} not found, continue");
                    continue;
                }

                if(memberInfo.MemberType != MemberTypes.Method)  // field or property, just name is enough
                {
                    // Debug.Log($"return {memberInfo.Name} as {index}");
                    return index;
                }

                if (memberContainer.type != MemberInfoPreParsedCache.MemberType.Method)
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
                if (!TypeStringEqual(methodInfo.ReturnType, memberContainer.returnType))
                {
                    // Debug.Log($"{memberInfo.Name} not matched return type {methodInfo.ReturnType}->{memberContainer.ReturnType}, continue");
                    continue;
                }

                if (methodInfo.GetParameters().Length != memberContainer.arguments.Length)
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
                    if(!TypeStringEqual(parameterInfos[paramIndex].ParameterType, memberContainer.arguments[paramIndex]))
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
