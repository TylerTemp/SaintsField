#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ReferencePicker
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ReferencePickerAttribute), true)]
    public partial class ReferencePickerAttributeDrawer : SaintsPropertyDrawer
    {
        public static IEnumerable<Type> GetTypes(SerializedProperty property)
        {
            string typename = property.managedReferenceFieldTypename;
            // Debug.Log(typename);
            string[] typeSplitString = typename.Split(' ', count: 2);
            string typeAssemblyName = typeSplitString[0];
            string typeContainerSlashClass = typeSplitString[1];
            Type realType = Type.GetType($"{typeContainerSlashClass}, {typeAssemblyName}");
            // Debug.Log($"{typeContainerSlashClass} -> {typeAssemblyName} = {realType}");

            // return TypeCache.GetTypesDerivedFrom(realType)
            //     .Prepend(realType)
            //     .Where(each => !each.IsSubclassOf(typeof(UnityEngine.Object)))
            //     .Where(each => !each.IsAbstract) // abstract classes
            //     .Where(each => !each.ContainsGenericParameters) // generic classes
            //     .Where(each => !each.IsClass || each.GetConstructor(Type.EmptyTypes) != null);
            return GetTypesDerivedFrom(realType)
                .Where(each => !each.IsSubclassOf(typeof(UnityEngine.Object)));
        }

        public static IEnumerable<Type> GetTypesDerivedFrom(Type realType)
        {
            return TypeCache.GetTypesDerivedFrom(realType)
                .Prepend(realType)
                .Where(each => !each.IsAbstract) // abstract classes
                .Where(each => !each.ContainsGenericParameters) // generic classes
                .Where(each => !each.IsClass || each.GetConstructor(Type.EmptyTypes) != null);
        }

        public static object CopyObj(object oldObj, object newObj)
        {
            if (newObj == null || oldObj == null)
            {
                return newObj;
            }
            // MyObject copyObject = ...

            (IReadOnlyList<FieldInfo> sourceFields, IReadOnlyList<PropertyInfo> sourceProperties) = GetFieldProperties(oldObj.GetType());
            (IReadOnlyList<FieldInfo> targetFields, IReadOnlyList<PropertyInfo> targetProperties) = GetFieldProperties(newObj.GetType());
            Dictionary<string, FieldInfo> targetNameToField = targetFields.ToDictionary(fieldInfo => fieldInfo.Name);
            Dictionary<string, PropertyInfo> targetNameToProperty = targetProperties.ToDictionary(propertyInfo => propertyInfo.Name);
            foreach (FieldInfo sourceField in sourceFields)
            {
                if (!targetNameToField.TryGetValue(sourceField.Name, out FieldInfo targetField))
                {
                    continue;
                }

                try
                {
                    object sourceValue = sourceField.GetValue(oldObj);
                    targetField.SetValue(newObj, sourceValue);
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(e);
#endif
                    // ignore
                }
            }

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                if (!targetNameToProperty.TryGetValue(sourceProperty.Name, out PropertyInfo targetProperty))
                {
                    continue;
                }

                if (!sourceProperty.CanRead || !targetProperty.CanRead)
                {
                    continue;
                }

                if (!targetProperty.CanWrite)
                {
                    continue;
                }

                if (sourceProperty.GetIndexParameters().Length != 0 || targetProperty.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                try
                {
                    object sourceValue = sourceProperty.GetValue(oldObj);
                    targetProperty.SetValue(newObj, sourceValue);
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(e);
#endif
                    // ignore
                }
            }


            // Type type = oldObj.GetType();
            // while (type != null)
            // {
            //     UpdateForType(type, oldObj, newObj);
            //     type = type.BaseType;
            // }

            return newObj;
        }

        private static (IReadOnlyList<FieldInfo> fieldInfos, IReadOnlyList<PropertyInfo> propertyInfos)
            GetFieldProperties(Type type)
        {
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            List<FieldInfo> fieldTargets = type.GetFields(bindAttrNormal).ToList();
            Dictionary<string, FieldInfo> backingToFieldInfo = fieldTargets
                .Where(each => each.Name.StartsWith("<") && each.Name.EndsWith(">k__BackingField"))
                .ToDictionary(each => each.Name);
            PropertyInfo[] propertyTargets = type.GetProperties(bindAttrNormal);
            foreach (PropertyInfo propertyInfo in propertyTargets)
            {
                string propName = propertyInfo.Name;
                string backingName = $"<{propName}>k__BackingField";
                if (backingToFieldInfo.TryGetValue(backingName, out FieldInfo dupInfo))
                {
                    fieldTargets.Remove(dupInfo);
                }
            }

            return (fieldTargets, propertyTargets);
        }

        private static void UpdateForType(Type type, object source, object destination)
        {
            FieldInfo[] myObjectFields = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo fi in myObjectFields)
            {
                // Debug.Log($"copy {fi.Name}");
                try
                {
                    object originalValue = fi.GetValue(source);
                    fi.SetValue(destination, originalValue);
                    // Debug.Log($"set {fi.Name}={originalValue}");
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(e);
#endif
                    // do nothing
                    // Debug.LogException(e);
                }
            }
        }
    }
}
#endif
