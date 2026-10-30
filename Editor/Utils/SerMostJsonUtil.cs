using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if SAINTSFIELD_NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
#if SAINTSFIELD_NEWTONSOFT_JSON
    [Serializable]
    public enum SerFlavor
    {
        NotSupported,  // nested/conjunction array/list, dictionary etc
        Null,
        UnityStandard,
        UnityObject,
        NonUnityObject,
        // AssetObject,
        // PrefabComponent,
        // SceneObject,
        // SceneComponent,
    }

    [Serializable]
    public enum CollectType
    {
        Value,
        List,
        Array,
    }

    // [Serializable]
    // public struct TypeInfo
    // {
    //     public string typeNameAndAssembly;
    //     public string monoScriptGuid;
    // }

    [Serializable]
    public struct TypedValue
    {
        public string fieldType;
        public string targetType;
        public CollectType collectType;

        public SerFlavor serFlavor;
        public string valueJson;

        public TypedValue(Type fieldType, Type targetType, CollectType collectType, SerFlavor serFlavor, string valueJson)
        {
            this.fieldType = TypeReference.GetTypeNameAndAssembly(fieldType);
            this.targetType = TypeReference.GetTypeNameAndAssembly(targetType);
            this.collectType = collectType;
            this.serFlavor = serFlavor;
            this.valueJson = valueJson;
        }
    }

    public static class SerMostJsonUtil
    {
        // Note: many collection types like Dictionary, HashSet is ignored
        private static TypedValue ToTypedValue(Type fieldType, object obj, HashSet<object> visitedObjects)
        {
            (bool notSupported, CollectType collectType, Type itemType) = IsArrayOrList(fieldType);
            if (notSupported)
            {
                return new TypedValue(
                    fieldType,
                    obj?.GetType() ?? fieldType,
                    CollectType.Value,
                    SerFlavor.NotSupported,
                    ""
                );
            }

            if (RuntimeUtil.IsNull(obj))
            {
                return new TypedValue(
                    fieldType,
                    itemType,
                    collectType,
                    SerFlavor.Null,
                    "null"
                );
            }

            // Leaf values do not recurse and can safely appear more than once.
            if (collectType != CollectType.Value
                || (
                    obj is not UnityEngine.Object
                    && !RuntimeUtil.IsUnityCoreSerializable(obj.GetType())
                    && !obj.GetType().IsEnum
                    )
                )
            {
                if (!visitedObjects.Add(obj))
                {
                    return new TypedValue(
                        fieldType,
                        obj.GetType(),
                        collectType,
                        SerFlavor.NotSupported,
                        ""
                    );
                }
            }

            if (collectType != CollectType.Value)  // list or array
            {
                List<TypedValue> values = new List<TypedValue>();
                HashSet<object> collectionVisitedObjects = new HashSet<object>(visitedObjects);
                foreach (object element in (IEnumerable)obj)
                {
                    // Siblings may share objects, but must still detect references to ancestors.
                    HashSet<object> elementVisitedObjects = new HashSet<object>(visitedObjects);
                    TypedValue itemValue = ToTypedValue(itemType, element, elementVisitedObjects);
                    if (itemValue.serFlavor == SerFlavor.NotSupported)
                    {
                        return new TypedValue(
                            fieldType,
                            itemType,
                            collectType,
                            SerFlavor.NotSupported,
                            ""
                        );
                    }
                    values.Add(itemValue);
                    collectionVisitedObjects.UnionWith(elementVisitedObjects);
                }
                visitedObjects.UnionWith(collectionVisitedObjects);

                return new TypedValue(
                    fieldType,
                    itemType,
                    collectType,
                    SerFlavor.NonUnityObject,
                    JsonConvert.SerializeObject(values)
                );
            }

            if (obj is UnityEngine.Object uObj)
            {
                try
                {
                    return new TypedValue(
                        fieldType,
                        uObj.GetType(),
                        collectType,
                        SerFlavor.UnityObject,
                        GlobalObjectId.GetGlobalObjectIdSlow(uObj).ToString()
                    );
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    return new TypedValue(
                        fieldType,
                        uObj.GetType(),
                        collectType,
                        SerFlavor.NotSupported,
                        ""
                    );
                }
            }

            if (RuntimeUtil.IsUnityCoreSerializable(obj.GetType()) || obj.GetType().IsEnum)
            {
                Type targetType = obj.GetType();
                bool isUnityStandard = !targetType.IsPrimitive && targetType != typeof(string) && !targetType.IsEnum;
                try
                {
                    return new TypedValue(
                        fieldType,
                        targetType,
                        collectType,
                        isUnityStandard ? SerFlavor.UnityStandard : SerFlavor.NonUnityObject,
                        isUnityStandard ? JsonUtility.ToJson(obj) : JsonConvert.SerializeObject(obj)
                    );
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    return new TypedValue(
                        fieldType,
                        obj.GetType(),
                        collectType,
                        SerFlavor.NotSupported,
                        ""
                    );
                }
            }

            // normal object
            Dictionary<string, TypedValue> nameToValue = new Dictionary<string, TypedValue>();
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            List<FieldInfo> fieldTargets = obj.GetType().GetFields(bindAttrNormal).ToList();
            PropertyInfo[] propertyTargets = obj.GetType().GetProperties(bindAttrNormal);

            foreach (FieldInfo fieldInfo in fieldTargets)
            {
                if (AbsRenderer.SkipTypeDrawing(fieldInfo.FieldType))
                {
                    continue;
                }
                object fieldValue = null;
                try
                {
                    fieldValue = fieldInfo.GetValue(obj);
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning($"field {fieldInfo.FieldType} inside {obj} gives error: {e}");
#endif
                    continue;
                }

                TypedValue typedValue = ToTypedValue(fieldInfo.FieldType, fieldValue, visitedObjects);
                if(typedValue.serFlavor != SerFlavor.NotSupported)
                {
                    nameToValue[fieldInfo.Name] = typedValue;
                }
            }

            foreach (PropertyInfo propertyInfo in propertyTargets)
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                if (propertyInfo.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                if (AbsRenderer.SkipTypeDrawing(propertyInfo.PropertyType))
                {
                    continue;
                }

                object propertyValue;
                try
                {
                    propertyValue = propertyInfo.GetValue(obj);
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning($"property {propertyInfo.PropertyType} inside {obj} gives error: {e}");
                    Debug.LogWarning(e.InnerException ?? e);
#endif
                    continue;
                }

                TypedValue typedValue = ToTypedValue(propertyInfo.PropertyType, propertyValue, visitedObjects);
                if(typedValue.serFlavor != SerFlavor.NotSupported)
                {
                    nameToValue[propertyInfo.Name] = typedValue;
                }
            }

            return new TypedValue(
                fieldType,
                obj.GetType(),
                collectType,
                SerFlavor.NonUnityObject,
                JsonConvert.SerializeObject(nameToValue)
            );
        }

        public static string ToTypedJson(Type fieldType, object obj)
        {
            TypedValue result = ToTypedValue(fieldType, obj, new HashSet<object>());

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (result.serFlavor == SerFlavor.NotSupported)
            {
                return "";
            }

            return JsonConvert.SerializeObject(result);
        }

        public static (bool ok, object result) FromTypedJson(Type fieldType, string typedJson)
        {
            return FromTypedValue(fieldType, JsonConvert.DeserializeObject<TypedValue>(typedJson));
        }

        private static (bool ok, object result) FromTypedValue(Type fieldType, TypedValue value)
        {
            if (fieldType == null || value.serFlavor == SerFlavor.NotSupported ||
                !Enum.IsDefined(typeof(SerFlavor), value.serFlavor) ||
                !Enum.IsDefined(typeof(CollectType), value.collectType))
            {
                const string error = "Invalid typed value or destination type";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }
            if (value.serFlavor == SerFlavor.Null)
            {
                if (!fieldType.IsValueType || Nullable.GetUnderlyingType(fieldType) != null)
                {
                    return (true, null);
                }
                string error = $"Cannot assign null to {fieldType}";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }

            Type targetType;
            try
            {
                targetType = Type.GetType(value.targetType, true);
            }
            catch (Exception e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
                return (false, e.Message);
            }
            object result;
            if (value.collectType != CollectType.Value)
            {
                (bool ok, object collection) = RestoreCollection(fieldType, targetType, value);
                if (!ok)
                {
                    return (false, collection);
                }
                result = collection;
            }
            else
            {
                if (!fieldType.IsAssignableFrom(targetType) && Nullable.GetUnderlyingType(fieldType) != targetType)
                {
                    string error = $"Cannot assign {targetType} to {fieldType}";
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(error);
#endif
                    return (false, error);
                }
                switch (value.serFlavor)
                {
                    case SerFlavor.UnityObject:
                        if (!typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                        {
                            const string error = "Invalid Unity object type";
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(error);
#endif
                            return (false, error);
                        }
                        try
                        {
                            result = GlobalObjectId.TryParse(value.valueJson, out GlobalObjectId id)
                                ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id)
                                : null;
                        }
                        catch (Exception e)
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(e);
#endif
                            return (false, e.Message);
                        }
                        break;
                    case SerFlavor.UnityStandard:
                        if (!RuntimeUtil.IsUnityCoreSerializable(targetType) || targetType.IsPrimitive ||
                            targetType == typeof(string) || targetType.IsEnum ||
                            typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                        {
                            string error = $"Invalid Unity standard type {targetType}";
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(error);
#endif
                            return (false, error);
                        }
                        try
                        {
                            result = JsonUtility.FromJson(value.valueJson, targetType);
                        }
                        catch (Exception e)
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(e);
#endif
                            return (false, e.Message);
                        }
                        break;
                    case SerFlavor.NonUnityObject:
                        if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                        {
                            const string error = "Unity objects require an object reference";
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(error);
#endif
                            return (false, error);
                        }
                        if (targetType.IsPrimitive || targetType == typeof(string) || targetType.IsEnum)
                        {
                            result = JsonConvert.DeserializeObject(value.valueJson, targetType);
                        }
                        else
                        {
                            (bool ok, object instance) = RestoreMembers(targetType, value.valueJson);
                            if (!ok)
                            {
                                return (false, instance);
                            }
                            result = instance;
                        }
                        break;
                    default:
                    {
                        string error = $"Unsupported serialization flavor {value.serFlavor}";
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning(error);
#endif
                        return (false, error);
                    }
                }
            }
            if (RuntimeUtil.IsNull(result) ||
                (value.collectType == CollectType.Value && !targetType.IsInstanceOfType(result)) ||
                !fieldType.IsInstanceOfType(result))
            {
                string error = $"Restored value cannot be assigned to {fieldType}";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }
            return (true, result);
        }

        private static (bool ok, object result) RestoreCollection(Type fieldType, Type targetType, TypedValue value)
        {
            Type collectionType;
            try
            {
                collectionType = Type.GetType(value.fieldType, true);
            }
            catch (Exception e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
                return (false, e.Message);
            }
            (bool notSupported, CollectType collectType, Type itemType) = IsArrayOrList(collectionType);
            if (notSupported || collectType != value.collectType || itemType != targetType ||
                value.serFlavor != SerFlavor.NonUnityObject || !fieldType.IsAssignableFrom(collectionType))
            {
                string error = $"Invalid collection for {fieldType}";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }
            List<TypedValue> items = JsonConvert.DeserializeObject<List<TypedValue>>(value.valueJson);
            if (items == null)
            {
                const string error = "Missing collection values";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }
            IList collection;
            try
            {
                collection = collectType == CollectType.Array
                    ? Array.CreateInstance(itemType, items.Count)
                    : (IList)Activator.CreateInstance(collectionType, true);
            }
            catch (Exception e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
                return (false, e.Message);
            }
            for (int index = 0; index < items.Count; index++)
            {
                (bool ok, object item) = FromTypedValue(itemType, items[index]);
                if (!ok)
                {
                    string error = $"Failed to restore collection element {index}: {item}";
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(error);
#endif
                    return (false, error);
                }
                try
                {
                    if (collectType == CollectType.Array)
                    {
                        collection[index] = item;
                    }
                    else
                    {
                        collection.Add(item);
                    }
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    return (false, e.Message);
                }
            }
            return (true, collection);
        }


        private static (bool ok, object result) RestoreMembers(Type targetType, string valueJson)
        {
            Dictionary<string, TypedValue> members = JsonConvert.DeserializeObject<Dictionary<string, TypedValue>>(valueJson);
            if (members == null || targetType.IsAbstract || targetType.IsInterface || targetType.ContainsGenericParameters)
            {
                string error = $"Cannot restore members of {targetType}";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }
            object instance;
            try
            {
                instance = Activator.CreateInstance(targetType, true);
            }
            catch (Exception e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
                try
                {
                    instance = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(targetType);
                }
                catch (Exception fallbackException)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(fallbackException);
#endif
                    return (false, fallbackException.Message);
                }
            }
            if (members.Count == 0)
            {
                return (true, instance);
            }
            bool anySuccess = false;
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            foreach (FieldInfo field in targetType.GetFields(bindAttr))
            {
                if (AbsRenderer.SkipTypeDrawing(field.FieldType) || !members.TryGetValue(field.Name, out TypedValue member))
                {
                    continue;
                }
                (bool ok, object fieldValue) = FromTypedValue(field.FieldType, member);
                if (!ok)
                {
                    continue;
                }
                try
                {
                    field.SetValue(instance, fieldValue);
                    anySuccess = true;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                }
            }
            foreach (PropertyInfo property in targetType.GetProperties(bindAttr))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length != 0 ||
                    AbsRenderer.SkipTypeDrawing(property.PropertyType) || !members.TryGetValue(property.Name, out TypedValue member))
                {
                    continue;
                }
                (bool ok, object propertyValue) = FromTypedValue(property.PropertyType, member);
                if (!ok)
                {
                    continue;
                }
                try
                {
                    property.SetValue(instance, propertyValue);
                    anySuccess = true;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                }
            }
            if (!anySuccess)
            {
                string error = $"No members restored for {targetType}";
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(error);
#endif
                return (false, error);
            }
            return (true, instance);
        }

        // Only List<T>, T[] is supported; nested/conjunction not
        private static (bool notSupported, CollectType collectType, Type itemType) IsArrayOrList(Type fieldType)
        {
            bool isArray = fieldType.IsArray;
            Type itemType;
            if (isArray)
            {
                if (fieldType.GetArrayRank() != 1)
                {
                    return (true, CollectType.Value, fieldType);
                }

                itemType = fieldType.GetElementType();
            }
            else if (IsList(fieldType))
            {
                itemType = fieldType.GetGenericArguments()[0];
            }
            else
            {
                return (false, CollectType.Value, fieldType);
            }

            bool isNested = itemType!.IsArray || IsList(itemType);
            return (isNested, isArray? CollectType.Array: CollectType.List, itemType);
        }

        private static bool IsList(Type itemType) =>
            itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(List<>);
    }
#endif
}
