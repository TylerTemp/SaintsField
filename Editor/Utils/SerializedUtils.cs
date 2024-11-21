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
                    sourceObj = Util.GetValueAtIndex(useObject, elemIndex).Item2;
                    // Debug.Log($"Get index from obj `{useObject}` returns {sourceObj}");
                    fieldOrProp = default;
                    // Debug.Log($"[index={elemIndex}]={targetObj}");
                    continue;
                }

                preNameIsArray = false;

                // if (propSegName.StartsWith("<") && propSegName.EndsWith(">k__BackingField"))
                // {
                //     propSegName = propSegName.Substring(1, propSegName.Length - 17);
                // }

                // Debug.Log($"get obj {sourceObj}.{propSegName}")
                //
                if (sourceObj == null)  // TODO: better error handling
                {
                    return (default, null);
                }
                // ;
                if (!(fieldOrProp.FieldInfo is null) || !(fieldOrProp.PropertyInfo is null))
                {
                    sourceObj = fieldOrProp.IsField
                        // ReSharper disable once PossibleNullReferenceException
                        ? fieldOrProp.FieldInfo.GetValue(sourceObj)
                        : fieldOrProp.PropertyInfo.GetValue(sourceObj);
                    // Debug.Log($"get key {propSegName} sourceObj = {sourceObj}");
                }

                fieldOrProp = GetFileOrProp(sourceObj, propSegName);
                // Debug.Log($"get key {propSegName} => {(fieldOrProp.IsField ? fieldOrProp.FieldInfo.Name : fieldOrProp.PropertyInfo.Name)}");
                // targetFieldName = propSegName;
                // Debug.Log($"[{propSegName}]={targetObj}");
            }

            return (fieldOrProp, sourceObj);
        }

        public static (string error, SerializedProperty property) GetArrayProperty(SerializedProperty property)
        {
            // Debug.Log(property.propertyPath);
            string[] paths = property.propertyPath.Split('.');

            (bool arrayTrim, IEnumerable<string> propPathSegments) = TrimEndArray(paths);
            if (!arrayTrim)
            {
                return ($"{property.propertyPath} is not an array/list.", null);
            }

            string arrayPath = string.Join(".", propPathSegments);
            SerializedProperty arrayProp = property.serializedObject.FindProperty(arrayPath);
            if (arrayProp == null)
            {
                return ($"Can't find {arrayPath} on {property.serializedObject}", null);
            }

            if (!arrayProp.isArray)
            {
                return ($"{arrayPath} on {property.serializedObject} is not an array/list", null);
            }

            return ("", arrayProp);
        }

        private static (bool trimed, IEnumerable<string> propPathSegs) TrimEndArray(IReadOnlyList<string> propPathSegments)
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
            if (sourceObj is null)
            {
                return (Array.Empty<T>(), null);
            }
            // Debug.Log(fieldOrProp.IsField);
            // Debug.Log(fieldOrProp.PropertyInfo);
            // Debug.Log(fieldOrProp.PropertyInfo.GetCustomAttributes());
            // this does not work with interface type
            // Debug.Log(fieldOrProp.FieldInfo.GetCustomAttributes(typeof(ISaintsAttribute)));
            // Debug.Log(fieldOrProp.FieldInfo.GetCustomAttributes());
            T[] attributes = fieldOrProp.IsField
                ? fieldOrProp.FieldInfo.GetCustomAttributes()
                    .OfType<T>()
                    .ToArray()
                : fieldOrProp.PropertyInfo.GetCustomAttributes()
                    .OfType<T>()
                    .ToArray();
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
                    // Debug.Log($"return field {field.Name} by {name}");
                    return new FieldOrProp
                    {
                        IsField = true,
                        PropertyInfo = null,
                        FieldInfo = field,
                    };
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    // return property.GetValue(source, null);
                    // Debug.Log($"return prop {property.Name} by {name}");
                    return new FieldOrProp
                    {
                        IsField = false,
                        PropertyInfo = property,
                        FieldInfo = null,
                    };
                }

                type = type.BaseType;
            }

            throw new Exception($"Unable to get {name} from {source}");
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

        public static IEnumerable<SerializedProperty> GetPropertyChildren(SerializedProperty property)
        {
            if (property == null || string.IsNullOrEmpty(property.propertyPath))
            {
                yield break;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (SerializedProperty iterator = property.Copy())
            {
                if (!iterator.NextVisible(true))
                {
                    yield break;
                }

                do
                {
                    SerializedProperty childProperty = property.FindPropertyRelative(iterator.name);
                    yield return childProperty;
                } while (iterator.NextVisible(false));
            }
        }

        public static string GetUniqueId(SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}.{property.propertyPath}";
        }

    }
}
