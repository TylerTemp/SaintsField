using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.SaintsSerialization;
using UnityEngine;

namespace SaintsField.Utils
{
    public static class SaintsSerializedUtil
    {
        public static void OnBeforeSerialize(List<SaintsSerializedProperty> saintsSerializedProperties, Type serContainerType)
        {
            List<int> toRemoveIndexes = new List<int>();
            for (int index = 0; index < saintsSerializedProperties.Count; index++)
            {
                SaintsSerializedProperty serializedProperty = saintsSerializedProperties[index];
                string propName = serializedProperty.name;
                if (serializedProperty.isProperty)
                {
                    PropertyInfo propertyInfo = serContainerType.GetProperty(propName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propertyInfo == null)
                    {
                        toRemoveIndexes.Add(index);
                    }
                }
                else
                {
                    FieldInfo fieldInfo = serContainerType.GetField(propName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo == null)
                    {
                        toRemoveIndexes.Add(index);
                    }
                }
            }

            toRemoveIndexes.Reverse();
            foreach (int removeIndex in toRemoveIndexes)
            {
                Debug.LogWarning($"Saints Serialied {saintsSerializedProperties[removeIndex].name}@{removeIndex} disappeared, removed from serialized list.");
                saintsSerializedProperties.RemoveAt(removeIndex);
            }
        }

        public static void OnAfterDeserialize(List<SaintsSerializedProperty> _saintsSerializedProperties, Type serContainerType, UnityEngine.Object serTarget)
        {
            foreach (SaintsSerializedProperty serializedProperty in _saintsSerializedProperties)
            {
                string propName = serializedProperty.name;
                if (serializedProperty.isProperty)
                {
                    PropertyInfo propertyInfo = serContainerType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propertyInfo == null)
                    {
                        Debug.LogWarning($"Saints Serialied {propName} not found, skip deseialization.");
                        continue;
                    }
                    propertyInfo.SetValue(serTarget, GetSaintsSerializedPropertyValue(serializedProperty, GetSaintsElementType(propertyInfo.PropertyType)));
                }
                else
                {
                    FieldInfo fieldInfo = serContainerType.GetField(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo == null)
                    {
                        Debug.LogWarning($"Saints Serialied {propName} not found, skip deseialization.");
                        continue;
                    }
                    Type elementType = GetSaintsElementType(fieldInfo.FieldType);
                    object realValue = GetSaintsSerializedPropertyValue(serializedProperty, elementType);
                    fieldInfo.SetValue(serTarget, realValue);
                }
            }
        }

        private static Type GetSaintsElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if(type.IsInterface)
            {
                if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
                {
                    return type.GetGenericArguments()[0];
                }
            }
            else
            {
                foreach (Type typeInterface in type.GetInterfaces())
                {
                    if (typeInterface.IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeInterface))
                    {
                        return typeInterface.GetGenericArguments()[0];
                    }
                }
            }

            // if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            // {
            //     return type.GetGenericArguments()[0];
            // }

            return type;
        }

        private static Array MakeArray(Type elementType, IEnumerable values, int length)
        {
            var arr = Array.CreateInstance(elementType, length);
            int index = 0;
            foreach (object each in values)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (elementType.IsEnum)
                {
                    arr.SetValue(Enum.ToObject(elementType, each), index);
                }
                else
                {
                    arr.SetValue(Convert.ChangeType(each, elementType), index);
                }

                index++;
            }

            return arr;
        }

        private static object GetSaintsSerializedPropertyValue(SaintsSerializedProperty serializedProperty, Type elementType)
        {
            switch (serializedProperty.propertyType)
            {
                case SaintsPropertyType.EnumLong:
                {
                    // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                    switch (serializedProperty.collectionType)
                    {
                        case CollectionType.Default:
                            return serializedProperty.longValue;
                        case CollectionType.Array:
                        {
                            return MakeArray(elementType, serializedProperty.longValues, serializedProperty.longValues.Length);
                        }

                        case CollectionType.List:
                            return serializedProperty.longValues.Length == 0
                                ? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))
                                : (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), MakeArray(elementType, serializedProperty.longValues, serializedProperty.longValues.Length));
                        default:
                            throw new ArgumentOutOfRangeException(nameof(serializedProperty.collectionType), serializedProperty.collectionType, null);
                    }

                }
                case SaintsPropertyType.EnumULong:
                {
                    // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                    switch (serializedProperty.collectionType)
                    {
                        case CollectionType.Default:
                            return serializedProperty.uLongValue;
                        case CollectionType.Array:
                        {
                            return MakeArray(elementType, serializedProperty.uLongValues, serializedProperty.uLongValues.Length);
                        }

                        case CollectionType.List:
                            return serializedProperty.uLongValues.Length == 0
                                ? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))
                                : (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), MakeArray(elementType, serializedProperty.uLongValues, serializedProperty.uLongValues.Length));
                        default:
                            throw new ArgumentOutOfRangeException(nameof(serializedProperty.collectionType), serializedProperty.collectionType, null);
                    }
                }
                case SaintsPropertyType.Other:
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializedProperty.propertyType), serializedProperty.propertyType, null);
            }
        }
    }
}
