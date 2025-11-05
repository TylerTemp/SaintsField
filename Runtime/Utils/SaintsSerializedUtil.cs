using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsSerialization;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    public static class SaintsSerializedUtil
    {
        // public static long OnBeforeSerializeDateTime(DateTime dt) => dt.Ticks;
        // public static long OnBeforeSerializeTimeSpan(TimeSpan dt) => dt.Ticks;

        public static (bool ok, SaintsSerializedProperty result) OnBeforeSerialize(SaintsSerializedProperty serializedProp, object obj, Type type)
        {
            if (type.IsEnum)
            {
                Type underType = Enum.GetUnderlyingType(type);
                if (underType == typeof(long))
                {
                    return (true, new SaintsSerializedProperty
                    {
                        propertyType = SaintsPropertyType.EnumLong,
                        longValue = Convert.ToInt64(obj),
                    });
                }

#if UNITY_2022_1_OR_NEWER
                if (underType == typeof(ulong))
                {
                    return (true, new SaintsSerializedProperty
                    {
                        propertyType = SaintsPropertyType.EnumULong,
                        uLongValue = Convert.ToUInt64(obj)
                    });
                }
#endif

                // throw new NotSupportedException(
                //     $"SaintsSerializedUtil OnBeforeSerialize not support enum underlying type {underType.FullName}");
                return (false, default);
            }

            // ReSharper disable once InvertIf
            if (type.IsInterface)
            {
                bool isVRef = false;
                if(serializedProp.propertyType == SaintsPropertyType.Interface)
                {
                    isVRef = serializedProp.IsVRef;
                }
                if (RuntimeUtil.IsNull(obj))
                {
                    return (true, new SaintsSerializedProperty
                    {
                        IsVRef = isVRef,
                        propertyType = SaintsPropertyType.Interface,
                    });
                }

                // ReSharper disable once InvertIf
                if (obj is UnityEngine.Object uObj)
                {
                    return (true, new SaintsSerializedProperty
                    {
                        propertyType = SaintsPropertyType.Interface,
                        IsVRef = false,
                        V = uObj,
                    });
                }

                return (true, new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.Interface,
                    VRef = obj,
                    IsVRef = true,
                });
            }

            if (obj is DateTime dt)
            {
                return (true, new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.DateTime,
                    longValue = dt.Ticks,
                });
            }
            if (obj is TimeSpan ts)
            {
                return (true, new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.TimeSpan,
                    longValue = ts.Ticks,
                });
            }

            if (obj is Guid guid)
            {
                return (true, new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.Guid,
                    stringValue = guid.ToString(),
                });
            }

            return (false, default);
        }

        public static (bool assign, SaintsDictionary<TKey, TValue> result) OnBeforeSerializeDictionary<TKey, TValue>(SaintsDictionary<TKey, TValue> serializedProp, object obj)
        {
            if (obj == null)
            {
                // Debug.Log("OnBeforeSerializeDictionary skip null");
                // return (true, new SaintsDictionary<TKey, TValue>());
                return (false, null);
            }

            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(obj is IDictionary<TKey, TValue> originDic))
            {
                // Debug.Log("OnBeforeSerializeDictionary not dictionary");
                return (false, null);
            }

            foreach (KeyValuePair<TKey, TValue> originKv in originDic)
            {
                if (serializedProp.TryGetValue(originKv.Key, out TValue value))
                {
                    if((object)value != (object)originKv.Value)
                    {
                        // Debug.Log($"Update serialized kv {value} -> {originKv.Value}");
                        serializedProp[originKv.Key] = originKv.Value;
                    }
                }
                else
                {
                    // Debug.Log("Update serialized kv");
                    serializedProp[originKv.Key] = originKv.Value;
                }
            }

            foreach (TKey removeKey in serializedProp.Keys.Except(originDic.Keys).ToArray())
            {
                serializedProp.Remove(removeKey);
            }

            // Debug.Log($"OnBeforeSerializeDictionary inplace modified {string.Join(", ", serializedProp.Select(each => $"{each.Key}:{each.Value}"))} to {string.Join(", ", serializedProp.Select(each => $"{each.Key}:{each.Value}"))}");

            return (true, serializedProp);
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

            List<SaintsSerializedProperty> serRef =
                new List<SaintsSerializedProperty>(toFill ?? Array.Empty<SaintsSerializedProperty>());
            if(serRef.Count < objList.Count)
            {
                for(int i = serRef.Count; i < objList.Count; i++)
                {
                    serRef.Add(new SaintsSerializedProperty());
                }
            }

            for (int i = 0; i < objList.Count; i++)
            {
                (bool ok, SaintsSerializedProperty result) = OnBeforeSerialize(serRef[i], objList[i], elementType);
                // ReSharper disable once InvertIf
                if(ok)
                {
                    if (inPlace)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            if (!inPlace)
            {
                toFill = results;
            }
        }

        public static void OnBeforeSerializeCollectionDictionary<TKey, TValue>(ref SaintsDictionary<TKey, TValue>[] toFill, Dictionary<TKey, TValue>[] objList)
        {
            Debug.Assert(objList != null);
            bool inPlace = toFill != null && toFill.Length == objList.Length;

            SaintsDictionary<TKey, TValue>[] results;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (inPlace)
            {
                results = Array.Empty<SaintsDictionary<TKey, TValue>>();
            }
            else
            {
                results = new SaintsDictionary<TKey, TValue>[objList.Length];
            }

            List<SaintsDictionary<TKey, TValue>> serRef =
                new List<SaintsDictionary<TKey, TValue>>(toFill ?? Array.Empty<SaintsDictionary<TKey, TValue>>());
            if(serRef.Count < objList.Length)
            {
                for(int i = serRef.Count; i < objList.Length; i++)
                {
                    serRef.Add(new SaintsDictionary<TKey, TValue>());
                }
            }

            for (int i = 0; i < objList.Length; i++)
            {
                (bool assign, SaintsDictionary<TKey, TValue> result) = OnBeforeSerializeDictionary(serRef[i], objList[i]);
                // ReSharper disable once InvertIf
                if(assign)
                {
                    if (inPlace)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            if (!inPlace)
            {
                toFill = results;
            }
        }

        public static void OnBeforeSerializeCollectionDictionary<TKey, TValue>(ref SaintsDictionary<TKey, TValue>[] toFill, List<Dictionary<TKey, TValue>> objList)
        {
            Debug.Assert(objList != null);
            bool inPlace = toFill != null && toFill.Length == objList.Count;

            SaintsDictionary<TKey, TValue>[] results;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (inPlace)
            {
                results = Array.Empty<SaintsDictionary<TKey, TValue>>();
            }
            else
            {
                results = new SaintsDictionary<TKey, TValue>[objList.Count];
            }

            List<SaintsDictionary<TKey, TValue>> serRef =
                new List<SaintsDictionary<TKey, TValue>>(toFill ?? Array.Empty<SaintsDictionary<TKey, TValue>>());
            if(serRef.Count < objList.Count)
            {
                for(int i = serRef.Count; i < objList.Count; i++)
                {
                    serRef.Add(new SaintsDictionary<TKey, TValue>());
                }
            }

            for (int i = 0; i < objList.Count; i++)
            {
                (bool assign, SaintsDictionary<TKey, TValue> result) = OnBeforeSerializeDictionary(serRef[i], objList[i]);
                // ReSharper disable once InvertIf
                if(assign)
                {
                    if (inPlace)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            if (!inPlace)
            {
                toFill = results;
            }
        }

        // public static void OnBeforeSerializeCollectionDateTime(ref long[] toFill, IReadOnlyList<DateTime> objList)
        // {
        //     Debug.Assert(objList != null);
        //     bool inPlace = toFill != null && toFill.Length == objList.Count;
        //
        //     long[] results;
        //     // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        //     if (inPlace)
        //     {
        //         results = Array.Empty<long>();
        //     }
        //     else
        //     {
        //         results = new long[objList.Count];
        //     }
        //
        //     for (int i = 0; i < objList.Count; i++)
        //     {
        //         if (inPlace)
        //         {
        //             toFill[i]  = OnBeforeSerializeDateTime(objList[i]);
        //         }
        //         else
        //         {
        //             results[i] = OnBeforeSerializeDateTime(objList[i]);
        //         }
        //     }
        //
        //     if (!inPlace)
        //     {
        //         toFill = results;
        //     }
        // }
        //
        // public static void OnBeforeSerializeCollectionTimeSpan(ref long[] toFill, IReadOnlyList<TimeSpan> objList)
        // {
        //     Debug.Assert(objList != null);
        //     bool inPlace = toFill != null && toFill.Length == objList.Count;
        //
        //     long[] results;
        //     // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        //     if (inPlace)
        //     {
        //         results = Array.Empty<long>();
        //     }
        //     else
        //     {
        //         results = new long[objList.Count];
        //     }
        //
        //     for (int i = 0; i < objList.Count; i++)
        //     {
        //         if (inPlace)
        //         {
        //             toFill[i]  = OnBeforeSerializeTimeSpan(objList[i]);
        //         }
        //         else
        //         {
        //             results[i] = OnBeforeSerializeTimeSpan(objList[i]);
        //         }
        //     }
        //
        //     if (!inPlace)
        //     {
        //         toFill = results;
        //     }
        // }

        // public static DateTime OnAfterDeserializeDateTime(long tick) => new DateTime(tick);
        // public static TimeSpan OnAfterDeserializeTimeSpan(long tick) => new TimeSpan(tick);

        public static (bool ok, T result) OnAfterDeserialize<T>(SaintsSerializedProperty saintsSerializedProperty, Type targetType)
        {
            switch (saintsSerializedProperty.propertyType)
            {
                case SaintsPropertyType.EnumLong:
                    if (targetType.IsEnum)
                    {
                        return (true, (T)Enum.ToObject(targetType, saintsSerializedProperty.longValue));
                    }
                    return (true, (T)Convert.ChangeType(saintsSerializedProperty.longValue, targetType));
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                    if (targetType.IsEnum)
                    {
                        return (true, (T)Enum.ToObject(targetType, saintsSerializedProperty.uLongValue));
                    }
                    return (true, (T)Convert.ChangeType(saintsSerializedProperty.uLongValue, targetType));
#endif
                case SaintsPropertyType.Interface:
                {
                    if (saintsSerializedProperty.IsVRef)
                    {
                        return (true, (T)saintsSerializedProperty.VRef);
                    }

                    if (RuntimeUtil.IsNull(saintsSerializedProperty.V))
                    {
                        return default;
                    }

                    try
                    {
                        return (true, (T)(object)saintsSerializedProperty.V);
                    }
                    catch (InvalidCastException)
                    {
                        return (false, default);
                    }
                }
                case SaintsPropertyType.DateTime:
                    return (true, (T)(object)new DateTime(saintsSerializedProperty.longValue));
                case SaintsPropertyType.TimeSpan:
                    return (true, (T)(object)new TimeSpan(saintsSerializedProperty.longValue));
                case SaintsPropertyType.Guid:
                {
                    string stringValue = saintsSerializedProperty.stringValue;
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return (true, (T)(object)Guid.Empty);
                    }

                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (Guid.TryParse(stringValue, out Guid guid))
                    {
                        return (true, (T)(object)guid);
                    }

                    return (false, default);
                }
                case SaintsPropertyType.Undefined:
                default:
                    return (false, targetType.IsValueType ? (T)Activator.CreateInstance(targetType) : default);

                    // throw new ArgumentOutOfRangeException(nameof(saintsSerializedProperty.propertyType), saintsSerializedProperty.propertyType, null);
            }
        }

        public static (bool assign, Dictionary<TKey, TValue> result) OnAfterDeserializeDictionary<TKey, TValue>(object originValue, SaintsDictionary<TKey, TValue> saintsSerializedProperty)
        {
            switch (originValue)
            {
                case null:
                {
                    // Debug.Log($"OnAfterDeserializeDictionary to new dictionary");
                    return (false, null);
                }
                case Dictionary<TKey, TValue> originDictionary:
                {
                    foreach (KeyValuePair<TKey, TValue> kv in saintsSerializedProperty)
                    {
                        if (originDictionary.TryGetValue(kv.Key, out TValue value))
                        {
                            if((object)value != (object)kv.Value)
                            {
                                originDictionary[kv.Key] = kv.Value;
                            }
                        }
                        else
                        {
                            originDictionary[kv.Key] = kv.Value;
                        }
                    }

                    foreach (TKey removeKey in originDictionary.Keys.Except(saintsSerializedProperty.Keys).ToArray())
                    {
                        originDictionary.Remove(removeKey);
                    }

                    // Debug.Log($"OnAfterDeserializeDictionary inplace {string.Join(", ", originDictionary.Select(each => $"{each.Key}:{each.Value}"))} with {string.Join(", ", saintsSerializedProperty.Select(each => $"{each.Key}:{each.Value}"))}");
                    return (true, originDictionary);
                }
                default:
                    // Debug.Log($"OnAfterDeserializeDictionary skip {originValue}");
                    return (false, null);
            }
        }


        public static (bool filled, T[] result) OnAfterDeserializeArray<T>(T[] toFill, SaintsSerializedProperty[] saintsSerializedProperties, Type elementType)
        {
            // Debug.Log($"OnAfterDeserializeArray toFill={toFill}, serArr={saintsSerializedProperties}, elementType={elementType}");

            bool canFill = toFill != null && toFill.Length == saintsSerializedProperties.Length;
            T[] results = new T[saintsSerializedProperties.Length];
            for (int i = 0; i < saintsSerializedProperties.Length; i++)
            {
                (bool ok, T result) = OnAfterDeserialize<T>(saintsSerializedProperties[i], elementType);
                if(ok)
                {
                    if (canFill)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            return (canFill, results);
        }

        public static (bool filled, Dictionary<TKey, TValue>[] result) OnAfterDeserializeDictionaryArray<TKey, TValue>(Dictionary<TKey, TValue>[] toFill, SaintsDictionary<TKey, TValue>[] saintsSerializedProperties)
        {
            bool canFill = toFill != null && toFill.Length == saintsSerializedProperties.Length;
            Dictionary<TKey, TValue>[] results = new Dictionary<TKey, TValue>[saintsSerializedProperties.Length];
            for (int i = 0; i < saintsSerializedProperties.Length; i++)
            {
                (bool assign, Dictionary<TKey, TValue> result) = OnAfterDeserializeDictionary(canFill? toFill[i]: results[i], saintsSerializedProperties[i]);
                if(assign)
                {
                    if (canFill)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            return (canFill, results);
        }

        public static (bool filled, List<Dictionary<TKey, TValue>> result) OnAfterDeserializeDictionaryList<TKey, TValue>(List<Dictionary<TKey, TValue>> toFill, SaintsDictionary<TKey, TValue>[] saintsSerializedProperties)
        {
            bool canFill = toFill != null && toFill.Count == saintsSerializedProperties.Length;
            List<Dictionary<TKey, TValue>> results = Enumerable.Range(0, saintsSerializedProperties.Length)
                .Select(_ => new Dictionary<TKey, TValue>())
                .ToList();

            for (int i = 0; i < saintsSerializedProperties.Length; i++)
            {
                (bool assign, Dictionary<TKey, TValue> result) = OnAfterDeserializeDictionary(canFill? toFill[i]: results[i], saintsSerializedProperties[i]);
                if(assign)
                {
                    if (canFill)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            return (canFill, results);
        }
        // public static (bool filled, DateTime[] result) OnAfterDeserializeArrayDateTime(DateTime[] toFill, long[] saintsSerializedProperties)
        // {
        //     // Debug.Log($"OnAfterDeserializeArray toFill={toFill}, serArr={saintsSerializedProperties}, elementType={elementType}");
        //
        //     bool canFill = toFill != null && toFill.Length == saintsSerializedProperties.Length;
        //     DateTime[] results = new DateTime[saintsSerializedProperties.Length];
        //     for (int i = 0; i < saintsSerializedProperties.Length; i++)
        //     {
        //         if (canFill)
        //         {
        //             toFill[i] = OnAfterDeserializeDateTime(saintsSerializedProperties[i]);
        //         }
        //         else
        //         {
        //             results[i] = OnAfterDeserializeDateTime(saintsSerializedProperties[i]);
        //         }
        //     }
        //
        //     return (canFill, results);
        // }
        // public static (bool filled, TimeSpan[] result) OnAfterDeserializeArrayTimeSpan(TimeSpan[] toFill, long[] saintsSerializedProperties)
        // {
        //     // Debug.Log($"OnAfterDeserializeArray toFill={toFill}, serArr={saintsSerializedProperties}, elementType={elementType}");
        //
        //     bool canFill = toFill != null && toFill.Length == saintsSerializedProperties.Length;
        //     TimeSpan[] results = new TimeSpan[saintsSerializedProperties.Length];
        //     for (int i = 0; i < saintsSerializedProperties.Length; i++)
        //     {
        //         if (canFill)
        //         {
        //             toFill[i] = OnAfterDeserializeTimeSpan(saintsSerializedProperties[i]);
        //         }
        //         else
        //         {
        //             results[i] = OnAfterDeserializeTimeSpan(saintsSerializedProperties[i]);
        //         }
        //     }
        //
        //     return (canFill, results);
        // }

        public static (bool filled, List<T> result) OnAfterDeserializeList<T>(List<T> toFill, SaintsSerializedProperty[] saintsSerializedProperties, Type targetType)
        {
            // Debug.Log($"toFill={toFill}, serArr={saintsSerializedProperties}, targetType={targetType}");
            bool canFill = toFill != null && toFill.Count == saintsSerializedProperties.Length;
            // Debug.Log($"canFill={canFill}");
            List<T> results = new List<T>(new T[saintsSerializedProperties.Length]);
            for (int i = 0; i < saintsSerializedProperties.Length; i++)
            {
                (bool ok, T result) = OnAfterDeserialize<T>(saintsSerializedProperties[i], targetType);
                // ReSharper disable once InvertIf
                if(ok)
                {
                    if (canFill)
                    {
                        toFill[i] = result;
                    }
                    else
                    {
                        results[i] = result;
                    }
                }
            }

            // if (!canFill)
            // {
            //     toFill = results;
            // }
            return (canFill, results);
        }

        // public static (bool filled, List<DateTime> result) OnAfterDeserializeListDateTime(List<DateTime> toFill, long[] saintsSerializedProperties)
        // {
        //     // Debug.Log($"toFill={toFill}, serArr={saintsSerializedProperties}, targetType={targetType}");
        //     bool canFill = toFill != null && toFill.Count == saintsSerializedProperties.Length;
        //     // Debug.Log($"canFill={canFill}");
        //     List<DateTime> results = new List<DateTime>(new DateTime[saintsSerializedProperties.Length]);
        //     for (int i = 0; i < saintsSerializedProperties.Length; i++)
        //     {
        //         if (canFill)
        //         {
        //             toFill[i] = OnAfterDeserializeDateTime(saintsSerializedProperties[i]);
        //         }
        //         else
        //         {
        //             results[i] = OnAfterDeserializeDateTime(saintsSerializedProperties[i]);
        //         }
        //     }
        //
        //     // if (!canFill)
        //     // {
        //     //     toFill = results;
        //     // }
        //     return (canFill, results);
        // }
        // public static (bool filled, List<TimeSpan> result) OnAfterDeserializeListTimeSpan(List<TimeSpan> toFill, long[] saintsSerializedProperties)
        // {
        //     bool canFill = toFill != null && toFill.Count == saintsSerializedProperties.Length;
        //     List<TimeSpan> results = new List<TimeSpan>(new TimeSpan[saintsSerializedProperties.Length]);
        //     for (int i = 0; i < saintsSerializedProperties.Length; i++)
        //     {
        //         if (canFill)
        //         {
        //             toFill[i] = OnAfterDeserializeTimeSpan(saintsSerializedProperties[i]);
        //         }
        //         else
        //         {
        //             results[i] = OnAfterDeserializeTimeSpan(saintsSerializedProperties[i]);
        //         }
        //     }
        //
        //     return (canFill, results);
        // }

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
