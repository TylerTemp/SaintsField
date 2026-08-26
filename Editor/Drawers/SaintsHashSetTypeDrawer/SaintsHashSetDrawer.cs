using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(ReferenceHashSet<>), true)]
    [CustomPropertyDrawer(typeof(SaintsHashSet<>), true)]
    [CustomPropertyDrawer(typeof(SaintsHashSetAttribute), true)]
    public partial class SaintsHashSetDrawer: SaintsPropertyDrawer
    {
        private static readonly Color WarningColor = new Color(0.8490566f, 0.3003738f, 0.3003738f);
        private const double DebounceTime = 0.6d;

        private readonly struct HashSetFieldContext
        {
            public readonly Type RawType;
            public readonly SerializedProperty WrapProp;
            public readonly FieldInfo WrapField;
            public readonly object WrapParent;
            public readonly Type WrapType;
            public readonly Type ElementType;

            public HashSetFieldContext(Type rawType, SerializedProperty wrapProp, FieldInfo wrapField,
                object wrapParent, Type wrapType, Type elementType)
            {
                RawType = rawType;
                WrapProp = wrapProp;
                WrapField = wrapField;
                WrapParent = wrapParent;
                WrapType = wrapType;
                ElementType = elementType;
            }
        }

        private static bool IncreaseArraySize(int newValue, SerializedProperty prop)
        {
            int propSize = prop.arraySize;
            if (propSize == newValue)
            {
                return false;
            }

            prop.arraySize = newValue;
            return true;
        }

        private static void DecreaseArraySize(IReadOnlyList<int> indexReversed, SerializedProperty prop)
        {
            int curSize = prop.arraySize;
            foreach (int index in indexReversed.Where(each => each < curSize))
            {
                // Debug.Log($"Remove index {index}");
                prop.DeleteArrayElementAtIndex(index);
            }
        }

        private static void EnsureSerializedVersion(SerializedProperty property)
        {
            SerializedProperty propVersion = property.FindPropertyRelative("_saintsSerializedVersion");
            if (propVersion == null || propVersion.intValue == 1)
            {
                return;
            }

            propVersion.intValue = 1;
            propVersion.serializedObject.ApplyModifiedProperties();
        }

        private static bool UsesReferenceWrap(Type rawType) =>
            rawType != null && ReflectUtils.GetSelfAndBaseTypesFromType(rawType)
                .Any(each => each.IsGenericType && each.GetGenericTypeDefinition() == typeof(ReferenceHashSet<>));

        private string _propName;

        private string GetPropName(Type rawType)
        {
            // Type fieldType = ReflectUtils.GetElementType(rawType);

            // ReSharper disable once InvertIf
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_propName == null)
            {
                _propName = ReflectUtils.GetIWrapPropName(rawType);
            }

            Debug.Assert(_propName != null, $"Failed to find property name for {rawType}. Do you froget to define a `static string EditorPropertyName` (nameof(YourPropList))?");

            return _propName;
        }

        private static SerializedProperty FindPropertyCompact(SerializedProperty property, string propValuesNameCompact)
        {
            SerializedProperty prop = property.FindPropertyRelative(propValuesNameCompact);
            if (prop != null)
            {
                return prop;
            }

            SerializedProperty accProp = property;
            foreach (string propSegName in propValuesNameCompact.Split('.'))
            {
                SerializedProperty findProp = accProp.FindPropertyRelative(propSegName) ?? SerializedUtils.FindPropertyByAutoPropertyName(accProp, propSegName);
                Debug.Assert(findProp != null, $"Failed to find prop {propSegName} in {accProp.propertyPath}");
                accProp = findProp;
            }

            return accProp;
        }

        private static (FieldInfo targetInfo, object targetParent) GetTargetInfo(string propNameCompact, Type type,
            object saintsSerValue)
        {
            object keysIterTarget = saintsSerValue;
            List<object> keysParents = new List<object>(3)
            {
                saintsSerValue,
            };
            Type keysParentType = type;
            FieldInfo keysField = null;
            foreach (string propKeysName in propNameCompact.Split('.'))
            {
                foreach (Type each in ReflectUtils.GetSelfAndBaseTypesFromType(keysParentType))
                {
                    FieldInfo field = each.GetField(propKeysName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy);
                    if (field == null)
                    {
                        continue;
                    }

                    keysField = field;
                    keysParentType = keysField.FieldType;
                    keysIterTarget = keysField.GetValue(keysIterTarget);
                    keysParents.Add(keysIterTarget);
                    break;
                }

                Debug.Assert(keysField != null, $"Failed to get key {propKeysName} from {keysIterTarget}");
            }

            int keysParentsCount = keysParents.Count;
            object keysParent = keysParentsCount >= 2 ? keysParents[keysParentsCount - 2] : keysParents[0];
            return (keysField, keysParent);
        }

        private (string error, HashSetFieldContext context) TryGetHashSetContext(SerializedProperty property,
            FieldInfo info, object parent)
        {
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;

            Type rawType = insideArray ? ReflectUtils.GetElementType(info.FieldType) : info.FieldType;
            if (rawType == null)
            {
                return ($"Failed to get hash set raw type from {property.propertyPath}", default);
            }

            string propNameCompact = GetPropName(rawType);
            SerializedProperty wrapProp = FindPropertyCompact(property, propNameCompact);
            if (wrapProp == null)
            {
                return ($"Failed to find hash set prop `{propNameCompact}` from {property.propertyPath}", default);
            }

            object fieldValue;
            try
            {
                fieldValue = info.GetValue(parent);
            }
            catch (Exception e)
            {
                return ($"Failed to get hash set field value from {property.propertyPath}: {e.Message}", default);
            }

            if (insideArray)
            {
                if (fieldValue is not IEnumerable enumerable)
                {
                    return ($"Failed to get hash set array source from {property.propertyPath}", default);
                }

                object[] arrayValues = enumerable.Cast<object>().ToArray();
                if (arrayIndex < 0 || arrayIndex >= arrayValues.Length)
                {
                    return ($"Failed to get hash set array element at index {arrayIndex} from {property.propertyPath}",
                        default);
                }

                fieldValue = arrayValues[arrayIndex];
            }

            if (fieldValue == null)
            {
                return ($"Failed to get hash set target value from {property.propertyPath}", default);
            }

            (FieldInfo wrapField, object wrapParent) = GetTargetInfo(propNameCompact, rawType, fieldValue);
            if (wrapField == null)
            {
                return ($"Failed to get hash set field `{propNameCompact}` from {property.propertyPath}", default);
            }

            Type wrapType = ReflectUtils.GetElementType(wrapField.FieldType);
            if (wrapType == null)
            {
                return ($"Failed to get hash set element type from {property.propertyPath}", default);
            }

            Type elementType = wrapType.IsGenericType ? wrapType.GetGenericArguments()[0] : null;
            if (elementType == null)
            {
                return ($"Failed to unwrap hash set element type from {property.propertyPath}", default);
            }

            return ("", new HashSetFieldContext(rawType, wrapProp, wrapField, wrapParent, wrapType, elementType));
        }

        internal enum SearchParamType
        {
            TargetAndIndex,
            Target,
            Index,
        }

        private static IEnumerable<int> Search(ISaintsHashSetEditorTool hashSetTool, SerializedProperty wrapProp,
            Type elementType, string searchText, bool defaultSearch, bool objectSearch, object parent,
            (MethodInfo methodInfo, SearchParamType paramType) extraSearchMethod)
        {
            int size = wrapProp.arraySize;

            bool searchEmpty = string.IsNullOrEmpty(searchText);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (searchEmpty)
            {
                for (int index = 0; index < size; index++)
                {
                    yield return index;
                }
                yield break;
            }

            IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(searchText).ToArray();
            IReadOnlyList<ISaintsWrapEditorTool> values = hashSetTool.EditorSaintsValues();
            int useSize = Mathf.Min(size, values.Count);
            for (int index = 0; index < useSize; index++)
            {
                bool matched = defaultSearch && SerializedUtils.SearchArrayPropertyItem(
                    wrapProp.GetArrayElementAtIndex(index), searchTokens, objectSearch);

                if (!matched && extraSearchMethod.methodInfo != null)
                {
                    object value = values[index].EditorGetValue();
                    object[] methodParams = extraSearchMethod.paramType switch
                    {
                        SearchParamType.Index => new object[] { index, searchTokens },
                        SearchParamType.Target => new[] { value, searchTokens },
                        _ => new[] { value, index, searchTokens },
                    };
                    matched = (bool)extraSearchMethod.methodInfo.Invoke(parent, methodParams);
                }

                yield return matched ? index : -1;
            }

            for (int index = useSize; index < size; index++)
            {
                yield return defaultSearch && SerializedUtils.SearchArrayPropertyItem(
                    wrapProp.GetArrayElementAtIndex(index), searchTokens, objectSearch)
                    ? index
                    : -1;
            }
        }

        internal static (MethodInfo methodInfo, SearchParamType paramType) GetSearchMethodInfo(
            string methodName, Type targetType, Type elementType)
        {
            Type tokenListType = typeof(IEnumerable<ListSearchToken>);

            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(targetType))
            {
                foreach (MethodInfo methodInfo in eachType.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (methodInfo.Name != methodName || methodInfo.ReturnParameter?.ParameterType != typeof(bool))
                    {
                        continue;
                    }

                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    if (methodParams.Length == 0 ||
                        !tokenListType.IsAssignableFrom(methodParams[methodParams.Length - 1].ParameterType))
                    {
                        continue;
                    }

                    if (methodParams.Length == 3
                        && elementType.IsAssignableFrom(methodParams[0].ParameterType)
                        && methodParams[1].ParameterType == typeof(int))
                    {
                        return (methodInfo, SearchParamType.TargetAndIndex);
                    }

                    if (methodParams.Length == 2 && elementType.IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, SearchParamType.Target);
                    }

                    if (methodParams.Length == 2 && methodParams[0].ParameterType == typeof(int))
                    {
                        return (methodInfo, SearchParamType.Index);
                    }
                }
            }

            return (null, default);
        }
    }
}
