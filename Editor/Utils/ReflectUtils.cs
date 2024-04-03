using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ReflectUtils
    {
        // public static FieldInfo GetField(object target, string fieldName)
        // {
        //     return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        // }

        // public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        // {
        //     if (target == null)
        //     {
        //         Debug.LogError("The target object is null. Check for missing scripts.");
        //         yield break;
        //     }
        //
        //     List<Type> types = GetSelfAndBaseTypes(target);
        //
        //     for (int i = types.Count - 1; i >= 0; i--)
        //     {
        //         IEnumerable<FieldInfo> fieldInfos = types[i]
        //             .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
        //             .Where(predicate);
        //
        //         foreach (FieldInfo fieldInfo in fieldInfos)
        //         {
        //             yield return fieldInfo;
        //         }
        //     }
        // }

        public static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new List<Type>
            {
                target.GetType(),
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            types.Reverse();

            return types;
        }

        // public static object GetTargetObjectWithProperty(SerializedProperty property)
        // {
        //     string path = property.propertyPath.Replace(".Array.data[", "[");
        //     object obj = property.serializedObject.targetObject;
        //     string[] elements = path.Split('.');
        //
        //     for (int i = 0; i < elements.Length - 1; i++)
        //     {
        //         string element = elements[i];
        //         if (element.Contains("["))
        //         {
        //             string elementName = element.Substring(0, element.IndexOf("["));
        //             int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
        //             obj = GetValue_Imp(obj, elementName, index);
        //         }
        //         else
        //         {
        //             obj = GetValue_Imp(obj, element);
        //         }
        //     }
        //
        //     return obj;
        // }

        // private static object GetValue_Imp(object source, string name)
        // {
        //     if (source == null)
        //     {
        //         return null;
        //     }
        //
        //     Type type = source.GetType();
        //
        //     while (type != null)
        //     {
        //         FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        //         if (field != null)
        //         {
        //             return field.GetValue(source);
        //         }
        //
        //         PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        //         if (property != null)
        //         {
        //             return property.GetValue(source, null);
        //         }
        //
        //         type = type.BaseType;
        //     }
        //
        //     return null;
        // }

        // private static object GetValue_Imp(object source, string name, int index)
        // {
        //     if (!(GetValue_Imp(source, name) is IEnumerable enumerable))
        //     {
        //         return null;
        //     }
        //
        //     IEnumerator enumerator = enumerable.GetEnumerator();
        //     for (int i = 0; i <= index; i++)
        //     {
        //         if (!enumerator.MoveNext())
        //         {
        //             return null;
        //         }
        //     }
        //
        //     return enumerator.Current;
        // }

        public enum GetPropType
        {
            NotFound,
            Property,
            Field,
            Method,
        }

        public static (GetPropType getPropType, object fieldOrMethodInfo) GetProp(Type targetType, string fieldName)
        {
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            FieldInfo fieldInfo = targetType.GetField(fieldName, bindAttr);
            if (fieldInfo == null)
            {
                fieldInfo = targetType.GetField($"<{fieldName}>k__BackingField", bindAttr);
            }
            if (fieldInfo != null)
            {
                return (GetPropType.Field, fieldInfo);
            }

            PropertyInfo propertyInfo = targetType.GetProperty(fieldName, bindAttr);
            if (propertyInfo != null)
            {
                return (GetPropType.Property, propertyInfo);
            }

            MethodInfo methodInfo = targetType.GetMethod(fieldName, bindAttr);
            // Debug.Log($"methodInfo={methodInfo}, fieldName={fieldName}, targetType={targetType}/FlattenHierarchy={bindAttr.HasFlag(BindingFlags.FlattenHierarchy)}");
            return methodInfo == null ? (GetPropType.NotFound, null) : (GetPropType.Method, methodInfo);

        }

        public static bool Truly(object value)
        {
            if (value is string stringValue)
            {
                return stringValue != "";
            }

            try
            {
                // Debug.Log($"try convert to bool");
                return Convert.ToBoolean(value);
            }
            catch (InvalidCastException)
            {
                bool equalNull = value == null;
                if (equalNull)
                {
                    // Debug.Log($"InvalidCastException, but value is null.");
                    return false;
                }
                try
                {
                    // Debug.Log($"try to cast to UnityEngine.Object");
                    return (UnityEngine.Object)value != null;
                }
                catch (InvalidCastException)
                {
                    // Debug.Log($"failed to cast to UnityEngine.Object");
                    return true;
                }
            }
            catch (NullReferenceException)
            {
                // Debug.Log($"Null, return false");
                return false;
            }
        }

        public static object[] MethodParamsFill(IReadOnlyList<ParameterInfo> methodParams, IEnumerable<object> toFillValues)
        {
            // first we just sign default value and null value
            object[] filledValues = methodParams.Select(param => param.IsOptional ? param.DefaultValue : null).ToArray();
            // then we check for each params:
            // 1.  If there are required params, fill the value
            // 2.  Then, if there are left value to fill and can match the optional type, then fill it
            // 3.  Ensure all required params are filled
            // 4.  Return.

            Queue<object> toFillQueue = new Queue<object>(toFillValues);
            Queue<object> leftOverQueue = new Queue<object>();
            // required:
            foreach (int index in Enumerable.Range(0, methodParams.Count))
            {
                if (!methodParams[index].IsOptional)
                {
                    // Debug.Log($"checking {index}={methodParams[index].Name}");
                    Debug.Assert(toFillQueue.Count > 0, $"Nothing to fill required parameter {methodParams[index].Name}");
                    while(toFillQueue.Count > 0)
                    {
                        object value = toFillQueue.Dequeue();
                        Type valueType = value.GetType();
                        Type paramType = methodParams[index].ParameterType;
                        if (valueType == paramType || valueType.IsSubclassOf(paramType))
                        {
                            // Debug.Log($"Push value {value} for {methodParams[index].Name}");
                            filledValues[index] = value;
                            break;
                        }

                        // Debug.Log($"Skip value {value} for {methodParams[index].Name}");
                        leftOverQueue.Enqueue(value);
                        // Debug.Assert(valueType == paramType || valueType.IsSubclassOf(paramType),
                        //     $"The value type `{valueType}` is not match the param type `{paramType}`");
                        // Debug.Log($"Add {value} at {index}");

                    }
                }
            }

            foreach (object leftOver in toFillQueue)
            {
                leftOverQueue.Enqueue(leftOver);
            }

            // optional:
            if(leftOverQueue.Count > 0)
            {
                foreach (int index in Enumerable.Range(0, methodParams.Count))
                {
                    if (leftOverQueue.Count == 0)
                    {
                        break;
                    }

                    if (methodParams[index].IsOptional)
                    {
                        object value = leftOverQueue.Peek();
                        Type valueType = value.GetType();
                        Type paramType = methodParams[index].ParameterType;
                        if(valueType == paramType || valueType.IsSubclassOf(paramType))
                        {
                            leftOverQueue.Dequeue();
                            filledValues[index] = value;
                        }
                    }
                }
            }

            return filledValues;
        }

        public static void SetValue(string propertyPath, FieldInfo info, object parent, object value)
        {
            int index = SerializedUtils.PropertyPathIndex(propertyPath);
            if (index == -1)
            {
                // Debug.Log($"direct set value {value} to {info} on {parent}");
                info.SetValue(parent, value);
            }
            else
            {
                object fieldValue = info.GetValue(parent);
                // Debug.Log($"try set value {value} at {index} to {info} on {parent}");
                if (fieldValue is Array array)
                {
                    array.SetValue(value, index);
                }
                else if(fieldValue is IList<object> list)
                {
                    list[index] = value;
                }
                else
                {
                    // Debug.Log($"direct set value {value} to {info} on {parent}");
                    info.SetValue(parent, value);
                }
            }

        }
    }
}
