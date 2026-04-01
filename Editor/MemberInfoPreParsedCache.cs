using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor
{
    public class MemberInfoPreParsedCache: ScriptableSingleton<MemberInfoPreParsedCache>
    {
        [Serializable]
        public enum MemberType
        {
            Field,
            Property,
            Method,
            Event,
        }

        [Serializable]
        public struct MemberContainer
        {
            public MemberType type;
            public string name;
            public string[] arguments;
            public string returnType;

            public MemberContainer(MemberType type, string name)
            {
                this.type = type;
                this.name = name;
                arguments = null;
                returnType = null;
            }

            public MemberContainer(string name, IEnumerable<string> arguments, string returnType)
            {
                type = MemberType.Method;
                this.name = name;
                this.arguments = arguments.ToArray();
                this.returnType = returnType;
            }
        }

        [Serializable]
        public struct FileInfo
        {
            public long lastWriteTime;
            public MemberContainer[] memberContainers;

            public FileInfo(long lastWriteTime, MemberContainer[] memberContainers)
            {
                this.lastWriteTime = lastWriteTime;
                this.memberContainers = memberContainers;
            }
        }

        public SaintsDictionary<string, FileInfo> nameToFileInfo = new SaintsDictionary<string, FileInfo>();

        public static string GetMemberInfoEssentialId(MemberInfo memberInfo)
        {
            if (memberInfo is MethodInfo methodInfo)
            {
                return $"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(each => each.ParameterType))})=>{methodInfo.ReturnType}";
            }
            return memberInfo.Name;
        }

        public SaintsDictionary<string, SaintsDictionary<string, int>> nameToMemberIdToOrder = new SaintsDictionary<string, SaintsDictionary<string, int>>();
    }
}
