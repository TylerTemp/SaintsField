using System;
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
            Field = 0,
            Property = 1,
            Method = 2,
            Event = 3,
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

            public MemberContainer(string name, string[] arguments, string returnType)
            {
                type = MemberType.Method;
                this.name = name;
                this.arguments = arguments;
                this.returnType = returnType;
            }
        }

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
