using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Editor
{
    public class MemberInfoComparerPreParsed : IComparer<MemberInfo>, IComparer
    {
        private static readonly Dictionary<Type, MemberInfoComparerPreParsed> TypeToPreParsedComparer =
            new Dictionary<Type, MemberInfoComparerPreParsed>();

        public enum MemberType
        {
            Field,
            Property,
            Method,
            Event,
        }

        private readonly struct MemberContainer
        {
            public readonly MemberType Type;
            public readonly string Name;
            public readonly IReadOnlyList<string> Arguments;
            public readonly string ReturnType;

            public MemberContainer(MemberType type, string name)
            {
                Type = type;
                Name = name;
                Arguments = null;
                ReturnType = null;
            }

            public MemberContainer(string name, IEnumerable<string> arguments, string returnType)
            {
                Type = MemberType.Method;
                Name = name;
                Arguments = arguments.ToArray();
                ReturnType = returnType;
            }
        }

        public static MemberInfoComparerPreParsed GetComparer(Type systemType)
        {
            if (TypeToPreParsedComparer.TryGetValue(systemType, out MemberInfoComparerPreParsed cache))
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"return cache for {systemType}: {cache}");
#endif
                return cache;
            }

            Assembly ass = systemType.GetTypeInfo().Assembly;
            // var nameSpace = systemType.Namespace;
            // SaintsField.Samples.Scripts.SaintsEditor.NewParserTest+Part1`2+TestNestedStructForParse`1[System.Int32,System.Int32,UnityEngine.GameObject]: SaintsField.Samples, SaintsField.Samples.Scripts.SaintsEditor
            // SaintsField.Samples.Scripts.SaintsEditor.NewParserTest+Part1`2+TestNestedStructForParse[System.Int32,System.Int32]: SaintsField.Samples, SaintsField.Samples.Scripts.SaintsEditor

            // Debug.Log($"{systemType}: {ass.GetName().Name}, {nameSpace}");
            string baseFolder = $"{SaintsFieldConfig.PreParserRelativeFolder}/{ass.GetName().Name}";
            if (!Directory.Exists(baseFolder))
            {
// #if SAINTSFIELD_DEBUG
//                 Debug.LogWarning($"folder not found {baseFolder}");
// #endif
                return null;
            }

            // Get actual file name of parse result.
            List<string> nameParts = new List<string>();
            foreach (string segTypeName in systemType.ToString().Split('+'))
            {
                string resultName = segTypeName;
                int leftBracket = resultName.IndexOf('[');
                if (leftBracket > 0)
                {
                    resultName = segTypeName[..leftBracket];
                }

                nameParts.Add(resultName);
            }

            string nameBase = string.Join(".", nameParts);

            string parsedFile = $"{baseFolder}/{nameBase}.rc";
            if (!File.Exists(parsedFile))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"not found {parsedFile}");
#endif
                return null;
            }

            // Debug.Log($"{nameBase}.rc={File.ReadAllText(parsedFile)}");
            using FileStream fs = new FileStream(
                parsedFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(fs, Encoding.UTF8);
            string versionLine = reader.ReadLine();
            if (versionLine != $"{SaintsFieldConfig.PreParserVersion}")
            {
                return TypeToPreParsedComparer[systemType] = null;
            }
            // skip the checksum line
            reader.ReadLine();

            const string fieldPrefix = "Field ";
            const string propertyPrefix = "Property ";
            const string eventPrefix = "Event ";
            const string methodPrefix = "Method ";

            List<MemberContainer> memberContainers = new List<MemberContainer>();

            // ReSharper disable once MoveVariableDeclarationInsideLoopCondition
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(fieldPrefix))
                {
                    string fieldName = line[fieldPrefix.Length..].Split('|')[1].Trim();
                    memberContainers.Add(new MemberContainer(MemberType.Field, fieldName));
                }
                else if (line.StartsWith(propertyPrefix))
                {
                    string propertyName = line[propertyPrefix.Length..].Split('|')[1].Trim();
                    memberContainers.Add(new MemberContainer(MemberType.Property, propertyName));
                }
                else if (line.StartsWith(eventPrefix))
                {
                    string eventName = line[eventPrefix.Length..].Split('|')[1].Trim();
                    memberContainers.Add(new MemberContainer(MemberType.Event, eventName));
                }
                else if (line.StartsWith(methodPrefix))
                {
                    string[] methodRaw = line[methodPrefix.Length..].Split('|');
                    string methodReturnType = methodRaw[0].Trim();
                    string methodName = methodRaw[1].Trim();

                    string[] methodArgumentsSplit = methodRaw[2].Split(';');
                    string[] methodArguments = new string[methodArgumentsSplit.Length];
                    for (int index = 0; index < methodArgumentsSplit.Length; index++)
                    {
                        methodArguments[index] = methodArgumentsSplit[index].Trim();
                    }

                    memberContainers.Add(new MemberContainer(methodName, methodArguments, methodReturnType));
                }
            }

            return new MemberInfoComparerPreParsed(memberContainers);
        }

        private readonly IReadOnlyList<MemberContainer> _memberContainers;

        private MemberInfoComparerPreParsed(IReadOnlyList<MemberContainer> memberContainers)
        {
            _memberContainers = memberContainers;
        }

        public int Compare(MemberInfo x, MemberInfo y)
        {
            Debug.Assert(x != null);
            Debug.Assert(y != null);

            int aIndex = FindMemberIndex(x, _memberContainers);
            // Debug.Log($"MemberOrderComparer {a.Name} index {aIndex}");
            int bIndex = FindMemberIndex(y, _memberContainers);
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
            IReadOnlyList<MemberContainer> codeAnalysisMembers)
        {
            // Debug.Log($"looking for member {memberInfo.Name}");

            int fallbackIndex = -1;

            for (int index = 0; index < codeAnalysisMembers.Count; index++)
            {
                MemberContainer memberContainer = codeAnalysisMembers[index];

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

                if (memberContainer.Type != MemberType.Method)
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
