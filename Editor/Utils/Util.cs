using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class Util
    {
        public static (float, string) GetCallbackFloat(object target, string by)
        {
            // SerializedProperty foundProperty = property.FindPropertyRelative(by) ??
            //                                    SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, by);
            // if (foundProperty != null)
            // {
            //     if (foundProperty.propertyType == SerializedPropertyType.Integer)
            //     {
            //         return (foundProperty.intValue, null);
            //     }
            //     if (foundProperty.propertyType == SerializedPropertyType.Float)
            //     {
            //         return (foundProperty.floatValue, null);
            //     }
            //
            //     return (-1, $"Expect int or float for `{by}`, get {foundProperty.propertyType}");
            // }
            //
            // object target = property.serializedObject.targetObject;

            // (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);
            // switch (found)
            // {
            //     case (ReflectUtils.GetPropType.NotFound, _):
            //     {
            //         return (-1, $"No field or method named `{by}` found on `{target}`");
            //     }
            //     case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
            //     {
            //         return ObjToFloat(propertyInfo.GetValue(target));
            //     }
            //     case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
            //     {
            //         return ObjToFloat(foundFieldInfo.GetValue(target));
            //     }
            //     case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
            //     {
            //         ParameterInfo[] methodParams = methodInfo.GetParameters();
            //         Debug.Assert(methodParams.All(p => p.IsOptional));
            //         // Debug.Assert(methodInfo.ReturnType == typeof(bool));
            //         return ObjToFloat(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
            //     }
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(found), found, null);
            // }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);

            if (found.Item1 == ReflectUtils.GetPropType.NotFound)
            {
                return (-1, $"No field or method named `{by}` found on `{target}`");
            }
            else if (found.Item1 == ReflectUtils.GetPropType.Property && found.Item2 is PropertyInfo propertyInfo)
            {
                return ObjToFloat(propertyInfo.GetValue(target));
            }
            else if (found.Item1 == ReflectUtils.GetPropType.Field && found.Item2 is FieldInfo foundFieldInfo)
            {
                return ObjToFloat(foundFieldInfo.GetValue(target));
            }
            else if (found.Item1 == ReflectUtils.GetPropType.Method && found.Item2 is MethodInfo methodInfo)
            {
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                Debug.Assert(methodParams.All(p => p.IsOptional));
                // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                return ObjToFloat(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(found), found, null);
            }

        }

        private static (float, string) ObjToFloat(object result)
        {
            switch (result)
            {
                case int intValue:
                    return (intValue, "");
                case float floatValue:
                    return (floatValue, "");
                default:
                    return (-1, $"{result} is neither int or float");
            }
        }

        public static float BoundFloatStep(float curValue, float start, float end, float step)
        {
            float distance = curValue - start;
            int stepRound = Mathf.RoundToInt(distance / step);
            float newValue = start + stepRound * step;
            return Mathf.Min(newValue, end);
        }

        public static int BoundIntStep(float curValue, float start, float end, int step)
        {
            int useStart = Mathf.CeilToInt(start);
            int useEnd = Mathf.FloorToInt(end);

            float distance = curValue - useStart;
            int stepRound = Mathf.RoundToInt(distance / step);
            int newValue = useStart + stepRound * step;
            return Mathf.Clamp(newValue, useStart, useEnd);
        }


    }
}
