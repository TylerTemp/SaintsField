#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class SerializedListUtils
    {
        public static Func<(int min, int max)> CreateSizeLimits(SerializedProperty property,
            ArraySizeAttribute attribute, FieldInfo fieldInfo, object parent)
        {
            if (attribute == null)
            {
                return null;
            }

            return () =>
            {
                (string error, bool _, int min, int max) =
                    ArraySizeAttributeDrawer.GetMinMax(attribute, property, fieldInfo, parent);
                return error == "" ? (min, max) : (-1, -1);
            };
        }

        public static Func<int, IReadOnlyList<ListSearchToken>, bool> CreateExtraSearch(
            SerializedProperty property, Type elementType, object callbackOwner, string methodName)
        {
            if (callbackOwner == null || string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromType(callbackOwner.GetType()))
            {
                foreach (MethodInfo method in type.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (method.Name != methodName || method.ReturnType != typeof(bool))
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length < 2 || parameters.Length > 3 ||
                        !typeof(IEnumerable<ListSearchToken>).IsAssignableFrom(parameters[parameters.Length - 1].ParameterType))
                    {
                        continue;
                    }

                    bool valueAndIndex = parameters.Length == 3 &&
                                         elementType.IsAssignableFrom(parameters[0].ParameterType) &&
                                         parameters[1].ParameterType == typeof(int);
                    bool valueOnly = parameters.Length == 2 && elementType.IsAssignableFrom(parameters[0].ParameterType);
                    bool indexOnly = parameters.Length == 2 && !valueOnly && parameters[0].ParameterType == typeof(int);
                    if (!valueAndIndex && !valueOnly && !indexOnly)
                    {
                        continue;
                    }

                    return (index, tokens) =>
                    {
                        if (indexOnly)
                        {
                            return (bool)method.Invoke(callbackOwner, new object[] { index, tokens });
                        }

                        SerializedProperty item = property.GetArrayElementAtIndex(index);
                        (SerializedUtils.FieldOrProp member, object owner) = SerializedUtils.GetFieldInfoAndDirectParent(item);
                        MemberInfo memberInfo = member.IsField ? (MemberInfo)member.FieldInfo : member.PropertyInfo;
                        (string error, int _, object value) = Util.GetValue(item, memberInfo, owner);
                        if (error != "")
                        {
                            return false;
                        }
                        return (bool)method.Invoke(callbackOwner, valueAndIndex
                            ? new[] { value, index, tokens }
                            : new[] { value, tokens });
                    };
                }
            }
            return null;
        }
    }
}
#endif
