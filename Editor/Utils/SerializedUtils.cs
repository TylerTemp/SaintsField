using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils
{
    public static class SerializedUtils
    {
        public static readonly char[] pathSplitSeparator = { '.' };

        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedObject obj, string propName)
        {
            return obj.FindProperty($"<{propName}>k__BackingField");
        }

        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedProperty property, string propName)
        {
            return property.FindPropertyRelative($"<{propName}>k__BackingField");
        }

        public static bool IsArrayOrDirectlyInsideArray(SerializedProperty property)
        {
            bool extractFromArrayType = property.propertyType == SerializedPropertyType.Generic && property.isArray;
            if (extractFromArrayType)
            {
                return true;
            }
            int index = PropertyPathIndex(property.propertyPath);
            return index >= 0;
        }

        public readonly struct FieldOrProp
        {
            public readonly bool IsField;
            public readonly FieldInfo FieldInfo;
            public readonly PropertyInfo PropertyInfo;

            public FieldOrProp(FieldInfo fieldInfo)
            {
                IsField = true;
                FieldInfo = fieldInfo;
                PropertyInfo = null;
            }

            public FieldOrProp(PropertyInfo propertyInfo)
            {
                IsField = false;
                FieldInfo = null;
                PropertyInfo = propertyInfo;
            }
        }

        public static (FieldOrProp fieldOrProp, object parent) GetFieldInfoAndDirectParent(SerializedProperty property)
        {
            string originPath = property.propertyPath;
            string[] propPaths = originPath.Split(pathSplitSeparator);
            (bool arrayTrim, string[] propPathSegments) = TrimEndArray(propPaths);
            if (arrayTrim)
            {
                propPaths = propPathSegments;
            }

            object sourceObj = property.serializedObject.targetObject;
            FieldOrProp fieldOrProp = default;

            bool preNameIsArray = false;
            foreach (string propSegName in propPaths)
            {
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
            }

            return (fieldOrProp, sourceObj);
        }

        public static string GetUniqueIdArray(SerializedProperty property)
        {
            string[] paths = property.propertyPath.Split(pathSplitSeparator);

            (bool _, string[] propPathSegments) = TrimEndArray(paths);
            return $"{property.serializedObject.targetObject.GetInstanceID()}_{string.Join(".", propPathSegments)}";
        }

        public static (string error, SerializedProperty property) GetArrayProperty(SerializedProperty property)
        {
            // Debug.Log(property.propertyPath);
            string[] paths = property.propertyPath.Split(pathSplitSeparator);

            (bool arrayTrim, string[] propPathSegments) = TrimEndArray(paths);
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

        private static (bool trimed, string[] propPathSegs) TrimEndArray(string[] propPathSegments)
        {

            int usePathLength = propPathSegments.Length;

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
            string[] propPaths = new string[usePathLength - 2];
            Array.Copy(propPathSegments, 0, propPaths, 0, usePathLength - 2);
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
            MemberInfo memberInfo = fieldOrProp.IsField ? (MemberInfo)fieldOrProp.FieldInfo : fieldOrProp.PropertyInfo;
            return (ReflectCache.GetCustomAttributes<T>(memberInfo), sourceObj);
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
                    return new FieldOrProp(field);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    // return property.GetValue(source, null);
                    // Debug.Log($"return prop {property.Name} by {name}");
                    return new FieldOrProp(property);
                }

                type = type.BaseType;
            }


            throw new Exception($"Unable to get {name} from {source}");
        }

        public static int PropertyPathIndex(string propertyPath)
        {
            string[] propPaths = propertyPath.Split(pathSplitSeparator);
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
            return $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";
        }

        public static bool IsOk(SerializedProperty property)
        {
            try
            {
                int _ = property.serializedObject.targetObject.GetInstanceID();
                string __ = property.propertyPath;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }

            return true;
        }

        public static Object GetSerObject(SerializedProperty property, MemberInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return property.objectReferenceValue;
            }

            (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

            if (error == "" && propertyValue is IWrapProp wrapProp)
            {
                return (Object)Util.GetWrapValue(wrapProp);
            }

            return null;
        }

        public static IEnumerable<SerializedProperty> GetAllField(SerializedObject obj)
        {
            obj.UpdateIfRequiredOrScript();
            SerializedProperty iterator = obj.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                // using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                if("m_Script" != iterator.propertyPath)
                {
                    yield return iterator;
                }
            }
        }

        public static IEnumerable<int> SearchArrayProperty(SerializedProperty property, string search)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty childProperty = property.GetArrayElementAtIndex(i);
                if(SearchProp(childProperty, search))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"found: {childProperty.propertyPath}");
#endif
                    yield return i;
                }
            }
        }

        private static bool SearchProp(SerializedProperty property, string search)
        {
            SerializedPropertyType propertyType;
            try
            {
                propertyType = property.propertyType;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }

            // Debug.Log($"{property.propertyPath} is {propertyType}");

            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString().Contains(search);
                case SerializedPropertyType.Float:
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    return property.floatValue.ToString().Contains(search);
                case SerializedPropertyType.String:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"{property.propertyPath}={property.stringValue} contains {search}={property.stringValue?.Contains(search)}");
#endif
                    return property.stringValue?.Contains(search) ?? false;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString().Contains(search);
                case SerializedPropertyType.ObjectReference:
                    // ReSharper disable once Unity.NoNullPropagation
                    if (property.objectReferenceValue is ScriptableObject so)
                    {
                        return SearchSoProp(so, search);
                    }
                    return property.objectReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Enum:
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if(property.enumNames.Length <= property.enumValueIndex || property.enumValueIndex < 0)
                    {
                        return false;
                    }
                    return property.enumNames[property.enumValueIndex].Contains(search);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString().Contains(search);
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString().Contains(search);
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString().Contains(search);
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString().Contains(search);
                case SerializedPropertyType.ArraySize:
                    if (property.isArray)
                    {
                        return property.arraySize.ToString().Contains(search);
                    }
                    goto default;
                case SerializedPropertyType.Character:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue.ToString().Contains(search);
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString().Contains(search);
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString().Contains(search);
                case SerializedPropertyType.ExposedReference:
                    // ReSharper disable once Unity.NoNullPropagation
                    return property.exposedReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString().Contains(search);
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString().Contains(search);
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString().Contains(search);
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString().Contains(search);
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString().Contains(search);
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename.Contains(search);
#endif
                case SerializedPropertyType.Generic:
                {
                    if (property.isArray)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                        Debug.Log($"is array {property.arraySize}: {property.propertyPath}");
#endif
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            if (SearchProp(property.GetArrayElementAtIndex(i), search))
                                return true;
                        }
                        return false;
                    }

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (SerializedProperty child in SerializedUtils.GetPropertyChildren(property))
                    {
                        if(SearchProp(child, search))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"found child: {child.propertyPath}");
#endif
                            return true;
                        }
                    }

                    return false;
                }
                case SerializedPropertyType.Gradient:
#if UNITY_2021_1_OR_NEWER
                case SerializedPropertyType.Hash128:
#endif
                default:
                    return false;
            }
        }

        private static bool SearchSoProp(ScriptableObject so, string search)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using(SerializedObject serializedObject = new SerializedObject(so))
            {
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (SearchProp(iterator, search))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
