// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// #if SAINTSFIELD_NEWTONSOFT_JSON
// using Newtonsoft.Json;
// #endif
// using SaintsField.Editor.Playa;
// using SaintsField.Editor.Playa.Utils;
// using SaintsField.Playa;
// using UnityEditor;
// using UnityEditor.AssetImporters;
// using UnityEngine;
//
// namespace SaintsField.Editor.Utils
// {
//     public static class SaintsSerializedProcesser
//     {
// #if UNITY_2019_2_OR_NEWER && SAINTSFIELD_NEWTONSOFT_JSON && SAINTSFIELD_SERIALIZED
//         public readonly struct TypeRawInfo
//         {
//             public readonly Type Type;
//
//             public readonly string AssemblyName;
//             public readonly string Namespace;
//             public readonly string MainTypeName;
//             public readonly IReadOnlyList<string> SubTypeChains;
//
//             public TypeRawInfo(Type type, string assemblyName, string @namespace, string mainTypeName, IReadOnlyList<string> subTypeChains)
//             {
//                 Type = type;
//                 AssemblyName = assemblyName;
//                 Namespace = @namespace;
//                 MainTypeName = mainTypeName;
//                 SubTypeChains = subTypeChains;
//             }
//
//             public override string ToString()
//             {
//                 return
//                     $"<Type Name={MainTypeName}|{string.Join("+", SubTypeChains)} Space={Namespace} Ass={AssemblyName} />";
//             }
//         }
//
//         [InitializeOnLoadMethod]
//         private static void Init()
//         {
//             bool hasChange = false;
//             List<TypeRawInfo> infos = new List<TypeRawInfo>();
//             List<MonoScript> foundMonoScripts = new List<MonoScript>();
//
//             foreach (Type type in TypeCache.GetTypesWithAttribute<SaintsSerializedAttribute>())
//             {
//                 // Debug.Log($"type.FullName={type.FullName}; ass={type.Assembly.FullName}");
//                 if (string.IsNullOrEmpty(type.FullName))
//                 {
//                     continue;
//                 }
//
//                 (bool monoScriptFound, MonoScript monoScript) = ScriptInfoUtils.GetMonoScriptFromType(type);
//                 if (monoScriptFound)
//                 {
//                     foundMonoScripts.Add(monoScript);
//                 }
//
//                 List<string> typeDotNames = type.FullName.Split('.').ToList();
//                 string namespaceStr;
//                 string typeMain;
//                 List<string> subTypeChains;
//                 if (typeDotNames.Count == 1)
//                 {
//                     namespaceStr = null;
//                     typeMain = typeDotNames[0];
//                     subTypeChains = new List<string>();
//                 }
//                 else
//                 {
//                     int lastIndex = typeDotNames.Count - 1;
//                     string typeParts = typeDotNames[lastIndex];
//                     typeDotNames.RemoveAt(lastIndex);
//                     namespaceStr = string.Join(".", typeDotNames);
//                     List<string> typePlusNames = typeParts.Split('+').ToList();
//                     typeMain = typePlusNames[0];
//                     typePlusNames.RemoveAt(0);
//                     subTypeChains = typePlusNames;
//                 }
//
//                 TypeRawInfo rawInfo = new TypeRawInfo(type, type.Assembly.FullName, namespaceStr, typeMain, subTypeChains);
//                 infos.Add(rawInfo);
//             }
//
//             IEnumerable<IGrouping<(string AssemblyName, string Namespace, string MainTypeName), TypeRawInfo>> grouped = infos.GroupBy(i => (i.AssemblyName, i.Namespace, i.MainTypeName));
//             foreach (IGrouping<(string AssemblyName, string Namespace, string MainTypeName), TypeRawInfo> group in grouped)
//             {
//                 // Debug.Log($"Group: {group.Key.AssemblyName} | {group.Key.Namespace} | {group.Key.MainTypeName}");
//                 List<TypeRawInfo> groupedList = group.OrderBy(each => string.Join("+", each.SubTypeChains)).ToList();
//                 // foreach (TypeRawInfo info in groupedList)
//                 // {
//                 //     Debug.Log($"  - {info}");
//                 // }
//
//                 Type containingType =
//                     Type.GetType($"{group.Key.Namespace}.{group.Key.MainTypeName}, {group.Key.AssemblyName}");
//                 // Debug.Log(containingType);
//                 Debug.Assert(containingType != null, $"{group.Key.Namespace}.{group.Key.MainTypeName}, {group.Key.AssemblyName}");
//
//                 GenInfo entranceInfo;
//                 if (groupedList[0].SubTypeChains.Count == 0)
//                 {
//                     entranceInfo = MakeGenInfo(groupedList[0]);
//                     groupedList.RemoveAt(0);
//                 }
//                 else
//                 {
//                     entranceInfo = new GenInfo(containingType);
//                 }
//
//                 Dictionary<Type, GenInfo> typeToGenInfo = new Dictionary<Type, GenInfo>
//                 {
//                     { containingType, entranceInfo },
//                 };
//
//                 foreach (TypeRawInfo typeRawInfo in groupedList)
//                 {
//                     Debug.Assert(typeRawInfo.SubTypeChains.Count > 0, typeRawInfo);
//                     Type eachType = Type.GetType($"{typeRawInfo.Namespace}.{typeRawInfo.MainTypeName}{string.Join("", typeRawInfo.SubTypeChains.Select(each => $"+{each}"))}, {typeRawInfo.AssemblyName}");
//                     if (eachType == null)
//                     {
//                         Debug.LogWarning($"failed to find type for {typeRawInfo}");
//                         continue;
//                     }
//                     IReadOnlyList<SerializedInfo> r = SaintsEditorUtils.GetSaintsSerialized(eachType);
//                     // Debug.Log($"get {r.Count} for {eachType}: {string.Join("\n", r)}");
//
//                     Type parentType = Type.GetType($"{typeRawInfo.Namespace}.{typeRawInfo.MainTypeName}{string.Join("", typeRawInfo.SubTypeChains.SkipLast(1).Select(each => $"+{each}"))}, {typeRawInfo.AssemblyName}");
//                     if (parentType == null)
//                     {
//                         Debug.LogWarning($"failed to find the parent type for {typeRawInfo}");
//                         continue;
//                     }
//
//                     if (!typeToGenInfo.TryGetValue(parentType, out GenInfo parentInfo))
//                     {
//                         Debug.LogWarning($"failed to find the parent GenInfo for {typeRawInfo} parent={parentType}");
//                         continue;
//                     }
//
//                     GenInfo genInfo = MakeGenInfo(typeRawInfo);
//                     parentInfo.SubTypes.Add(genInfo);
//                     typeToGenInfo.Add(eachType, genInfo);
//                 }
//
//                 string assShort = group.Key.AssemblyName.Split(',')[0];
//                 string infoFile = $"Temp/SaintsField/{assShort}_{group.Key.Namespace}_{group.Key.MainTypeName}.json";
//                 string generatedFile = $"Temp/SaintsField/{assShort}_{group.Key.Namespace}_{group.Key.MainTypeName}.json.g";
//                 string infoFolder = Path.GetDirectoryName(infoFile);
//                 if (!Directory.Exists(infoFolder))
//                 {
//                     // ReSharper disable once AssignNullToNotNullAttribute
//                     Directory.CreateDirectory(infoFolder);
//                 }
//
//                 string newContent = JsonConvert.SerializeObject(entranceInfo, Formatting.Indented);
//
//                 bool needGen = true;
//                 if (File.Exists(infoFile))
//                 {
//                     string oldContent = File.ReadAllText(infoFile);
//                     needGen = oldContent != newContent;
//                 }
//
//                 // ReSharper disable once InvertIf
//                 if(needGen)
//                 {
//                     hasChange = true;
//                     File.WriteAllText(infoFile, newContent);
//                 }
//
//                 if(File.Exists(generatedFile))
//                 {
//                     File.Delete(generatedFile);
//                 }
//             }
//
//             if (hasChange)
//             {
//                 EditorApplication.delayCall += () =>
//                 {
//                     Debug.Log($"Force reload to generate code {string.Join(", ", foundMonoScripts)}");
//                     // foreach (MonoScript foundMonoScript in foundMonoScripts)
//                     // {
//                     //     AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(foundMonoScript), ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
//                     // }
//                     // EditorUtility.RequestScriptReload();
//                     // AssetDatabase.ImportAsset("Assets/SaintsField/Dll/SaintsFieldSourceGenerator.dll", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
//                 };
//             }
//         }
//
//         private static GenInfo MakeGenInfo(TypeRawInfo typeRawInfo)
//         {
//             Type type = Type.GetType($"{typeRawInfo.Namespace}.{typeRawInfo.MainTypeName}{string.Join("", typeRawInfo.SubTypeChains.Select(each => $"+{each}"))}, {typeRawInfo.AssemblyName}");
//             if (type == null)
//             {
//                 throw new Exception($"failed to find type for {typeRawInfo}");
//             }
//             IReadOnlyList<SerializedInfo> r = SaintsEditorUtils.GetSaintsSerialized(type);
//             // Debug.Log($"get {r.Count} for {type}: {string.Join("\n", r)}");
//             GenInfo genInfo = new GenInfo(type)
//             {
//                 SerializedInfos = r,
//             };
//             return genInfo;
//         }
//
//         private class GenInfo
//         {
//             public readonly string Namespace;
//             public readonly string Keyword;
//             public readonly bool IsStruct;
//             public readonly string Name;
//             public IReadOnlyList<SerializedInfo> SerializedInfos = Array.Empty<SerializedInfo>();
//             public readonly List<GenInfo> SubTypes = new List<GenInfo>();
//
//             public GenInfo(Type type)
//             {
//                 Namespace = type.Namespace;
//                 Keyword = GetTypeKeyword(type);
//                 IsStruct = type.IsValueType;
//                 Name = type.Name;
//             }
//
//             public override string ToString()
//             {
//                 return
//                     $"<GenInfo name={Name} isStruct={IsStruct} keyword={Keyword} ser={string.Join(", ", SerializedInfos)} subTypes={string.Join(", ", SubTypes)}/>";
//             }
//         }
//
//         private static string GetTypeKeyword(Type type)
//         {
//             if (type.IsPublic)
//                 return "public";
//             // else if (type.IsNotPublic)
//             //     Console.WriteLine("internal (top-level)");
//             if (type.IsNestedPublic)
//                 // Console.WriteLine("nested public");
//                 return "public";
//             if (type.IsNestedPrivate)
//                 // Console.WriteLine("nested private");
//                 return "private";
//             if (type.IsNestedFamily)
//                 // Console.WriteLine("nested protected");
//                 return "protected";
//             if (type.IsNestedAssembly)
//                 // Console.WriteLine("nested internal");
//                 return "internal";
//             if (type.IsNestedFamORAssem)
//                 return "protected internal";
//             if (type.IsNestedFamANDAssem)
//                 return "private protected";
//             // ReSharper disable once ConvertIfStatementToReturnStatement
//             if (type.IsNotPublic) //top-level but not public
//                 return "internal";
//             return null;
//         }
// #endif
//     }
// }
