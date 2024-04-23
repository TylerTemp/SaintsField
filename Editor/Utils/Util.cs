using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Utils
{
    public static class Util
    {
        public static T LoadResource<T>(string resourcePath) where T: UnityEngine.Object
        {
            string[] resourceSearchFolder = {
                "Assets/Editor Default Resources/SaintsField",
                "Assets/SaintsField/Editor/Editor Default Resources/SaintsField",  // unitypackage
                // this is readonly, put it to last so user  can easily override it
                "Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField", // Unity UPM
            };

            T result = resourceSearchFolder
                .Select(resourceFolder => AssetDatabase.LoadAssetAtPath<T>($"{resourceFolder}/{resourcePath}"))
                // .Where(each => each != null)
                // .DefaultIfEmpty((T)EditorGUIUtility.Load(relativePath))
                .FirstOrDefault(each => each != null);
            if (result == null)
            {
                result = (T)EditorGUIUtility.Load(resourcePath);
            }

            // if (result == null)
            // {
            //     result = AssetDatabase.LoadAssetAtPath<T>(Path.Combine("Assets", iconPath).Replace("\\", "/"));
            // }
            Debug.Assert(result != null, $"{resourcePath} not found in {string.Join(", ", resourceSearchFolder)}");
            return result;
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

        public static void SignFieldValue(UnityEngine.Object targetObject, object curItem, object parentObj, FieldInfo field)
        {
            Undo.RecordObject(targetObject, "SignFieldValue");
            field.SetValue(parentObj, curItem);
//             if(parentType.IsValueType)  // hack struct :(
//             {
//                 // EditorUtility.SetDirty(property.serializedObject.targetObject);
//                 // field.SetValue(parentObj, curItem);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL_SET_VALUE
//                 Debug.Log($"SetValue {property.propertyType}, {property.propertyPath} on {property.serializedObject.targetObject}: {curItem}");
// #endif
//
//
//                 property.serializedObject.ApplyModifiedProperties();
//             }
        }

        public static void SignPropertyValue(SerializedProperty property, object newValue)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    property.objectReferenceValue = (UnityEngine.Object) newValue;
                    break;
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                    property.intValue = (int) newValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL_SET_VALUE
                    Debug.Log($"{property.propertyType}: set={property.intValue}");
#endif
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool) newValue;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float) newValue;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = newValue.ToString();
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color) newValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object) newValue;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2) newValue;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3) newValue;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4) newValue;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect) newValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = (int) newValue;
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = (char) newValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = (AnimationCurve) newValue;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds) newValue;
                    break;
                // case SerializedPropertyType.Gradient:
                //     property.gradientValue = (Gradient) curItem;
                //     break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion) newValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = (UnityEngine.Object) newValue;
                    break;
                // case SerializedPropertyType.FixedBufferSize:
                //     property.fixedBufferSize = (int) curItem;
                //     break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = (Vector2Int) newValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = (Vector3Int) newValue;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = (RectInt) newValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = (BoundsInt) newValue;
                    break;
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = (UnityEngine.Object) newValue;
                    break;
#endif
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.FixedBufferSize:
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
        }

        // public static string GetLabelString(SaintsPropertyDrawer.LabelState labelState)
        // {
        //     // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        //     switch (labelState)
        //     {
        //         case SaintsPropertyDrawer.LabelState.None:
        //             return "";
        //
        //         case SaintsPropertyDrawer.LabelState.AsIs:
        //             return null;
        //
        //         case SaintsPropertyDrawer.LabelState.EmptySpace:
        //             return " ";
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(labelState), labelState, null);
        //     }
        // }

        public static Label PrefixLabelUIToolKit(string label, int indentLevel)
        {
            return new Label(label)
            {
                style =
                {
                    flexShrink = 0,
                    flexGrow = 0,
                    minWidth = SaintsPropertyDrawer.LabelBaseWidth - indentLevel * 15,
                    left = SaintsPropertyDrawer.LabelLeftSpace,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            };
        }

        public static int ListIndexOfAction<T>(IEnumerable<T> lis, Func<T, bool> callback)
        {
            // foreach (int index in Enumerable.Range(0, lis.Count))
            // {
            //     if (callback(lis[index]))
            //     {
            //         return index;
            //     }
            // }

            foreach ((T value, int index) in lis.WithIndex())
            {
                if(callback(value))
                {
                    return index;
                }
            }

            return -1;
        }

        public static bool GetIsEqual(object curValue, object itemValue)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (curValue == null && itemValue == null)
            {
                // Debug.Log($"GetSelected null");
                return true;
            }
            if (curValue is UnityEngine.Object curValueObj
                && itemValue is UnityEngine.Object itemValueObj
                && curValueObj == itemValueObj)
            {
                // Debug.Log($"GetSelected Unity Object {curValue}");
                return true;
            }
            if (itemValue == null)
            {
                // Debug.Log($"GetSelected nothing null");
                // nothing
                return false;
            }
            // ReSharper disable once InvertIf
            if (itemValue.Equals(curValue))
            {
                // Debug.Log($"GetSelected equal {curValue}");
                return true;
            }

            return false;
        }

        public static (string error, bool isTruly) GetTruly(object target, string by)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();

            foreach (Type type in types)
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(type, by);

                if (getPropType == ReflectUtils.GetPropType.NotFound)
                {
                    continue;
                }

                if (getPropType == ReflectUtils.GetPropType.Property)
                {
                    return ("", ReflectUtils.Truly(((PropertyInfo)fieldOrMethodInfo).GetValue(target)));
                }
                if (getPropType == ReflectUtils.GetPropType.Field)
                {
                    return ("", ReflectUtils.Truly(((FieldInfo)fieldOrMethodInfo).GetValue(target)));
                }
                // ReSharper disable once InvertIf
                if (getPropType == ReflectUtils.GetPropType.Method)
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    object methodResult;
                    // try
                    // {
                    //     methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray())
                    // }
                    try
                    {
                        methodResult = methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return (e.InnerException.Message, false);

                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return (e.Message, false);
                    }
                    return ("", ReflectUtils.Truly(methodResult));
                }

            }

            // throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            string error = $"No field or method named `{by}` found on `{target}`";
            return (error, false);
        }

        public static (string error, T result) GetOf<T>(string by, T defaultValue, SerializedProperty property, FieldInfo fieldInfo, object target)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();

            foreach (Type type in types)
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(type, by);

                object genResult;
                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.NotFound:
                        continue;

                    case ReflectUtils.GetPropType.Property:
                        genResult = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                        break;
                    case ReflectUtils.GetPropType.Field:
                        genResult = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                        break;
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                        object rawValue = fieldInfo.GetValue(target);
                        object curValue = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);
                        object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), arrayIndex == -1
                            ? new[]
                            {
                                curValue,
                            }
                            : new []
                            {
                                curValue,
                                arrayIndex,
                            });

                        try
                        {
                            genResult = methodInfo.Invoke(target, passParams);
                        }
                        catch (TargetInvocationException e)
                        {
                            Debug.LogException(e);
                            Debug.Assert(e.InnerException != null);
                            return (e.InnerException.Message, defaultValue);
                        }
                        catch (Exception e)
                        {
                            // _error = e.Message;
                            Debug.LogException(e);
                            return (e.Message, defaultValue);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }

                // Debug.Log($"GetOf {genResult}/{genResult?.GetType()}/{genResult==null}");

                T finalResult;
                try
                {
                    // finalResult = (T)genResult;
                    finalResult = (T)Convert.ChangeType(genResult, typeof(T));
                }
                catch (InvalidCastException)
                {
                    // Debug.Log($"{genResult}/{genResult.GetType()} -> {typeof(T)}");
                    // Debug.LogException(e);
                    // return (e.Message, defaultValue);
                    try
                    {
                        finalResult = (T)genResult;
                    }
                    catch (InvalidCastException e)
                    {
                        Debug.LogException(e);
                        return (e.Message, defaultValue);
                    }
                }

                return ("", finalResult);
            }

            string error = $"No field or method named `{by}` found on `{target}`";
            return (error, defaultValue);
        }

        public static (string error, T result) GetMethodOf<T>(string by, T defaultValue, SerializedProperty property, FieldInfo fieldInfo, object target)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            object rawValue = fieldInfo.GetValue(target);
            object curValue = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);

            foreach (Type type in types)
            {
                MethodInfo methodInfo = type.GetMethod(by, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), arrayIndex == -1
                    ? new[]
                    {
                        curValue,
                    }
                    : new []
                    {
                        curValue,
                        arrayIndex,
                    });

                T result;
                try
                {
                    result = (T)methodInfo.Invoke(target, passParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    Debug.Assert(e.InnerException != null);
                    return (e.InnerException.Message, defaultValue);
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    return (e.Message, defaultValue);
                }
                catch (Exception e)
                {
                    // _error = e.Message;
                    Debug.LogException(e);
                    return (e.Message, defaultValue);
                }

                return ("", result);
            }

            string error = $"No field or method named `{by}` found on `{target}`";
            return (error, defaultValue);
        }

        public static UnityEngine.Object GetTypeFromObj(UnityEngine.Object fieldResult, Type fieldType)
        {
            UnityEngine.Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case GameObject go:
                    // ReSharper disable once RedundantCast
                    result = fieldType == typeof(GameObject) ? (UnityEngine.Object)go : go.GetComponent(fieldType);
                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    result = fieldType == typeof(GameObject)
                        // ReSharper disable once RedundantCast
                        ? (UnityEngine.Object)comp.gameObject
                        : comp.GetComponent(fieldType);
                    break;
                // default:
                //     Debug.Log(fieldResult.GetType());
                //     break;
            }

            return result;
        }
    }
}
