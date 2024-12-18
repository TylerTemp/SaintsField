#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ReferencePicker
{
    [CustomPropertyDrawer(typeof(ReferencePickerAttribute))]
    public partial class ReferencePickerAttributeDrawer: SaintsPropertyDrawer
    {
        private static IEnumerable<Type> GetTypes(SerializedProperty property)
        {
            string typename = property.managedReferenceFieldTypename;
            // Debug.Log(typename);
            string[] typeSplitString = typename.Split(' ', count: 2);
            string typeAssemblyName = typeSplitString[0];
            string typeContainerSlashClass = typeSplitString[1];
            Type realType = Type.GetType($"{typeContainerSlashClass}, {typeAssemblyName}");
            // Debug.Log($"{typeContainerSlashClass} -> {typeAssemblyName} = {realType}");

            return TypeCache.GetTypesDerivedFrom(realType)
                .Prepend(realType)
                .Where(each => !each.IsSubclassOf(typeof(UnityEngine.Object)))
                .Where(each => !each.IsAbstract) // abstract classes
                .Where(each => !each.ContainsGenericParameters) // generic classes
                .Where(each => !each.IsClass || each.GetConstructor(Type.EmptyTypes) != null);
        }

        private static object CopyObj(object oldObj, object newObj)
        {
            if (newObj == null || oldObj == null)
            {
                return newObj;
            }
            // MyObject copyObject = ...
            Type type = oldObj.GetType();
            while (type != null)
            {
                UpdateForType(type, oldObj, newObj);
                type = type.BaseType;
            }

            return newObj;
        }

        private static void UpdateForType(Type type, object source, object destination)
        {
            FieldInfo[] myObjectFields = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo fi in myObjectFields)
            {
                try
                {
                    fi.SetValue(destination, fi.GetValue(source));
                }
                catch (Exception)
                {
                    // do nothing
                    // Debug.LogException(e);
                }
            }
        }
    }
}
#endif
