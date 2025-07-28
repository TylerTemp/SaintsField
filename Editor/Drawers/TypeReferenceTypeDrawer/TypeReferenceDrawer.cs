using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TypeReferenceTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(TypeReference), true)]
    // [CustomPropertyDrawer(typeof(TypeReferenceAttribute), true)]
    public partial class TypeReferenceDrawer: SaintsPropertyDrawer
    {
        private const string PropNameTypeNameAndAssembly = "_typeNameAndAssembly";
        private const string PropNameMonoScriptGuid = "_monoScriptGuid";

        private static IReadOnlyList<Assembly> _allAssemblies;

        public static IEnumerable<Assembly> GetAssemblyOfName(IReadOnlyList<string> names)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_allAssemblies == null)
            {
                _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (Assembly assembly in _allAssemblies)
            {
                string assemblyName = assembly.GetName().Name;
                if (names.Contains(assemblyName))
                {
                    yield return assembly;
                }
            }
        }

        public static IEnumerable<Assembly> GetAssembly(TypeReferenceAttribute typeReferenceAttribute, object parent)
        {
            HashSet<Assembly> yieldAss = new HashSet<Assembly>();
            // EType.Current
            // ReSharper disable once MergeIntoPattern
            // ReSharper disable once MergeSequentialChecks
            if (typeReferenceAttribute != null && typeReferenceAttribute.OnlyAssemblies != null)
            {
                foreach (Assembly assembly in GetAssemblyOfName(typeReferenceAttribute.OnlyAssemblies))
                {
                    if (!yieldAss.Add(assembly))
                    {
                        continue;
                    }
                    string assemblyName = assembly.GetName().Name;
                    if (typeReferenceAttribute.OnlyAssemblies.Contains(assemblyName))
                    {
                        yield return assembly;
                    }
                }
                yield break;
            }

            EType eTypeFilter = typeReferenceAttribute?.EType ?? EType.Current;

            if (eTypeFilter.HasFlagFast(EType.CurrentOnly))
            {
                Assembly parentAss = parent.GetType().Assembly;
                if (yieldAss.Add(parentAss))
                {
                    yield return parentAss;
                }
            }

            if (eTypeFilter.HasFlagFast(EType.CurrentReferenced))
            {
                foreach (AssemblyName assemblyName in parent.GetType().Assembly.GetReferencedAssemblies())
                {
                    Assembly ass = Assembly.Load(assemblyName);
                    if (yieldAss.Add(ass))
                    {
                        yield return ass;
                    }
                }
            }

            EType eTypeFullSearchFilter = eTypeFilter & EType.AllAssembly;

            // ReSharper disable once InvertIf
            if (eTypeFullSearchFilter != 0)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_allAssemblies == null)
                {
                    _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                }

                foreach (Assembly assembly in _allAssemblies)
                {
                    string assemblyName = assembly.GetName().Name;
                    if (assemblyName == "mscorlib" && eTypeFullSearchFilter.HasFlagFast(EType.MsCorLib)
                        || assemblyName == "System" && eTypeFullSearchFilter.HasFlagFast(EType.System)
                        || assemblyName == "System.Core" && eTypeFullSearchFilter.HasFlagFast(EType.SystemCore)
                        || eTypeFullSearchFilter.HasFlagFast(EType.NonEssential))
                    {
                        if (yieldAss.Add(assembly))
                        {
                            yield return assembly;
                        }
                    }
                }
            }

            // ReSharper disable once InvertIf
            if (typeReferenceAttribute?.ExtraAssemblies != null)
            {
                foreach (Assembly assembly in GetAssemblyOfName(typeReferenceAttribute.ExtraAssemblies))
                {
                    if (yieldAss.Add(assembly))
                    {
                        yield return assembly;
                    }
                }
            }
        }

        // private IReadOnlyList<Assembly> _cachedAsssemblies;
        // private readonly Dictionary<Assembly, Type[]> _cachedAsssembliesTypes = new Dictionary<Assembly, Type[]>();

        public static void FillAsssembliesTypes(IEnumerable<Assembly> assemblies, Dictionary<Assembly, Type[]> toFill)
        {
            foreach (Assembly assembly in assemblies)
            {
                if (!toFill.ContainsKey(assembly))
                {
                    toFill[assembly] = assembly.GetTypes();
                }
            }
        }

        private static AdvancedDropdownMetaInfo GetDropdownMetaInfo(Type selected, TypeReferenceAttribute typeRefAttr, IReadOnlyList<Assembly> cachedAsssemblies, IReadOnlyDictionary<Assembly, Type[]> cachedAsssembliesTypes, bool isImGui, object parent)
        {
            // ReSharper disable once MergeConditionalExpression
            EType eTypeFilter = typeRefAttr == null
                ? EType.Current
                : typeRefAttr.EType;

            AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>
            {
                {"[Null]", null},
            };

            // int count = 0;

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (cachedAsssemblies == null)
            {
                cachedAsssemblies = GetAssembly(typeRefAttr, parent).ToArray();
            }

            bool allowInternal = eTypeFilter.HasFlagFast(EType.AllowInternal);

            bool flatListRendering = !eTypeFilter.HasFlagFast(EType.GroupAssmbly | EType.GroupNameSpace);

            foreach (Assembly assembly in cachedAsssemblies)
            {
                Type[] types = cachedAsssembliesTypes[assembly];

                List<Type> visibleItems = new List<Type>(types.Length);
                List<Type> invisibleItems = new List<Type>(types.Length);

                foreach (Type type in types)
                {
                    if (type.IsVisible)
                    {
                        if(FilterSuperTypes(type, typeRefAttr?.SuperTypes))
                        {
                            visibleItems.Add(type);
                        }
                    }
                    else
                    {
                        if(allowInternal && FilterSuperTypes(type, typeRefAttr?.SuperTypes))
                        {
                            invisibleItems.Add(type);
                        }
                    }
                }

                // Debug.Log(visibleItems.Count > 0);
                // Debug.Log(invisibleItems.Count > 0);
                // Debug.Log((eTypeFilter & (EType.GroupAssmbly | EType.GroupNameSpace)));
                // Debug.Log((eTypeFilter & (EType.GroupAssmbly | EType.GroupNameSpace)) == 0);
                if (flatListRendering && cachedAsssemblies.Count > 1 && (visibleItems.Count > 0 || invisibleItems.Count > 0))
                {
                    // Debug.Log("Sep main");
                    dropdownList.AddSeparator();
                    dropdownList.Add(new AdvancedDropdownList<Type>(TypeReference.GetShortAssemblyName(assembly), true));
                    dropdownList.AddSeparator();
                }

                foreach (Type visibleItem in visibleItems)
                {
                    dropdownList.Add(FormatPath(visibleItem, eTypeFilter, isImGui), visibleItem);
                }

                // if(visibleItems.Count > 0 && invisibleItems.Count > 0)
                // {
                //     Debug.Log("Sep visible");
                //     dropdownList.AddSeparator();
                // }

                foreach (Type invisibleItem in invisibleItems)
                {
                    dropdownList.Add(FormatPath(invisibleItem, eTypeFilter, isImGui), invisibleItem);
                }
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurDisplay = selected == null
                    ? "null"
                    : FormatName(selected, isImGui),
                CurValues = selected == null? Array.Empty<object>(): new []{selected},
                DropdownListValue = dropdownList,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        private static bool FilterSuperTypes(Type type, IReadOnlyList<Type> superTypes)
        {
            if (superTypes == null || superTypes.Count == 0)
            {
                return true;
            }

            return superTypes.Any(superType => superType.IsAssignableFrom(type));
        }

        private const string GrayHtmlColor = "#7f7f7f";

        private static string FormatName(Type loadedType, bool imGui) =>
            imGui
                ? $"{loadedType.Name}:{loadedType.Namespace}({TypeReference.GetShortAssemblyName(loadedType)})"
                : $"{loadedType.Name} <color={GrayHtmlColor}>{loadedType.Namespace}({TypeReference.GetShortAssemblyName(loadedType)})</color>";

        public static string FormatPath(Type loadedType, EType eType, bool imGui)
        {
            string ass = TypeReference.GetShortAssemblyName(loadedType);
            string nameSpace = loadedType.Namespace;
            string name = loadedType.Name;

            if (eType.HasFlagFast(EType.GroupAssmbly))
            {
                if (eType.HasFlagFast(EType.GroupNameSpace))
                {
                    return $"{ass}/{nameSpace}/{name}";
                }

                return imGui
                    ? $"{ass}/{name}:{nameSpace}"
                    : $"{ass}/{name} <color={GrayHtmlColor}>{nameSpace}</color>";
            }

            if (eType.HasFlagFast(EType.GroupNameSpace))
            {
                return imGui
                    ? $"{ass}:{nameSpace}/{name}"
                    : $"<color={GrayHtmlColor}>{ass}</color>:{nameSpace}/{name}";
            }

            return imGui
                ? $"{name}:{nameSpace}({ass})"
                : $"{name} <color={GrayHtmlColor}>{nameSpace}({ass})</color>";
        }

        private static (string error, Type type) GetSelectedType(SerializedProperty property)
        {
            string typeNameAndAssembly = property.FindPropertyRelative(PropNameTypeNameAndAssembly).stringValue;
            string monoScriptGuid = property.FindPropertyRelative(PropNameMonoScriptGuid).stringValue;

            if (string.IsNullOrEmpty(typeNameAndAssembly))
            {
                return ("", null);
            }

            Type loadedType = Type.GetType(typeNameAndAssembly);
            if(loadedType == null && !string.IsNullOrEmpty(monoScriptGuid))
            {
                loadedType = TypeReference.EditorGetMonoType(monoScriptGuid);
            }

            if (loadedType == null)
            {
                // SetHelpBox(helpBox, $"No type found for `{typeNameAndAssembly}({monoScriptGuid})`");
                // return;
                return ($"No type found for `{typeNameAndAssembly}({monoScriptGuid})`", null);
            }

            return ("", loadedType);
        }

        private static TypeReference SetValue(SerializedProperty property, Type curType)
        {
            SerializedProperty typeNameAndAssemblyProp = property.FindPropertyRelative(PropNameTypeNameAndAssembly);
            SerializedProperty monoScriptGuidProp = property.FindPropertyRelative(PropNameMonoScriptGuid);
            if (curType == null)
            {
                typeNameAndAssemblyProp.stringValue = "";
                monoScriptGuidProp.stringValue = "";
                property.serializedObject.ApplyModifiedProperties();
                return new TypeReference();
            }

            typeNameAndAssemblyProp.stringValue = TypeReference.GetTypeNameAndAssembly(curType);
            monoScriptGuidProp.stringValue = TypeReference.EditorGetMonoGuid(curType);
            property.serializedObject.ApplyModifiedProperties();
            return new TypeReference(curType);
        }

        private static TypeReferenceAttribute GetTypeReferenceAttribute(IEnumerable<PropertyAttribute> allAttributes)
        {
            foreach (PropertyAttribute attr in allAttributes)
            {
                if(attr is TypeReferenceAttribute typeReferenceAttribute)
                {
                    return typeReferenceAttribute;
                }
            }

            return null;
        }
    }
}
