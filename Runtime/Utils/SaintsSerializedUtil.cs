using System;
using System.Collections.Generic;
using SaintsField.SaintsSerialization;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    public static class SaintsSerializedUtil
    {
        public static SaintsSerializedProperty OnBeforeSerialize(object obj, Type type)
        {
            if (!type.IsEnum)
            {
                return new SaintsSerializedProperty();
            }

            Type underType = Enum.GetUnderlyingType(type);
            if (underType == typeof(long))
            {
                return new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.EnumLong,
                    longValue = Convert.ToInt64(obj)
                };
            }

#if UNITY_2022_1_OR_NEWER
            if (underType == typeof(ulong))
            {
                return new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.EnumULong,
                    uLongValue = Convert.ToUInt64(obj)
                };
            }
#endif

            throw new NotSupportedException($"SaintsSerializedUtil OnBeforeSerialize not support enum underlying type {underType.FullName}");
        }

        // public static void OnBeforeSerializeArray<T>(ref SaintsSerializedProperty[] toFill, ref T[] objList, Type elementType)
        // {
        //     // Debug.Log($"OnBeforeSerializeArray toFill={toFill}, objList={objList}, elementType={elementType}");
        //     // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        //     if (objList == null)
        //     {
        //         objList = Array.Empty<T>();
        //     }
        //
        //     OnBeforeSerializeCollection(ref toFill, objList, elementType);
        // }

        // public static void OnBeforeSerializeList<T>(ref SaintsSerializedProperty[] toFill, ref List<T> objList, Type elementType)
        // {
        //     // Debug.Log($"OnBeforeSerializeArray toFill={toFill}, objList={objList}, elementType={elementType}");
        //     // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        //     if (objList == null)
        //     {
        //         objList = new List<T>();
        //     }
        //
        //     OnBeforeSerializeCollection(ref toFill, objList, elementType);
        // }

        public static void OnBeforeSerializeCollection<T>(ref SaintsSerializedProperty[] toFill, IReadOnlyList<T> objList, Type elementType)
        {
            Debug.Assert(objList != null);
            bool inPlace = toFill != null && toFill.Length == objList.Count;

            SaintsSerializedProperty[] results;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (inPlace)
            {
                results = Array.Empty<SaintsSerializedProperty>();
            }
            else
            {
                results = new SaintsSerializedProperty[objList.Count];
            }

            for (int i = 0; i < objList.Count; i++)
            {
                if (inPlace)
                {
                    toFill[i]  = OnBeforeSerialize(objList[i], elementType);
                }
                else
                {
                    results[i] = OnBeforeSerialize(objList[i], elementType);
                }
            }

            if (!inPlace)
            {
                toFill = results;
            }
        }

        public static T OnAfterDeserialize<T>(SaintsSerializedProperty saintsSerializedProperty, Type targetType)
        {
            switch (saintsSerializedProperty.propertyType)
            {
                case SaintsPropertyType.EnumLong:
                    if (targetType.IsEnum)
                    {
                        return (T)Enum.ToObject(targetType, saintsSerializedProperty.longValue);
                    }
                    return (T)Convert.ChangeType(saintsSerializedProperty.longValue, targetType);
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                    if (targetType.IsEnum)
                    {
                        return (T)Enum.ToObject(targetType, saintsSerializedProperty.uLongValue);
                    }
                    return (T)Convert.ChangeType(saintsSerializedProperty.uLongValue, targetType);
#endif
                case SaintsPropertyType.Undefined:
                    return targetType.IsValueType ? (T)Activator.CreateInstance(targetType) : default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(saintsSerializedProperty.propertyType), saintsSerializedProperty.propertyType, null);
            }
        }

        public static (bool filled, T[]result) OnAfterDeserializeArray<T>(T[] toFill, SaintsSerializedProperty[] saintsSerializedProperties, Type elementType)
        {
            // Debug.Log($"OnAfterDeserializeArray toFill={toFill}, serArr={saintsSerializedProperties}, elementType={elementType}");

            bool canFill = toFill != null && toFill.Length == saintsSerializedProperties.Length;
            T[] results = new T[saintsSerializedProperties.Length];
            for (int i = 0; i < saintsSerializedProperties.Length; i++)
            {
                if (canFill)
                {
                    toFill[i] = OnAfterDeserialize<T>(saintsSerializedProperties[i], elementType);
                }
                else
                {
                    results[i] = OnAfterDeserialize<T>(saintsSerializedProperties[i], elementType);
                }
            }

            return (canFill, results);
        }

        public static (bool filled, List<T> result) OnAfterDeserializeList<T>(List<T> toFill, SaintsSerializedProperty[] saintsSerializedProperties, Type targetType)
        {
            // Debug.Log($"toFill={toFill}, serArr={saintsSerializedProperties}, targetType={targetType}");
            bool canFill = toFill != null && toFill.Count == saintsSerializedProperties.Length;
            // Debug.Log($"canFill={canFill}");
            List<T> results = new List<T>(new T[saintsSerializedProperties.Length]);
            for (int i = 0; i < saintsSerializedProperties.Length; i++)
            {
                if (canFill)
                {
                    toFill[i] = OnAfterDeserialize<T>(saintsSerializedProperties[i], targetType);
                }
                else
                {
                    results[i] = OnAfterDeserialize<T>(saintsSerializedProperties[i], targetType);
                }
            }

            // if (!canFill)
            // {
            //     toFill = results;
            // }
            return (canFill, results);
        }

//         public static void OnBeforeSerialize(List<SaintsSerializedProperty> saintsSerializedProperties, Type serContainerType)
//         {
//             List<int> toRemoveIndexes = new List<int>();
//             for (int index = 0; index < saintsSerializedProperties.Count; index++)
//             {
//                 SaintsSerializedProperty serializedProperty = saintsSerializedProperties[index];
//                 string propName = serializedProperty.name;
//                 if (serializedProperty.isProperty)
//                 {
//                     PropertyInfo propertyInfo = serContainerType.GetProperty(propName,
//                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                     if (propertyInfo == null)
//                     {
//                         toRemoveIndexes.Add(index);
//                     }
//                 }
//                 else
//                 {
//                     FieldInfo fieldInfo = serContainerType.GetField(propName,
//                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                     if (fieldInfo == null)
//                     {
//                         toRemoveIndexes.Add(index);
//                     }
//                 }
//             }
//
//             toRemoveIndexes.Reverse();
//             foreach (int removeIndex in toRemoveIndexes)
//             {
//                 Debug.LogWarning($"Saints Serialied {saintsSerializedProperties[removeIndex].name}@{removeIndex} disappeared, removed from serialized list.");
//                 saintsSerializedProperties.RemoveAt(removeIndex);
//             }
//         }
//
//         public static void OnAfterDeserialize(List<SaintsSerializedProperty> _saintsSerializedProperties, Type serContainerType, UnityEngine.Object serTarget)
//         {
//             foreach (SaintsSerializedProperty serializedProperty in _saintsSerializedProperties)
//             {
//                 string propName = serializedProperty.name;
//                 if (serializedProperty.isProperty)
//                 {
//                     PropertyInfo propertyInfo = serContainerType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                     if (propertyInfo == null)
//                     {
//                         Debug.LogWarning($"Saints Serialied {propName} not found, skip deseialization.");
//                         continue;
//                     }
//                     propertyInfo.SetValue(serTarget, GetSaintsSerializedPropertyValue(serializedProperty, GetSaintsElementType(propertyInfo.PropertyType)));
//                 }
//                 else
//                 {
//                     FieldInfo fieldInfo = serContainerType.GetField(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                     if (fieldInfo == null)
//                     {
//                         Debug.LogWarning($"Saints Serialied {propName} not found, skip deseialization.");
//                         continue;
//                     }
//                     Type elementType = GetSaintsElementType(fieldInfo.FieldType);
//                     object realValue = GetSaintsSerializedPropertyValue(serializedProperty, elementType);
//                     fieldInfo.SetValue(serTarget, realValue);
//                 }
//             }
//         }
//
//         private static Type GetSaintsElementType(Type type)
//         {
//             if (type.IsArray)
//             {
//                 return type.GetElementType();
//             }
//
//             if(type.IsInterface)
//             {
//                 if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
//                 {
//                     return type.GetGenericArguments()[0];
//                 }
//             }
//             else
//             {
//                 foreach (Type typeInterface in type.GetInterfaces())
//                 {
//                     if (typeInterface.IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeInterface))
//                     {
//                         return typeInterface.GetGenericArguments()[0];
//                     }
//                 }
//             }
//
//             // if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
//             // {
//             //     return type.GetGenericArguments()[0];
//             // }
//
//             return type;
//         }
//
//         private static Array MakeArray(Type elementType, IEnumerable values, int length)
//         {
//             var arr = Array.CreateInstance(elementType, length);
//             int index = 0;
//             foreach (object each in values)
//             {
//                 // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
//                 if (elementType.IsEnum)
//                 {
//                     arr.SetValue(Enum.ToObject(elementType, each), index);
//                 }
//                 else
//                 {
//                     arr.SetValue(Convert.ChangeType(each, elementType), index);
//                 }
//
//                 index++;
//             }
//
//             return arr;
//         }
//
//         private static object GetSaintsSerializedPropertyValue(SaintsSerializedProperty serializedProperty, Type elementType)
//         {
//             switch (serializedProperty.propertyType)
//             {
//                 case SaintsPropertyType.EnumLong:
//                 {
//                     // ReSharper disable once ConvertSwitchStatementToSwitchExpression
//                     switch (serializedProperty.collectionType)
//                     {
//                         case CollectionType.Default:
//                             return serializedProperty.longValue;
//                         case CollectionType.Array:
//                         {
//                             return MakeArray(elementType, serializedProperty.longValues, serializedProperty.longValues.Length);
//                         }
//
//                         case CollectionType.List:
//                             return serializedProperty.longValues.Length == 0
//                                 ? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))
//                                 : (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), MakeArray(elementType, serializedProperty.longValues, serializedProperty.longValues.Length));
//                         default:
//                             throw new ArgumentOutOfRangeException(nameof(serializedProperty.collectionType), serializedProperty.collectionType, null);
//                     }
//
//                 }
//                 case SaintsPropertyType.EnumULong:
//                 {
//                     // ReSharper disable once ConvertSwitchStatementToSwitchExpression
//                     switch (serializedProperty.collectionType)
//                     {
//                         case CollectionType.Default:
//                             return serializedProperty.uLongValue;
//                         case CollectionType.Array:
//                         {
//                             return MakeArray(elementType, serializedProperty.uLongValues, serializedProperty.uLongValues.Length);
//                         }
//
//                         case CollectionType.List:
//                             return serializedProperty.uLongValues.Length == 0
//                                 ? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))
//                                 : (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), MakeArray(elementType, serializedProperty.uLongValues, serializedProperty.uLongValues.Length));
//                         default:
//                             throw new ArgumentOutOfRangeException(nameof(serializedProperty.collectionType), serializedProperty.collectionType, null);
//                     }
//                 }
//                 case SaintsPropertyType.Other:
//                 default:
//                     throw new ArgumentOutOfRangeException(nameof(serializedProperty.propertyType), serializedProperty.propertyType, null);
//             }
//         }
    }
}
