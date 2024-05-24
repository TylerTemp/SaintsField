using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ReflectUtils
    {
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
            // Debug.Log($"init get fieldInfo {fieldInfo}");
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

        private class MethodParamFiller
        {
            public string name;
            public bool isOptional;
            public object defaultValue;

            public bool signed;
            public object value;
        }

        public static object[] MethodParamsFill(IReadOnlyList<ParameterInfo> methodParams, IEnumerable<object> toFillValues)
        {
            // first we just sign default value and null value
            MethodParamFiller[] filledValues = methodParams
                .Select(param => param.IsOptional
                    ? new MethodParamFiller
                    {
                        name = param.Name,
                        isOptional = true,
                        defaultValue = param.DefaultValue,
                    }
                    : new MethodParamFiller
                    {
                        name = param.Name,
                    })
                .ToArray();
            // then we check for each params:
            // 1.  If there are required params, fill the value
            // 2.  Then, if there are left value to fill and can match the optional type, then fill it
            // 3.  Ensure all required params are filled
            // 4.  Return.

            Queue<object> toFillQueue = new Queue<object>(toFillValues);
            Queue<object> leftOverQueue = new Queue<object>();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
            Debug.Log($"toFillQueue.Count={toFillQueue.Count}");
#endif
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
                        Type paramType = methodParams[index].ParameterType;
                        if (paramType.IsInstanceOfType(value))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
                            Debug.Log($"Push value {value} for {methodParams[index].Name}");
#endif
                            filledValues[index].value = value;
                            filledValues[index].signed = true;
                            break;
                        }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
                        Debug.Log($"No push value {value}({value.GetType()}) for {methodParams[index].Name}({paramType})");
#endif

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
                        Type paramType = methodParams[index].ParameterType;
                        if(paramType.IsInstanceOfType(value))
                        {
                            leftOverQueue.Dequeue();
                            filledValues[index].value = value;
                            filledValues[index].signed = false;
                        }
                    }
                }
            }

            return filledValues.Select(each =>
            {
                if (each.signed)
                {
                    return each.value;
                }
                Debug.Assert(each.isOptional, $"No value for required parameter `{each.name}` in method.");
                return each.defaultValue;
            }).ToArray();
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
