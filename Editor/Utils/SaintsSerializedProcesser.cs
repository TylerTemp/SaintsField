using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class SaintsSerializedProcesser
    {
#if UNITY_2019_2_OR_NEWER && SAINTSFIELD_NEWTONSOFT_JSON && SAINTSFIELD_SERIALIZED
        public readonly struct TypeRawInfo
        {
            public readonly Type Type;

            public readonly string AssemblyName;
            public readonly string Namespace;
            public readonly string MainTypeName;
            public readonly IReadOnlyList<string> SubTypeChains;

            public TypeRawInfo(Type type, string assemblyName, string @namespace, string mainTypeName, IReadOnlyList<string> subTypeChains)
            {
                Type = type;
                AssemblyName = assemblyName;
                Namespace = @namespace;
                MainTypeName = mainTypeName;
                SubTypeChains = subTypeChains;
            }

            public override string ToString()
            {
                return
                    $"<Type Name={MainTypeName}|{string.Join("+", SubTypeChains)} Space={Namespace} Ass={AssemblyName} />";
            }
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            List<TypeRawInfo> infos = new List<TypeRawInfo>();
            foreach (Type type in TypeCache.GetTypesWithAttribute<SaintsSerializedAttribute>())
            {
                Debug.Log($"type.FullName={type.FullName}; ass={type.Assembly.FullName}");
                if (string.IsNullOrEmpty(type.FullName))
                {
                    return;
                }
                List<string> typeDotNames = type.FullName.Split('.').ToList();
                string namespaceStr;
                string typeMain;
                List<string> subTypeChains;
                if (typeDotNames.Count == 1)
                {
                    namespaceStr = null;
                    typeMain = typeDotNames[0];
                    subTypeChains = new List<string>();
                }
                else
                {
                    int lastIndex = typeDotNames.Count - 1;
                    string typeParts = typeDotNames[lastIndex];
                    typeDotNames.RemoveAt(lastIndex);
                    namespaceStr = string.Join(".", typeDotNames);
                    List<string> typePlusNames = typeParts.Split('+').ToList();
                    typeMain = typePlusNames[0];
                    typePlusNames.RemoveAt(0);
                    subTypeChains = typePlusNames;
                }

                TypeRawInfo rawInfo = new TypeRawInfo(type, type.Assembly.FullName, namespaceStr, typeMain, subTypeChains);
                infos.Add(rawInfo);
            }

            IEnumerable<IGrouping<(string AssemblyName, string Namespace, string MainTypeName), TypeRawInfo>> grouped = infos.GroupBy(i => (i.AssemblyName, i.Namespace, i.MainTypeName));
            foreach (IGrouping<(string AssemblyName, string Namespace, string MainTypeName), TypeRawInfo> group in grouped)
            {
                Debug.Log($"Group: {group.Key.AssemblyName} | {group.Key.Namespace} | {group.Key.MainTypeName}");
                List<TypeRawInfo> groupedList = group.OrderBy(each => string.Join("+", each.SubTypeChains)).ToList();
                foreach (TypeRawInfo info in groupedList)
                {
                    Debug.Log($"  - {info}");
                }

                Type containingType =
                    Type.GetType($"{group.Key.Namespace}.{group.Key.MainTypeName}, {group.Key.AssemblyName}");
                Debug.Log(containingType);

                string assShort = group.Key.AssemblyName.Split(',')[0];
                string infoFile = $"Temp/SaintsField/{assShort}_{group.Key.Namespace}_{group.Key.MainTypeName}.json";
                string infoFolder = Path.GetDirectoryName(infoFile);
                if (!Directory.Exists(infoFolder))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Directory.CreateDirectory(infoFolder);
                }

                File.WriteAllText(infoFile, "{}");
            }
        }
#endif
    }
}
