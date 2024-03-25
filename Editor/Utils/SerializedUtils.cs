using System;
using System.Collections;
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

        // public static SerializedProperty FindPropertyRelativeByAutoPropertyName(SerializedProperty prop, string propName)
        // {
        //     return prop.FindPropertyRelative($"<{propName}>k__BackingField");
        // }

        // public static T GetAttribute<T>(SerializedProperty property) where T : class
        // {
        //     T[] attributes = GetAttributes<T>(property);
        //     return attributes.Length > 0 ? attributes[0] : null;
        // }

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
            int usePathLength = propPaths.Length;
            if(propPaths.Length > 2)
            {
                string lastPart = propPaths[propPaths.Length - 1];
                string secLastPart = propPaths[propPaths.Length - 2];
                bool isArray = secLastPart == "Array" && lastPart.StartsWith("data[") && lastPart.EndsWith("]");
                if (isArray)
                {
                    // Debug.Log($"use sub length {originPath}");
                    usePathLength -= 2;
                }
                // else
                // {
                //     Debug.Log($"use normal length {originPath}");
                // }
            }
            // else
            // {
            //     Debug.Log($"use normal length {originPath}");
            // }

            object sourceObj = property.serializedObject.targetObject;
            FieldOrProp fieldOrProp = default;

            bool preNameIsArray = false;
            foreach (int propIndex in Enumerable.Range(0, usePathLength))
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

            // IEnumerator enumerator = enumerable.GetEnumerator();
            // for (int i = 0; i <= index; i++)
            // {
            //     if (!enumerator.MoveNext())
            //     {
            //         return null;
            //     }
            // }
            //
            // return enumerator.Current;
        }

        // public static Type GetType(SerializedProperty prop)
        // {
        //     //gets parent type info
        //     string[] slices = prop.propertyPath.Split('.');
        //     object targetObj = prop.serializedObject.targetObject;
        //
        //     foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypes(targetObj))
        //     {
        //         // foreach (FieldInfo field in type!.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        //         // {
        //         //     Debug.Log($"name={field.Name}");
        //         // }
        //         Type getType = eachType;
        //
        //         for(int i = 0; i < slices.Length; i++)
        //         {
        //             if (slices[i] == "Array")
        //             {
        //                 i++; //skips "data[x]"
        //                 // type = type!.GetElementType(); //gets info on array elements
        //                 Debug.Assert(getType != null);
        //                 getType = getType.GetElementType();
        //             }
        //             else  //gets info on field and its type
        //             {
        //                 // Debug.Log($"{slices[i]}, {type!.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)}");
        //                 Debug.Assert(getType != null);
        //                 FieldInfo field = getType.GetField(slices[i],
        //                     BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
        //                     BindingFlags.Instance);
        //
        //                 // Debug.Log(field?.Name);
        //                 if (field != null)
        //                 {
        //                     return field.FieldType;
        //                 }
        //                 // getType =
        //                 //     !.FieldType;
        //             }
        //         }
        //
        //         //type is now the type of the property
        //         // return type;
        //     }
        //
        //     throw new Exception($"Unable to get type from {targetObj}");
        //
        //     // Type type = prop.serializedObject.targetObject.GetType()!;
        //     // Debug.Log($"{prop.propertyPath}, {type}");
        //     // foreach (FieldInfo field in type!.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        //     // {
        //     //     Debug.Log($"name={field.Name}");
        //     // }
        //     //
        //     // for(int i = 0; i < slices.Length; i++)
        //     // {
        //     //     if (slices[i] == "Array")
        //     //     {
        //     //         i++; //skips "data[x]"
        //     //         type = type!.GetElementType(); //gets info on array elements
        //     //     }
        //     //     else  //gets info on field and its type
        //     //     {
        //     //         Debug.Log($"{slices[i]}, {type!.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)}");
        //     //         type = type
        //     //             !.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
        //     //             !.FieldType;
        //     //     }
        //     // }
        //     //
        //     // //type is now the type of the property
        //     // return type;
        // }

        // public static object GetValue(SerializedProperty property)
        // {
        //     Object targetObject = property.serializedObject.targetObject;
        //     Type targetObjectClassType = targetObject.GetType();
        //     FieldInfo field = targetObjectClassType.GetField(property.propertyPath);
        //     // if (field != null)
        //     // {
        //     //     var value = field.GetValue(targetObject);
        //     //     // Debug.Log(value.s);
        //     // }
        //     Debug.Assert(field != null, $"{property.propertyPath}/{targetObject}");
        //     return field!.GetValue(targetObject);
        // }

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
