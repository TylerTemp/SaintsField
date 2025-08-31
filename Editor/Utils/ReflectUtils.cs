﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ReflectUtils
    {
        public static List<Type> GetSelfAndBaseTypes(object target)
        {
            return GetSelfAndBaseTypesFromType(target.GetType());
        }

        public static List<Type> GetSelfAndBaseTypesFromType(Type thisType)
        {
            List<Type> types = new List<Type>(1)
            {
                thisType,
            };

            // ReSharper disable once UseIndexFromEndExpression
            while (types[types.Count - 1].BaseType != null)
            {
                // ReSharper disable once UseIndexFromEndExpression
                types.Add(types[types.Count - 1].BaseType);
            }

            // types.Reverse();

            return types;
        }

        public enum GetPropType
        {
            NotFound,
            Property,
            Field,
            Method,
        }

        public const BindingFlags FindTargetBindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                      BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

        public static (GetPropType getPropType, object fieldOrMethodInfo) GetProp(Type targetType, string fieldName)
        {
            const BindingFlags bindAttr = FindTargetBindAttr;

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
            // Debug.Log($"methodInfo={methodInfo}, fieldName={fieldName}, targetType={targetType}/FlattenHierarchy={bindAttr.HasFlagFast(BindingFlags.FlattenHierarchy)}");
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
            public string Name;
            public bool IsOptional;
            public object DefaultValue;

            public bool Signed;
            public object Value;
        }

        public static object[] MethodParamsFill(IReadOnlyList<ParameterInfo> methodParams, IEnumerable<object> toFillValues)
        {
            // first we just sign default value and null value
            MethodParamFiller[] filledValues = methodParams
                .Select(param => param.IsOptional
                    ? new MethodParamFiller
                    {
                        Name = param.Name,
                        IsOptional = true,
                        DefaultValue = param.DefaultValue,
                    }
                    : new MethodParamFiller
                    {
                        Name = param.Name,
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
            for (var index = 0; index < methodParams.Count; index++)
            {
                if (!methodParams[index].IsOptional)
                {
                    // Debug.Log($"checking {index}={methodParams[index].Name}");
                    Debug.Assert(toFillQueue.Count > 0, $"Nothing to fill required parameter {methodParams[index].Name}");
                    while(toFillQueue.Count > 0)
                    {
                        object value = toFillQueue.Dequeue();
                        Type paramType = methodParams[index].ParameterType;
                        // Debug.Log($"{value} -> {paramType}");
                        if (value == null || paramType.IsInstanceOfType(value) || CheckSignEnum(value, paramType))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
                            Debug.Log($"Push value {value} for {methodParams[index].Name}");
#endif
                            filledValues[index].Value = value;
                            filledValues[index].Signed = true;
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
                Debug.Log($"leftOver: {leftOver}");
#endif
                leftOverQueue.Enqueue(leftOver);
            }

            // optional:
            if(leftOverQueue.Count > 0)
            {
                for (var index = 0; index < methodParams.Count; index++)
                {
                    if (leftOverQueue.Count == 0)
                    {
                        break;
                    }

                    if (methodParams[index].IsOptional)
                    {
                        object value = leftOverQueue.Peek();
                        Type paramType = methodParams[index].ParameterType;
                        if(value == null || paramType.IsInstanceOfType(value))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
                            Debug.Log($"add optional: {value} -> {methodParams[index].Name}({paramType})");
#endif
                            leftOverQueue.Dequeue();
                            filledValues[index].Value = value;
                            filledValues[index].Signed = true;
                        }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CALLBACK
                        else
                        {
                            Debug.Log($"not fit optional: {value}({value.GetType()}) -> {paramType}");
                        }
#endif
                    }
                }
            }

            return filledValues.Select(each =>
            {
                if (each.Signed)
                {
                    return each.Value;
                }
                Debug.Assert(each.IsOptional, $"No value for required parameter `{each.Name}` in method.");
                return each.DefaultValue;
            }).ToArray();
        }

        private static bool CheckSignEnum(object value, Type paramType)
        {
            return value is int && paramType.IsSubclassOf(typeof(Enum));
        }

        private static void SetIWrapPropValue(IWrapProp wrapProp, object value)
        {
            SerializedUtils.FieldOrProp fieldOrProp = Util.GetWrapProp(wrapProp);
            if (fieldOrProp.IsField)
            {
                fieldOrProp.FieldInfo.SetValue(wrapProp, value);
            }
            else
            {
                SetProperyValue(fieldOrProp.PropertyInfo, wrapProp, value);
            }
        }

        private static void SetProperyValue(PropertyInfo prop, object instance, object value)
        {
            if(prop.CanWrite)
            {
                prop.SetValue(instance, value);
            }
            else
            {
                MethodInfo setter = prop.GetSetMethod(true);
                if (setter != null)
                {
                    setter.Invoke(instance, new [] { value });
                }

            }
        }

        public static void SetValue(string propertyPath, UnityEngine.Object targetObject, MemberInfo info, object parent, object value)
        {
            Undo.RecordObject(targetObject, "SetValue");
            int index = SerializedUtils.PropertyPathIndex(propertyPath);
            if (index == -1)
            {
                object infoValue;
                if (info.MemberType == MemberTypes.Field)
                {
                    infoValue = ((FieldInfo)info).GetValue(parent);
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    infoValue = ((PropertyInfo)info).GetValue(parent);
                }
                else
                {
                    return;
                }

                if (infoValue is IWrapProp wrapProp)
                {
                    SetIWrapPropValue(wrapProp, value);
                }
                else
                {
                    if (info.MemberType == MemberTypes.Field)
                    {
                        ((FieldInfo)info).SetValue(parent, value);
                    }
                    else if (info.MemberType == MemberTypes.Property)
                    {
                        ((PropertyInfo)info).SetValue(parent, value);
                    }
                }
            }
            else
            {
                object fieldValue;
                if (info.MemberType == MemberTypes.Field)
                {
                    fieldValue = ((FieldInfo)info).GetValue(parent);
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    fieldValue = ((PropertyInfo)info).GetValue(parent);
                }
                else
                {
                    return;
                }

                // Debug.Log($"try set value {value} at {index} to {info} on {parent}");
                if (fieldValue is Array array)
                {
                    if (array.GetValue(index) is IWrapProp wrapProp)
                    {
                        SetIWrapPropValue(wrapProp, value);
                    }
                    else
                    {
                        array.SetValue(value, index);
                    }
                }
                else if(fieldValue is IList list)
                {
                    if (list[index] is IWrapProp wrapProp)
                    {
                        SetIWrapPropValue(wrapProp, value);
                    }
                    else
                    {
                        list[index] = value;
                    }
                }
                else
                {
                    // Debug.Log($"direct set value {value} to {info} on {parent}");
                    // info.SetValue(parent, value);
                    if (info.MemberType == MemberTypes.Field)
                    {
                        ((FieldInfo)info).SetValue(parent, value);
                    }
                    else if (info.MemberType == MemberTypes.Property)
                    {
                        ((PropertyInfo)info).SetValue(parent, value);
                    }
                }
            }

        }

        public static Type GetElementType(Type type)
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

        public static Type GetMostBaseType(Type type)
        {
            Type lastType = type;
            while (true)
            {
                Type baseType = lastType.BaseType;
                if (baseType == null)
                {
                    return lastType;
                }

                if (!baseType.IsGenericType)
                {
                    return lastType;
                }

                lastType = baseType;
            }
        }

        public static string GetIWrapPropName(Type type, string staticNameHolder = "EditorPropertyName")
        {
            Type lastType = type;

            while (true)
            {
                string name = GetStaticFieldStringValueFromType(lastType, staticNameHolder);
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }

                lastType = lastType.BaseType;
                if (lastType == null)
                {
                    return null;
                }
            }
        }

        // public static string GetIWrapPropName(Type type) => GetFieldStringValueFromType(type, "EditorPropertyName");

        public static string GetStaticFieldStringValueFromType(Type type, string fieldName)
        {
            PropertyInfo r = type.GetProperty(fieldName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (r != null)
            {
                return (string)r.GetValue(null);
            }
            FieldInfo f = type.GetField(fieldName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (f != null)
            {
                return (string)f.GetValue(null);
            }

            return null;
        }

        public static Type GetIWrapPropType(Type wrapPropType, string prop)
        {
            PropertyInfo wrapPropertyInfo = wrapPropType.GetProperty(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (wrapPropertyInfo != null)
            {
                return wrapPropertyInfo.PropertyType;
            }
            FieldInfo wrapFieldInfo = wrapPropType.GetField(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            Debug.Assert(wrapFieldInfo != null);
            return wrapFieldInfo.FieldType;
        }

        public static Type GetIWrapPropType(Type wrapPropType)
        {
            string prop = GetIWrapPropName(wrapPropType);
            // Debug.Log($"prop:{prop}");
            if (string.IsNullOrEmpty(prop))
            {
                return null;
            }

            return GetIWrapPropType(wrapPropType, prop);
        }

        public static Type GetDictionaryType(Type type)
        {
            // IDictionary
            return type
                .GetInterfaces()
                .FirstOrDefault(interfaceType =>
                    interfaceType.IsGenericType
                    && (
                        interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                        || interfaceType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                    )
                );
        }

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
            while (toCheck != null && toCheck != typeof(object)) {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static (bool found, string value) GetRichLabelFromEnum(Type enumType, object enumValue)
        {
            string enumFieldName = Enum.GetName(enumType, enumValue);
            FieldInfo fieldInfo = enumType.GetField(enumFieldName);
            PropertyAttribute[] attributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(fieldInfo);

            foreach (PropertyAttribute attribute in attributes)
            {
                switch (attribute)
                {
                    case RichLabelAttribute r:
                        return (true, r.RichTextXml);
                    case InspectorNameAttribute i:
                        return (true, i.displayName);
                }
            }

            return (false, enumFieldName);
        }
    }
}
