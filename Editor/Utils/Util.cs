using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Utils
{
    public static class Util
    {
        public static readonly string[] ResourceSearchFolder = {
            "Assets/Editor Default Resources/SaintsField",
            "Assets/SaintsField/Editor/Editor Default Resources/SaintsField",  // unitypackage
            // this is readonly, put it to last so user  can easily override it
            "Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField", // Unity UPM
        };

        public static T LoadResource<T>(string resourcePath) where T: UnityEngine.Object
        {
            foreach (T each in ResourceSearchFolder
                         .Select(resourceFolder => AssetDatabase.LoadAssetAtPath<T>($"{resourceFolder}/{resourcePath}")))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                if(each != null)
                {
                    return each;
                }
            }

            T result = (T)EditorGUIUtility.Load(resourcePath);
            Debug.Assert(result != null, $"{resourcePath} not found in {string.Join(", ", ResourceSearchFolder)}");
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

        public static SerializedUtils.FieldOrProp GetWrapProp(IWrapProp wrapProp)
        {
            string propName = ReflectUtils.GetIWrapPropName(wrapProp.GetType());
            const BindingFlags bind = BindingFlags.Instance | BindingFlags.NonPublic |
                                      BindingFlags.Public | BindingFlags.FlattenHierarchy;
            foreach (Type selfAndBaseType in ReflectUtils.GetSelfAndBaseTypes(wrapProp))
            {
                // Debug.Log(selfAndBaseType);
                FieldInfo actualFieldInfo = selfAndBaseType.GetField(propName, bind);
                // Debug.Log(actualFieldInfo);
                if (actualFieldInfo != null)
                {
                    return new SerializedUtils.FieldOrProp(actualFieldInfo);
                }

                PropertyInfo actualPropertyInfo = selfAndBaseType.GetProperty(propName, bind);
                // Debug.Log(actualPropertyInfo);
                if (actualPropertyInfo != null)
                {
                    return new SerializedUtils.FieldOrProp(actualPropertyInfo);
                }
                // Debug.Assert(actualFieldInfo != null);
                // actualFieldInfo.SetValue(wrapProp, curItem);
            }

            throw new ArgumentException($"No field or property named `{propName}` found on `{wrapProp}`");
        }

        public static object GetWrapValue(IWrapProp wrapProp)
        {
            SerializedUtils.FieldOrProp fieldOrProp = GetWrapProp(wrapProp);
            return fieldOrProp.IsField ? fieldOrProp.FieldInfo.GetValue(wrapProp) : fieldOrProp.PropertyInfo.GetValue(wrapProp);
        }

        public static void SignPropertyValue(SerializedProperty property, MemberInfo fieldInfo, object parent, object newValue)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    (string error, int _, object value) = GetValue(property, fieldInfo, parent);
                    if (error == "" && value is IWrapProp)
                    {
                        string propName = ReflectUtils.GetIWrapPropName(value.GetType());
                        SerializedProperty wrapProperty = property.FindPropertyRelative(propName) ??
                                                          SerializedUtils.FindPropertyByAutoPropertyName(property,
                                                              propName);
                        // Debug.Log($"set wrap value {wrapProperty.propertyPath} to {newValue}");
                        SignPropertyValue(wrapProperty, fieldInfo, parent, newValue);
                    }
                    else
                    {
                        if (property.isArray)
                        {
                            IEnumerable enumerator = (IEnumerable)newValue;
                            int index = 0;
                            foreach (object valueObject in enumerator)
                            {
                                property.arraySize = index + 1;
                                SerializedProperty arrayElement = property.GetArrayElementAtIndex(index);
                                SignPropertyValue(arrayElement, fieldInfo, parent, valueObject);
                                index++;
                            }
                        }
                        else
                        {
                            if(newValue != null)
                            {
                                foreach (SerializedProperty childProperty in SerializedUtils.GetPropertyChildren(property))
                                {
                                    // Debug.Log(newValue);
                                    // Debug.Log(newValue.GetType());
                                    // Debug.Log(childProperty.name);
                                    if(childProperty != null)
                                    {
                                        FieldInfo childFieldInfo = newValue.GetType().GetField(childProperty.name);
                                        if (childFieldInfo != null)
                                        {
                                            object childValue = childFieldInfo.GetValue(newValue);
                                            SignPropertyValue(childProperty, childFieldInfo, newValue, childValue);
                                        }
                                    }
                                }
                            }
                        }
                        // property.objectReferenceValue = (UnityEngine.Object)newValue;
                    }
                    break;
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                {
                    switch (newValue)
                    {
                        case long newValueLong:
                            property.longValue = newValueLong;
                            break;
                        case ulong newValueUlong:
                            property.longValue = (long)newValueUlong;
                            break;
                        case int newValueInt:
                            property.intValue = newValueInt;
                            break;
                        case uint newValueUInt:
                            property.intValue = (int)newValueUInt;
                            break;
                        default:
                            property.intValue = (int)newValue;
                            break;
                    }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL_SET_VALUE
                    Debug.Log($"{property.propertyType}: set={property.intValue}");
#endif
                }
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool) newValue;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float) newValue;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = newValue?.ToString();
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color) newValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object) newValue;
                    // Debug.Log($"#Util# {property.propertyPath} -> {newValue} = {property.objectReferenceValue}");
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
#if UNITY_2022_2_OR_NEWER
                case SerializedPropertyType.Gradient:
                    property.gradientValue = (Gradient) newValue;
                    break;
#endif
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion) newValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = (UnityEngine.Object) newValue;
                    break;
                // case SerializedPropertyType.FixedBufferSize:  // this is readonly
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
                // case SerializedPropertyType.Gradient:
                case SerializedPropertyType.FixedBufferSize:
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL
            Debug.Log($"check equal using both null on {itemValue} -> {curValue}");
#endif
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (curValue == null && itemValue == null)
            {
                // Debug.Log($"GetSelected null");
                return true;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL
            Debug.Log($"check equal using both UnityEngine.Object on {itemValue} -> {curValue}");
#endif
            if (curValue is UnityEngine.Object curValueObj
                && itemValue is UnityEngine.Object itemValueObj)
            {
                // Debug.Log($"GetSelected Unity Object {curValue}");
                return curValueObj == itemValueObj;
            }

            if (curValue is UnityEngine.Object curValue2)
            {
                // Debug.Log($"compare uObject {curValue2} with native {itemValue}/{itemValue == null}/{curValue2 == (itemValue as UnityEngine.Object)}/{curValue2 == null}");
                if (itemValue == null)
                {
                    return curValue2 == null;
                }
#pragma warning disable CS0252, CS0253
                return curValue2 == itemValue;
#pragma warning restore CS0252, CS0253
            }
            if(itemValue is UnityEngine.Object itemValue2)
            {
                if(curValue == null)
                {
                    return itemValue2 == null;
                }
#pragma warning disable CS0252, CS0253
                return curValue == itemValue2;
#pragma warning restore CS0252, CS0253
            }

            if (itemValue == null)
            {
                // Debug.Log($"GetSelected nothing null");
                // nothing
                return false;
            }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL
            Debug.Log($"check equal using .Equals on {itemValue} -> {curValue}");
#endif
            // ReSharper disable once InvertIf
            if (itemValue.Equals(curValue))
            {
                // Debug.Log($"GetSelected equal {curValue}");
                return true;
            }

            return false;
        }

        public static (string error, T value) GetOfNoParams<T>(object target, string by, T defaultValue)
        {
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypes(target))
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

                        try
                        {
                            genResult = methodInfo.Invoke(target, Array.Empty<object>());
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

        // public static (string error, bool isTruly) GetTruly(object target, string by)
        // {
        //     (string error, object value) = GetOfNoParams<object>(target, by, null);
        //     return error != ""
        //         ? (error, false)
        //         : ("", ReflectUtils.Truly(value));
        // }

        public static (string error, T result) GetOf<T>(string by, T defaultValue, SerializedProperty property, MemberInfo memberInfo, object target)
        {
            if (target == null)
            {
                return ("Target is null", defaultValue);
            }

            foreach (Type type in ReflectUtils.GetSelfAndBaseTypes(target))
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
                    {
                        FieldInfo fInfo = (FieldInfo)fieldOrMethodInfo;
                        genResult = fInfo.GetValue(target);
                        // Debug.Log($"{fInfo}/{fInfo.Name}, target={target} genResult={genResult}");
                    }
                        break;
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        object[] passParams;
                        if (property == null)
                        {
                            passParams = Array.Empty<object>();
                        }
                        else
                        {
                            (string error, int arrayIndex, object curValue) = GetValue(property, memberInfo, target);
                            if (error != "")
                            {
                                return (error, defaultValue);
                            }

                            passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), arrayIndex == -1
                                ? new[]
                                {
                                    curValue,
                                }
                                : new []
                                {
                                    curValue,
                                    arrayIndex,
                                });

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_UTIL_GET_OF
                            Debug.Log($"#Util# arrayIndex={arrayIndex}, rawValue={rawValue}, curValue={curValue}, fill={string.Join(",", passParams)}");
#endif
                        }

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
                    if (typeof(T) == typeof(string))
                    {
                        finalResult = (T)Convert.ChangeType(genResult == null? "": genResult.ToString(), typeof(T));
                    }
                    else
                    {
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
                }
                catch (FormatException)
                {
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

            return ($"No field or method named `{by}` found on `{target}`", defaultValue);
        }

        public static (string error, T result) GetMethodOf<T>(string by, T defaultValue, SerializedProperty property, MemberInfo memberInfo, object target)
        {
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            (string error, int arrayIndex, object curValue) = GetValue(property, memberInfo, target);
            if (error != "")
            {
                return (error, defaultValue);
            }

            foreach (Type type in ReflectUtils.GetSelfAndBaseTypes(target))
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

            return ($"No field or method named `{by}` found on `{target}`", defaultValue);
        }

        public static UnityEngine.Object GetTypeFromObj(UnityEngine.Object fieldResult, Type fieldType)
        {
            UnityEngine.Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case ScriptableObject so:
                    // result = fieldType.IsSubclassOf(typeof())
                {
                    if (fieldType.IsInstanceOfType(so))
                    {
                        result = so;
                    }
                }
                    break;
                case GameObject go:
                    // ReSharper disable once RedundantCast
                    if (fieldType == typeof(GameObject) || fieldType.IsInstanceOfType(go))
                    {
                        result = go;
                    }
                    else
                    {
                        Component r = null;
                        try
                        {
                            r = go.GetComponent(fieldType);
                        }
                        catch (ArgumentException)
                        {
                            // ignore
                        }

                        if (r)
                        {
                            result = r;
                        }
                    }

                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    if (fieldType == typeof(GameObject) || fieldType.IsSubclassOf(typeof(GameObject)))
                    {
                        result = comp.gameObject;
                    }
                    else
                    {
                        Component r;
                        try
                        {
                            r = comp.GetComponent(fieldType);
                        }
                        catch (ArgumentException)
                        {
                            return null;
                        }
                        if (r)  // life circle problem, need to check bool first
                        {
                            result = r;
                        }
                    }
                    break;

                case Texture2D _:
                {
                    if (fieldType == typeof(Sprite) || fieldType.IsSubclassOf(typeof(Sprite)))
                    {
                        string assetPath = AssetDatabase.GetAssetPath(fieldResult);
                        if(assetPath != "")
                        {
                            result = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        }

                        if (result == null)
                        {
                            goto default;
                        }
                    }
                    else
                    {
                        goto default;
                    }
                }
                    break;

                default:
                    // Debug.Log($"{fieldType}/{fieldResult}: {fieldType.IsInstanceOfType(fieldResult)}");
                    if (fieldType.IsInstanceOfType(fieldResult))
                    {
                        result = fieldResult;
                    }

                    break;
                //     Debug.Log(fieldResult.GetType());
                //     break;
            }

            return result;
        }

        public static IReadOnlyList<UnityEngine.Object> GetTargetsTypeFromObj(UnityEngine.Object fieldResult, Type fieldType)
        {
            // UnityEngine.Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case ScriptableObject so:
                    // result = fieldType.IsSubclassOf(typeof())
                {
                    if (fieldType.IsInstanceOfType(so))
                    {
                        return new[] { so };
                    }
                }
                    break;
                case GameObject go:
                    // ReSharper disable once RedundantCast
                    if (fieldType == typeof(GameObject) || fieldType.IsInstanceOfType(go))
                    {
                        return new[] { go };
                    }

                {
                    Component[] r = Array.Empty<Component>();
                    try
                    {
                        r = go.GetComponents(fieldType);
                    }
                    catch (ArgumentException)
                    {
                        // ignore
                    }

                    if (r.Length > 0)
                    {
                        return r;
                    }
                }

                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    {
                        if (fieldType == typeof(GameObject) || fieldType.IsSubclassOf(typeof(GameObject)))
                        {
                            return new[] { comp.gameObject };
                        }
                        Component[] r;
                        try
                        {
                            r = comp.GetComponents(fieldType);
                        }
                        catch (ArgumentException)
                        {
                            return null;
                        }
                        if (r.Length > 0)  // life circle problem, need to check bool first
                        {
                            return r;
                        }
                    }
                    break;

                case Texture2D _:
                {
                    if (fieldType == typeof(Sprite) || fieldType.IsSubclassOf(typeof(Sprite)))
                    {
                        string assetPath = AssetDatabase.GetAssetPath(fieldResult);
                        if(assetPath != "")
                        {
                            Sprite result = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                            if (result != null)
                            {
                                return new[] { result };
                            }
                            goto default;
                        }
                    }
                    else
                    {
                        goto default;
                    }
                }
                    break;

                default:
                    // Debug.Log($"{fieldType}/{fieldResult}: {fieldType.IsInstanceOfType(fieldResult)}");
                    if (fieldType.IsInstanceOfType(fieldResult))
                    {
                        return new[] { fieldResult };
                    }

                    break;
                //     Debug.Log(fieldResult.GetType());
                //     break;
            }

            return Array.Empty<UnityEngine.Object>();
        }

        private static bool ConditionEditModeChecker(EMode editorMode)
        {
            bool editorRequiresEdit = editorMode.HasFlagFast(EMode.Edit);
            bool editorRequiresPlay = editorMode.HasFlagFast(EMode.Play);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(editorRequiresEdit && editorRequiresPlay)
            {
                return true;
            }

            if(!editorRequiresEdit && !editorRequiresPlay)
            {
                return false;
            }

            return (
                !editorRequiresEdit || !EditorApplication.isPlaying
            ) && (
                !editorRequiresPlay || EditorApplication.isPlaying
            );
        }

        public static (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) ConditionChecker(IEnumerable<ConditionInfo> conditionInfos, SerializedProperty property, MemberInfo info, object target)
        {
            List<bool> callbackBoolResults = new List<bool>();
            List<string> errors = new List<string>();

            foreach (ConditionInfo conditionInfo in conditionInfos)
            {
                if (conditionInfo.Compare == LogicCompare.EditorMode)
                {
                    callbackBoolResults.Add(ConditionEditModeChecker((EMode) conditionInfo.Target));
                    continue;
                }
                // ReSharper disable once UseNegatedPatternInIsExpression
                if (!(conditionInfo.Target is string conditionStringTarget))
                {
                    Debug.Assert(conditionInfo.Compare == LogicCompare.Truly, $"target {conditionInfo.Target} should be truly compared");
                    bool thisTruly = ReflectUtils.Truly(conditionInfo.Target);
                    callbackBoolResults.Add(conditionInfo.Reverse ? !thisTruly : thisTruly);
                    continue;
                }

                (string error, object result) = conditionStringTarget.Contains(".")
                    ? AccGetOf<object>(conditionStringTarget, null, property, target)
                    : GetOf<object>(conditionStringTarget, null, property, info, target);

                if (error != "")
                {
                    errors.Add(error);
                    continue;
                }

                object value = conditionInfo.Value;
                if (conditionInfo.ValueIsCallback)
                {
                    Debug.Assert(value is string, $"value {value} of target {conditionInfo.Target} is not a string as a callback name");
                    (string errorValue, object callbackResult) = GetOf<object>((string)value, null, property, info, target);
                    if (errorValue != "")
                    {
                        errors.Add(errorValue);
                        continue;
                    }

                    value = callbackResult;
                }

                bool boolResult;
                switch (conditionInfo.Compare)
                {
                    case LogicCompare.Truly:
                        boolResult = ReflectUtils.Truly(result);
                        break;
                    case LogicCompare.Equal:
                        boolResult = GetIsEqual(result, value);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CONDITION
                        Debug.Log($"#Condition# {result} == {value} = {boolResult}");
#endif
                        break;
                    case LogicCompare.NotEqual:
                        boolResult = !GetIsEqual(result, value);
                        break;
                    case LogicCompare.GreaterThan:
                        boolResult = ((IComparable)result).CompareTo((IComparable)value) > 0;
                        break;
                    case LogicCompare.LessThan:
                        boolResult = ((IComparable)result).CompareTo((IComparable)value) < 0;
                        break;
                    case LogicCompare.GreaterEqual:
                        boolResult = ((IComparable)result).CompareTo((IComparable)value) >= 0;
                        break;
                    case LogicCompare.LessEqual:
                        boolResult = ((IComparable)result).CompareTo((IComparable)value) <= 0;
                        break;
                    case LogicCompare.BitAnd:
                        boolResult = ((int)result & (int)value) != 0;
                        break;
                    case LogicCompare.BitXor:
                        boolResult = ((int)result ^ (int)value) != 0;
                        break;
                    case LogicCompare.BitHasFlag:
                    {
                        int valueInt = (int)value;
                        boolResult = ((int)result & valueInt) == valueInt;
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(conditionInfo.Compare), conditionInfo.Compare, null);
                }
                callbackBoolResults.Add(conditionInfo.Reverse ? !boolResult : boolResult);
            }

            if (errors.Count > 0)
            {
                return (errors, Array.Empty<bool>());
            }

            return (Array.Empty<string>(), callbackBoolResults);
        }

        private static (string error, T result) AccGetOf<T>(string by, T defaultValue, SerializedProperty property,
            object parent)
        {
            object accParent = parent;
            // MemberInfo accMemberInfo = memberInfo;
            (string error, T result) thisResult = ("No Attributes", defaultValue);

            foreach (string attrName in by.Split(SerializedUtils.pathSplitSeparator))
            {
                MemberInfo accMemberInfo = null;
                foreach (Type type in ReflectUtils.GetSelfAndBaseTypes(accParent))
                {
                    MemberInfo[] members = type.GetMember(attrName,
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.Static |
                        BindingFlags.FlattenHierarchy);
                    if (members.Length <= 0) continue;
                    accMemberInfo = members[0];
                    break;
                }

                thisResult = GetOf(attrName, defaultValue, property, accMemberInfo, accParent);
                // Debug.Log($"{attrName} = {thisResult.result}");
                if (thisResult.error != "")
                {
                    return thisResult;
                }
                accParent = thisResult.result;
            }
            return thisResult;

        }

        public static void BindEventWithValue(UnityEventBase unityEventBase, MethodInfo methodInfo, Type[] invokeRequiredTypeArr, object target, object value)
        {
            switch (value)
            {
                case bool boolValue:
                    UnityEventTools.AddBoolPersistentListener(
                        unityEventBase,
                        (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>),
                            target, methodInfo),
                        boolValue);
                    return;
                case float floatValue:
                    UnityEventTools.AddFloatPersistentListener(
                        unityEventBase,
                        (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>),
                            target, methodInfo),
                        floatValue);
                    return;
                case int intValue:
                    UnityEventTools.AddIntPersistentListener(
                        unityEventBase,
                        (UnityAction<int>)Delegate.CreateDelegate(typeof(UnityAction<int>),
                            target, methodInfo),
                        intValue);
                    return;

                case string stringValue:
                    UnityEventTools.AddStringPersistentListener(
                        unityEventBase,
                        (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>),
                            target, methodInfo),
                        stringValue);
                    return;

                case UnityEngine.Object unityObjValue:
                    UnityEventTools.AddObjectPersistentListener(
                        unityEventBase,
                        (UnityAction<UnityEngine.Object>)Delegate.CreateDelegate(typeof(UnityAction<UnityEngine.Object>),
                            target, methodInfo),
                        unityObjValue);
                    return;

                default:
                {
                    // Type[] invokeRequiredTypeArr = invokeRequiredTypes.ToArray();
                    // when method requires 1 parameter
                    // if value given, will go to the logic above, which is static parameter value
                    // otherwise, it's a method dynamic bind

                    // so, all logic here must be dynamic bind
                    // ParameterInfo[] methodParams = ;
                    Debug.Assert(methodInfo.GetParameters().Length == invokeRequiredTypeArr.Length);

                    Type genericAction;

                    // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                    switch (invokeRequiredTypeArr.Length)
                    {
                        case 0:
                            genericAction = typeof(UnityAction);
                            break;
                        case 1:
                            genericAction = typeof(UnityAction<>);
                            break;
                        case 2:
                            genericAction = typeof(UnityAction<,>);
                            break;
                        case 3:
                            genericAction = typeof(UnityAction<,,>);
                            break;
                        case 4:
                            genericAction = typeof(UnityAction<,,,>);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(invokeRequiredTypeArr.Length), invokeRequiredTypeArr.Length, null);
                    }

                    // Debug.Log(invokeRequiredTypeArr);

                    Type genericActionIns = genericAction.MakeGenericType(invokeRequiredTypeArr);
                    MethodInfo addPersistentListenerMethod = unityEventBase
                        .GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                        .First(each => each.Name == "AddPersistentListener" && each.GetParameters().Length == 1);
                    Delegate callback = Delegate.CreateDelegate(genericActionIns, target,
                        methodInfo);
                    addPersistentListenerMethod.Invoke(unityEventBase, new object[]
                    {
                        callback,
                    });
                }
                    return;
            }
        }

        public struct SaintsInterfaceInfo
        {
            public string Error;
            public Type InterfaceType;
            public Type FieldType;
            public SerializedProperty TargetProperty;
        }

        public static SaintsInterfaceInfo GetSaintsInterfaceInfo(SerializedProperty property, IWrapProp wrapProp)
        {
            Type interfaceType = null;
            Type fieldType = wrapProp.GetType();

            Type mostBaseType = ReflectUtils.GetMostBaseType(fieldType);
            if (mostBaseType.IsGenericType && mostBaseType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
            {
                IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
                if (genericArguments.Count == 2)
                {
                    interfaceType = genericArguments[1];
                }
            }
            SerializedProperty targetProperty = property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(wrapProp.GetType())) ??
                             SerializedUtils.FindPropertyByAutoPropertyName(property,
                                 ReflectUtils.GetIWrapPropName(wrapProp.GetType()));

            if(targetProperty == null)
            {
                return new SaintsInterfaceInfo
                {
                    Error = $"{ReflectUtils.GetIWrapPropName(wrapProp.GetType())} not found in {property.propertyPath}",
                };
            }

            SerializedUtils.FieldOrProp wrapFieldOrProp = GetWrapProp(wrapProp);
            fieldType = wrapFieldOrProp.IsField
                ? wrapFieldOrProp.FieldInfo.FieldType
                : wrapFieldOrProp.PropertyInfo.PropertyType;

            return new SaintsInterfaceInfo
            {
                Error = "",
                InterfaceType = interfaceType,
                FieldType = fieldType,
                TargetProperty = targetProperty,
            };
        }

        public static int CombineHashCode<T1, T2>(T1 object1, T2 object2)
        {
            // HashCode.Combine does not exist in old Unity
#if UNITY_2021_1_OR_NEWER
            return HashCode.Combine(object1, object2);
#else
            var hashCode = 17;
            hashCode *= 31 + object1?.GetHashCode() ?? 0;
            hashCode *= 31 + object2?.GetHashCode() ?? 0;
            return hashCode;
#endif
        }
        public static int CombineHashCode<T1, T2, T3>(T1 object1, T2 object2, T3 object3)
        {
            // HashCode.Combine does not exist in old Unity
#if UNITY_2021_1_OR_NEWER
            return HashCode.Combine(object1, object2, object3);
#else
            var hashCode = CombineHashCode(object1, object2);
            hashCode *= 31 + object3?.GetHashCode() ?? 0;
            return hashCode;
#endif
        }

        public static int CombineHashCode<T1, T2, T3, T4>(T1 object1, T2 object2, T3 object3, T4 object4)
        {
            // HashCode.Combine does not exist in old Unity
#if UNITY_2021_1_OR_NEWER
            return HashCode.Combine(object1, object2, object3, object4);
#else
            var hashCode = CombineHashCode(object1, object2, object3);
            hashCode *= 31 + object4?.GetHashCode() ?? 0;
            return hashCode;
#endif
        }

        public static (string error, int index, object value) GetValue(SerializedProperty property, MemberInfo fieldInfo, object parent)
        {
            int arrayIndex;
            if (property == null)
            {
                arrayIndex = -1;
            }
            else
            {
                try
                {
                    arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                }
                catch (NullReferenceException e)
                {
                    return (e.Message, -1, null);
                }
                catch (ObjectDisposedException e)
                {
                    return (e.Message, -1, null);
                }
            }

            return GetValueAtIndex(arrayIndex, fieldInfo, parent);
        }

        public static (string error, int index, object value) GetValueAtIndex(int arrayIndex, MemberInfo fieldInfo, object parent)
        {
            if (fieldInfo == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError($"MemberInfo is null");
#endif
                return ("No MemberInfo given", arrayIndex, null);
            }

            if (parent == null)
            {
                return ("No parent given", arrayIndex, null);
            }

            object rawValue;
            if (fieldInfo.MemberType == MemberTypes.Field)
            {
                rawValue = ((FieldInfo)fieldInfo).GetValue(parent);
            }
            else if (fieldInfo.MemberType == MemberTypes.Property)
            {
                rawValue = ((PropertyInfo)fieldInfo).GetValue(parent);
            }
            else
            {
                return ($"Unable to get value from {fieldInfo} ({fieldInfo.MemberType})", -1, null);
            }

            if (arrayIndex == -1)
            {
                return ("", -1, rawValue);
            }

            (string indexError, object indexResult) = GetValueAtIndex(rawValue, arrayIndex);
            if (indexError != "")
            {
                return (indexError, -1, null);
            }

            return ("", arrayIndex, indexResult);
        }

        public static (SerializedProperty arrayProperty, int index, string error) GetArrayProperty(SerializedProperty property, MemberInfo info, object parent)
        {
            int arrayIndex;
            try
            {
                arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            }
            catch (NullReferenceException)
            {
                return (null, -1, "Property disposed");
            }
            catch (ObjectDisposedException)
            {
                return (null, -1, "Property disposed");
            }

            if (arrayIndex != -1)
            {
                (string error, SerializedProperty arrProp) = SerializedUtils.GetArrayProperty(property);
                return (arrProp, arrayIndex, error);
            }

            (string propError, int index, object propertyValue) = GetValue(property, info, parent);
            if (propError != "")
            {
                return (null, index, propError);
            }

            if (propertyValue is IWrapProp)
            {
                string targetPropName = ReflectUtils.GetIWrapPropName(propertyValue.GetType());
                SerializedProperty arrProp = property.FindPropertyRelative(targetPropName) ??
                          SerializedUtils.FindPropertyByAutoPropertyName(property, targetPropName);
                if (arrProp == null)
                {
                    return (null, arrayIndex, $"{targetPropName} not found in {property.propertyPath}");
                }

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if(!arrProp.isArray)
                {
                    return (arrProp, arrayIndex, $"{targetPropName} is not an array in {property.propertyPath}");
                }

                return (arrProp, arrayIndex, "");
            }

            return (null, arrayIndex, $"{property.propertyPath} is not an array");
        }


        public static (string error, object result) GetValueAtIndex(object source, int index)
        {
            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(source is IEnumerable enumerable))
            {
                throw new Exception($"Not a enumerable {source}");
            }

            if (source is Array arr)
            {
                object result;
                try
                {
                    result = arr.GetValue(index);
                }
                catch (IndexOutOfRangeException e)
                {
                    return (e.Message, null);
                }

                return ("", result);
            }
            if (source is IList list)
            {
                object result;
                try
                {
                    result = list[index];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    return (e.Message, null);
                }

                return ("", result);
            }

            // Debug.Log($"start check index in {source}");
            foreach ((object result, int searchIndex) in enumerable.Cast<object>().WithIndex())
            {
                // Debug.Log($"check index {searchIndex} in {source}");
                if(searchIndex == index)
                {
                    return ("", result);
                }
            }

            return ($"Not found index {index} in {source}", null);
        }

        public static void PropertyChangedCallback(SerializedProperty property, MemberInfo info, Action<object> onValueChangedCallback)
        {
            object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (noCacheParent == null)
            {
                Debug.LogWarning("Property disposed unexpectedly, skip onChange callback.");
                return;
            }
            (string error, int _, object curValue) = GetValue(property, info, noCacheParent);
            if (error == "")
            {
                onValueChangedCallback(curValue);
            }
        }

        #region Scene Related

        public struct TargetWorldPosInfo
        {
            public string Error;

            public bool IsTransform;

            public Transform Transform;
            public Vector3 WorldPos;

            public override string ToString()
            {
                if (Error != "")
                {
                    return $"<TargetWorldPosInfo Error={Error} />";
                }
                return IsTransform
                    ? $"<TargetWorldPosInfo Transform={Transform.gameObject.name} />"
                    : $"<TargetWorldPosInfo WorldPos={WorldPos} />";
            }
        }

        public static TargetWorldPosInfo GetPropertyTargetWorldPosInfoSpace(string space, SerializedProperty property, MemberInfo info, object parent)
        {
            try
            {
                SerializedPropertyType _ = property.propertyType;
            }
            catch (InvalidCastException)
            {
                return new TargetWorldPosInfo
                {
                    Error = $"Property disposed",
                };
            }
            catch (NullReferenceException)
            {
                return new TargetWorldPosInfo
                {
                    Error = $"Property disposed",
                };
            }
            catch (ObjectDisposedException)
            {
                return new TargetWorldPosInfo
                {
                    Error = $"Property disposed",
                };
            }


            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                {
                    (string error, int _, object propertyValue) = GetValue(property, info, parent);

                    if (error == "" && propertyValue is IWrapProp wrapProp)
                    {
                        object propWrapValue = GetWrapValue(wrapProp);
                        switch (propWrapValue)
                        {
                            case null:
                                return new TargetWorldPosInfo { Error = "Target is null" };
                            case GameObject wrapGo:
                                return new TargetWorldPosInfo
                                {
                                    Error = "",
                                    IsTransform = true,
                                    Transform = wrapGo.transform,
                                };
                            case Component wrapComp:
                                return new TargetWorldPosInfo
                                {
                                    Error = "",
                                    IsTransform = true,
                                    Transform = wrapComp.transform,
                                };
                            default:
                                return new TargetWorldPosInfo
                                {
                                    Error = $"{propWrapValue} is not GameObject or Component",
                                };
                        }
                    }

                    return new TargetWorldPosInfo
                    {
                        Error = $"{property.propertyType} is not supported",
                    };
                }
                case SerializedPropertyType.ObjectReference when property.objectReferenceValue is GameObject isGo:
                    return new TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = true,
                        Transform = isGo.transform,
                    };
                case SerializedPropertyType.ObjectReference when property.objectReferenceValue is Component comp:
                    return new TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = true,
                        Transform = comp.transform,
                    };
                    // return ("", comp.transform);
                    // go = ((Component) property.objectReferenceValue)?.gameObject;
                case SerializedPropertyType.Vector2:
                    return GetValueFromVectorSpace(space, property, info, parent, property.vector2Value);
                case SerializedPropertyType.Vector3:
                    return GetValueFromVectorSpace(space, property, info, parent, property.vector3Value);
                default:
                    return new TargetWorldPosInfo
                    {
                        Error = $"{property.propertyType} is not supported",
                    };
            }
        }

        private static TargetWorldPosInfo GetValueFromVectorSpace(string space, SerializedProperty property,
            MemberInfo info, object parent,
            Vector3 v3Value)
        {
            if (space is null)
            {
                return new TargetWorldPosInfo
                {
                    Error = "",
                    IsTransform = false,
                    WorldPos = v3Value,
                };
            }

            if(space == "this")
            {
                (string error, Transform container) = GetContainingTransform(property);
                if (error != "")
                {
                    return new TargetWorldPosInfo
                    {
                        Error = error,
                    };
                }

                return new TargetWorldPosInfo
                {
                    Error = "",
                    IsTransform = false,
                    WorldPos = container.TransformPoint(v3Value),
                };
            }

            (string callbackError, int _, object value) = GetValue(property, info, parent);
            if (callbackError != "")
            {
                return new TargetWorldPosInfo
                {
                    Error = callbackError,
                };
            }

            switch (value)
            {
                case GameObject go:
                    return new TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = false,
                        WorldPos = go.transform.TransformPoint(v3Value),
                    };
                case Component comp:
                    return new TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = false,
                        WorldPos = comp.transform.TransformPoint(v3Value),
                    };
                default:
                    return new TargetWorldPosInfo
                    {
                        Error = $"{value} is not GameObject or Component",
                    };
            }
        }

        public static TargetWorldPosInfo GetValueFromVector(string space, SerializedProperty property,
            Vector3 v3Value)
        {
            if (space == null)
            {
                return new TargetWorldPosInfo
                {
                    Error = "",
                    IsTransform = false,
                    WorldPos = v3Value,
                };
            }

            (string error, Transform container) = GetContainingTransform(property);
            if (error != "")
            {
                return new TargetWorldPosInfo
                {
                    Error = error,
                };
            }

            return new TargetWorldPosInfo
            {
                Error = "",
                IsTransform = false,
                Transform = container,
                WorldPos = container.TransformPoint(v3Value),
            };
        }

        private static (string error, Transform container) GetContainingTransform(SerializedProperty property)
        {
            UnityEngine.Object targetObj;
            try
            {
                targetObj = property.serializedObject.targetObject;
            }
            catch (ArgumentNullException e)
            {
                return (e.Message, null);
            }
            catch (NullReferenceException e)
            {
                return (e.Message, null);
            }
            catch (ObjectDisposedException e)
            {
                return (e.Message, null);
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (targetObj)
            {
                case GameObject go:
                    return ("", go.transform);
                case Component comp:
                    return ("", comp.transform);
                default:
                    return ($"Target is not GameObject or Component", null);
            }
        }

        #endregion
    }
}
