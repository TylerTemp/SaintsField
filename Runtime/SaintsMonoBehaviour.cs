using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Playa;
using SaintsField.SaintsSerialization;
using UnityEngine;

namespace SaintsField
{
    public class SaintsMonoBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {

        [PlayaShowIf(
#if SAINTSFIELD_SERIALIZATION_DEBUG
            true
#else
            false
#endif
        )]
        // [ListDrawerSettings(searchable: true)]
        [Table]
        [SerializeField] private List<SaintsSerializedProperty> _saintsSerializedProperties = new List<SaintsSerializedProperty>();

        public void OnBeforeSerialize()
        {
            List<int> toRemoveIndexes = new List<int>();
            for (int index = 0; index < _saintsSerializedProperties.Count; index++)
            {
                SaintsSerializedProperty serializedProperty = _saintsSerializedProperties[index];
                string propName = serializedProperty.name;
                if (serializedProperty.isProperty)
                {
                    PropertyInfo propertyInfo = GetType().GetProperty(propName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propertyInfo == null)
                    {
                        toRemoveIndexes.Add(index);
                    }
                }
                else
                {
                    FieldInfo fieldInfo = GetType().GetField(propName,
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
                Debug.Log($"remove @{removeIndex}=>{_saintsSerializedProperties[removeIndex].name}");
                _saintsSerializedProperties.RemoveAt(removeIndex);
            }
        }

        public void OnAfterDeserialize()
        {
            foreach (SaintsSerializedProperty serializedProperty in _saintsSerializedProperties)
            {
                string propName = serializedProperty.name;
                if (serializedProperty.isProperty)
                {
                    PropertyInfo propertyInfo = GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propertyInfo == null)
                    {
                        continue;
                    }
                    propertyInfo.SetValue(this, GetSaintsSerializedPropertyValue(serializedProperty, GetSaintsElementType(propertyInfo.PropertyType)));
                }
                else
                {
                    FieldInfo fieldInfo = GetType().GetField(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo == null)
                    {
                        continue;
                    }
                    Type elementType = GetSaintsElementType(fieldInfo.FieldType);
                    object realValue = GetSaintsSerializedPropertyValue(serializedProperty, elementType);
                    fieldInfo.SetValue(this, realValue);
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
