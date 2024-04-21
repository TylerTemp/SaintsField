using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class SerializedUtils
    {
        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedObject obj, string propName)
        {
            return obj.FindProperty($"<{propName}>k__BackingField");
        }

        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedProperty property, string propName)
        {
            return property.FindPropertyRelative($"<{propName}>k__BackingField");
        }

        public struct FieldOrProp
        {
            public bool IsField;
            public FieldInfo FieldInfo;
            public PropertyInfo PropertyInfo;
        }

        public static (FieldOrProp fieldOrProp, object parent) GetFieldInfoAndDirectParent(SerializedProperty property)
        {
            string originPath = property.propertyPath;
            string[] propPaths = originPath.Split('.');
            (bool arrayTrim, IEnumerable<string> propPathSegments) = TrimEndArray(propPaths);
            if (arrayTrim)
            {
                propPaths = propPathSegments.ToArray();
            }

            object sourceObj = property.serializedObject.targetObject;
            FieldOrProp fieldOrProp = default;

            bool preNameIsArray = false;
            foreach (int propIndex in Enumerable.Range(0, propPaths.Length))
            {
                string propSegName = propPaths[propIndex];
                // Debug.Log($"check key {propSegName}");
                if(propSegName == "Array")
                {
                    preNameIsArray = true;
                    continue;
                }
                if (propSegName.StartsWith("data[") && propSegName.EndsWith("]"))
                {
                    Debug.Assert(preNameIsArray);
                    // Debug.Log(propSegName);
                    // Debug.Assert(targetProp != null);
                    preNameIsArray = false;

                    int elemIndex = Convert.ToInt32(propSegName.Substring(5, propSegName.Length - 6));

                    object useObject;

                    if(fieldOrProp.FieldInfo is null && fieldOrProp.PropertyInfo is null)
                    {
                        useObject = sourceObj;
                    }
                    else
                    {
                        useObject = fieldOrProp.IsField
                            // ReSharper disable once PossibleNullReferenceException
                            ? fieldOrProp.FieldInfo.GetValue(sourceObj)
                            : fieldOrProp.PropertyInfo.GetValue(sourceObj);
                    }

                    // Debug.Log($"Get index from obj {useObject}[{elemIndex}]");
                    sourceObj = GetValueAtIndex(useObject, elemIndex);
                    // Debug.Log($"Get index from obj [{useObject}] returns {sourceObj}");
                    fieldOrProp = default;
                    // Debug.Log($"[index={elemIndex}]={targetObj}");
                    continue;
                }

                preNameIsArray = false;

                // if (propSegName.StartsWith("<") && propSegName.EndsWith(">k__BackingField"))
                // {
                //     propSegName = propSegName.Substring(1, propSegName.Length - 17);
                // }

                // Debug.Log($"get obj {sourceObj}.{propSegName}");
                if(fieldOrProp.FieldInfo is null && fieldOrProp.PropertyInfo is null)
                {
                    fieldOrProp = GetFileOrProp(sourceObj, propSegName);
                }
                else
                {
                    sourceObj = fieldOrProp.IsField
                        // ReSharper disable once PossibleNullReferenceException
                        ? fieldOrProp.FieldInfo.GetValue(sourceObj)
                        : fieldOrProp.PropertyInfo.GetValue(sourceObj);
                    fieldOrProp = GetFileOrProp(sourceObj, propSegName);
                }
                // targetFieldName = propSegName;
                // Debug.Log($"[{propSegName}]={targetObj}");
            }

            return (fieldOrProp, sourceObj);
        }

        public static (bool trimed, IEnumerable<string> propPathSegs) TrimEndArray(IReadOnlyList<string> propPathSegments)
        {

            int usePathLength = propPathSegments.Count;

            if (usePathLength <= 2)
            {
                return (false, propPathSegments);
            }

            string lastPart = propPathSegments[usePathLength - 1];
            string secLastPart = propPathSegments[usePathLength - 2];
            bool isArray = secLastPart == "Array" && lastPart.StartsWith("data[") && lastPart.EndsWith("]");
            if (!isArray)
            {
                return (false, propPathSegments);
            }

            // old Unity does not have SkipLast
            List<string> propPaths = new List<string>(propPathSegments);
            propPaths.RemoveAt(propPaths.Count - 1);
            propPaths.RemoveAt(propPaths.Count - 1);
            return (true, propPaths);
        }

        public static (T[] attributes, object parent) GetAttributesAndDirectParent<T>(SerializedProperty property) where T : class
        {
            (FieldOrProp fieldOrProp, object sourceObj) = GetFieldInfoAndDirectParent(property);
            T[] attributes = fieldOrProp.IsField
                ? fieldOrProp.FieldInfo.GetCustomAttributes(typeof(T), true).Cast<T>().ToArray()
                : fieldOrProp.PropertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>().ToArray();
            return (attributes, sourceObj);
        }

        private static FieldOrProp GetFileOrProp(object source, string name)
        {
            Type type = source.GetType();
            // Debug.Log($"get type {type}");

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    // Debug.Log($"return field {field.Name}");
                    return new FieldOrProp
                    {
                        IsField = true,
                        PropertyInfo = null,
                        FieldInfo = field,
                    };
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    // return property.GetValue(source, null);
                    // Debug.Log($"return prop {property.Name}");
                    return new FieldOrProp
                    {
                        IsField = false,
                        PropertyInfo = property,
                        FieldInfo = null,
                    };
                }

                type = type.BaseType;
            }

            throw new Exception($"Unable to get type from {source}");
        }

        public static object GetValueAtIndex(object source, int index)
        {
            if (!(source is IEnumerable enumerable))
            {
                throw new Exception($"Not a enumerable {source}");
            }

            int searchIndex = 0;
            // Debug.Log($"start check index in {source}");
            foreach (object result in enumerable)
            {
                // Debug.Log($"check index {searchIndex} in {source}");
                if(searchIndex == index)
                {
                    return result;
                }
                searchIndex++;
            }

            throw new Exception($"Not found index {index} in {source}");
        }

        public static int PropertyPathIndex(string propertyPath)
        {
            string[] propPaths = propertyPath.Split('.');
            // ReSharper disable once UseIndexFromEndExpression
            string lastPropPath = propPaths[propPaths.Length - 1];
            if (lastPropPath.StartsWith("data[") && lastPropPath.EndsWith("]"))
            {
                return int.Parse(lastPropPath.Substring(5, lastPropPath.Length - 6));
            }

            return -1;
        }

    }
}
