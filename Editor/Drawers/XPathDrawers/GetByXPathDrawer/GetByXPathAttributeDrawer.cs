using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GetByXPathAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentsAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute))]
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute))]
    [CustomPropertyDrawer(typeof(FindComponentAttribute))]
    public partial class GetByXPathAttributeDrawer: SaintsPropertyDrawer
    {

        private static (bool valid, object value) ValidateXPathResult(object each, Type expectType, Type expectInterface)
        {
            object result;
            // Debug.Log($"{each}");
            if (expectType.IsInstanceOfType(each))
            {
                result = each;
            }
            else if (each is Object uObject)
            {
                Object r = Util.GetTypeFromObj(uObject, expectType);

                if (r == null)
                {
                    return (false, null);
                }

                result = r;
            }
            else
            {
                return (false, null);
            }

            if (expectInterface == null)
            {
                return (true, result);
            }

            if (result is GameObject resultGo)
            {
                result = resultGo.transform;
            }
            if(result is Component resultComponent)
            {
                foreach (Component component in resultComponent.GetComponents<Component>())
                {
                    // Debug.Log($"{expectInterface}/{component}")
                    if (component == null)  // some broken component
                    {
                        continue;
                    }
                    if (expectInterface.IsAssignableFrom(component.GetType()))
                    {
                        return (true, component);
                    }
                }
            }

            bool valid = expectInterface.IsAssignableFrom(result.GetType());
            return valid ? (true, result) : (false, null);
        }

        private static (string error, Type expectType, Type expectInterface) GetExpectedTypeOfProp(
            SerializedProperty property, MemberInfo info)
        {
            Type targetType = info is FieldInfo fi ? fi.FieldType : ((PropertyInfo)info).PropertyType;

            Type rawType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(targetType)
                : targetType;

            // Debug.Log($"targetType={targetType}, rawType={rawType}");
            if (!typeof(IWrapProp).IsAssignableFrom(rawType))
            {
                return ("", rawType, GetInterface(rawType));
            }

            // Debug.Log($"Raw Type: {rawType}");

            Type wrapType = ReflectUtils.GetIWrapPropType(rawType);
            if (wrapType == null)
            {
                return ($"Failed to get wrap type from {property.propertyPath}", null, null);
            }

            return ("", wrapType, GetInterface(rawType));
        }

        private static Type GetInterface(Type rawType)
        {
            Type mostBaseType = ReflectUtils.GetMostBaseType(rawType);
            if(ReflectUtils.IsSubclassOfRawGeneric(typeof(SaintsInterface<,>), mostBaseType))
            {
                return mostBaseType.GetGenericArguments()[1];
            }

            return null;
        }

        private static bool Mismatch(object originalValue, object targetValue)
        {
            bool equal = Util.GetIsEqual(originalValue, targetValue);
            // ReSharper disable once InvertIf
            if (!equal && originalValue is IWrapProp wrapProp)
            {
                object originalWrapValue = Util.GetWrapValue(wrapProp);
                equal = Util.GetIsEqual(originalWrapValue, targetValue);
            }

            return !equal;
        }

        private static void UpdateSharedCacheBase(GetByXPathGenericCache target, SerializedProperty property, FieldInfo info)
        {
            target.Error = "";
            if (target.Parent == null || target.GetByXPathAttributes == null)
            {
                (GetByXPathAttribute[] _, object parent) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(property);
                if(parent == null)
                {
                    target.Error = "Can not get parent for property";
                    return;
                }

                target.Parent = parent;
            }

            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;

            if (isArray)
            {
                bool reSign = target.ArrayProperty == null;
                if (!reSign)
                {
                    try
                    {
                        int _ = target.ArrayProperty.arraySize;
                    }
                    catch (NullReferenceException)
                    {
                        reSign = true;
                    }
                    catch (ObjectDisposedException)
                    {
                        reSign = true;
                    }
                }
                if(reSign)
                {
                    (SerializedProperty arrProperty, int _, string arrError) =
                        Util.GetArrayProperty(property, info, target.Parent);
                    if (arrError != "")
                    {
                        target.Error = arrError;
                        target.ArrayProperty = null;
                        return;
                    }

                    target.ArrayProperty = arrProperty;
                }
            }

            if (target.ExpectedType == null)
            {
                (string typeError, Type expectType, Type expectInterface) = GetExpectedTypeOfProp(property, info);
                // Debug.Log($"Update Shared Cache expectType={expectType}/{property.propertyPath}/{property.isArray}");
                if (typeError != "")
                {
                    // Debug.Log(typeError);
                    target.Error = typeError;
                    return;
                }

                target.ExpectedType = expectType;
                target.ExpectedInterface = expectInterface;
            }
        }

        private static void UpdateSharedCacheSource(GetByXPathGenericCache target, SerializedProperty property, FieldInfo info)
        {
            // Debug.Log(property.propertyPath);
            // target.ImGuiResourcesLastTime = EditorApplication.timeSinceStartup;
            GetXPathValuesResult iterResults = GetXPathValues(
                target.GetByXPathAttributes
                    .Select(xPathAttribute => new XPathResourceInfo
                    {
                        OptimizationPayload = xPathAttribute.OptimizationPayload,
                        OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                    })
                    .ToArray(),
                target.ExpectedType,
                target.ExpectedInterface,
                property,
                info,
                target.Parent);

            bool isArray = target.ArrayProperty != null;

            target.CachedResults
                = isArray
                    ? iterResults.Results.ToArray()
                    : new[] { iterResults.Results.FirstOrDefault() };
        }

        private struct ProcessPropertyInfo
        {
            public SerializedProperty Property;
            public object Value;
            public PropertyCache PropertyCache;
        }

        private static void UpdateSharedCacheSetValue(GetByXPathGenericCache target, bool isFirstTime, SerializedProperty property)
        {
            if (target.Error != "")
            {
                return;
            }

            IReadOnlyList<object> expandedResults = target.CachedResults;
            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;

            bool nothingSigner = NothingSigner(target.GetByXPathAttributes[0]);

            bool forceReOrder = target.GetByXPathAttributes[0].ForceReOrder;

            if(!nothingSigner && isArray && target.ArrayProperty.arraySize != expandedResults.Count)
            {
                if(!forceReOrder)
                {
                    if (expandedResults.Count <
                        target.ArrayProperty
                            .arraySize) // reducing the size, let's check if we need to shift the array first (to erase null values)
                    {
                        int accValueIndex = 0;
                        for (int arrayIndex = 0; arrayIndex < target.ArrayProperty.arraySize; arrayIndex++)
                        {
                            SerializedProperty element = target.ArrayProperty.GetArrayElementAtIndex(arrayIndex);
                            if (element.propertyType != SerializedPropertyType.ObjectReference)
                            {
                                break;
                            }

                            Object elementValue = element.objectReferenceValue;
                            if (RuntimeUtil.IsNull(elementValue))
                            {

                            }
                            else
                            {
                                if (arrayIndex != accValueIndex)
                                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                                Debug.Log($"#GetByXPath# shift array element {arrayIndex} to {accValueIndex}");
#endif

                                    target.ArrayProperty.MoveArrayElement(arrayIndex, accValueIndex);
                                }

                                accValueIndex += 1;
                            }
                        }
                    }
                }
                target.ArrayProperty.arraySize = expandedResults.Count;
                EnqueueSceneViewNotification($"Adjust array {target.ArrayProperty.displayName} to length {target.ArrayProperty.arraySize}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Raw: Adjust array {target.ArrayProperty.displayName} to length {target.ArrayProperty.arraySize}");
#endif
                target.ArrayProperty.serializedObject.ApplyModifiedProperties();
            }

            GetByXPathAttribute getByXPathAttribute = target.GetByXPathAttributes[0];

            // Debug.Log($"expandedResults count = {expandedResults.Count}");

            // property.serializedObject.Update();
            List<object> processingTargets = new List<object>();
            List<ProcessPropertyInfo> processingProperties = new List<ProcessPropertyInfo>();

            foreach ((object targetResult, int index) in expandedResults.WithIndex())
            {
                SerializedProperty processingProperty;
                if (isArray)
                {
                    if(index >= target.ArrayProperty.arraySize)
                    {
                        break;  // let the array target to deal with this
                    }
                    processingProperty = target.ArrayProperty.GetArrayElementAtIndex(index);
                }
                else
                {
                    processingProperty = property;
                }
                int propertyCacheKey = isArray
                    ? index
                    : -1;

                (SerializedUtils.FieldOrProp fieldOrProp, object fieldParent) = SerializedUtils.GetFieldInfoAndDirectParent(processingProperty);
                // Debug.Log(propertyCacheKey);
                PropertyCache propertyCache = target.IndexToPropertyCache[propertyCacheKey] = new PropertyCache
                {
                    Error = "",
                    // ReSharper disable once RedundantCast
                    MemberInfo = fieldOrProp.IsField? (MemberInfo)fieldOrProp.FieldInfo: fieldOrProp.PropertyInfo,
                    Parent = fieldParent,
                    SerializedProperty = processingProperty,
                };

                if (nothingSigner)
                {
                    continue;
                }

                // Debug.Log($"#GetByXPath# IndexToPropertyCache[{propertyCacheKey}] = {propertyCache}");

                (string originalValueError, int _, object originalValue) = Util.GetValue(processingProperty, propertyCache.MemberInfo, propertyCache.Parent);
                if (originalValueError != "")
                {
                    propertyCache.Error = originalValueError;
                    return;
                }

                if (originalValue is IWrapProp wrapProp)
                {
                    originalValue = Util.GetWrapValue(wrapProp);
                }

                processingTargets.Add(targetResult);
                processingProperties.Add(new ProcessPropertyInfo
                {
                    Property = processingProperty,
                    Value = originalValue,
                    PropertyCache = propertyCache,
                });
            }

            if (nothingSigner)
            {
                return;
            }

            if(!forceReOrder)
            {
                foreach ((object processingTarget, int processingTargetIndex) in processingTargets.WithIndex().Reverse()
                             .ToArray())
                {
                    int processingPropertyMatchedIndex =
                        processingProperties.FindIndex(each => each.Value == processingTarget);
                    if (processingPropertyMatchedIndex >= 0)
                    {
                        processingTargets.RemoveAt(processingTargetIndex);
                        processingProperties.RemoveAt(processingPropertyMatchedIndex);
                    }
                }
            }

            // now check mismatched
            foreach ((object targetResult, ProcessPropertyInfo propertyInfo) in processingTargets.Zip(processingProperties, (targetResult, propertyInfo) => (targetResult, propertyInfo)))
            {
                propertyInfo.PropertyCache.OriginalValue = propertyInfo.Value;
                bool fieldIsNull = RuntimeUtil.IsNull(propertyInfo.Value);
                propertyInfo.PropertyCache.TargetValue = targetResult;
                bool targetIsNull = RuntimeUtil.IsNull(targetResult);
                propertyInfo.PropertyCache.TargetIsNull = targetIsNull;

                propertyInfo.PropertyCache.MisMatch = Mismatch(propertyInfo.Value, targetResult);

                // Debug.Log($"#GetByXPath# o={originalValue}({processingProperty.propertyPath}), t={targetResult}, mismatch={propertyCache.MisMatch}");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# mismatch={propertyInfo.PropertyCache.MisMatch}, {propertyInfo.PropertyCache.OriginalValue} -> {targetResult}: {propertyInfo.PropertyCache.SerializedProperty.propertyPath}");
#endif

                if(propertyInfo.PropertyCache.MisMatch)
                {
                    bool resign = getByXPathAttribute.AutoResignToValue && !targetIsNull;
                    if (!resign)
                    {
                        resign = getByXPathAttribute.AutoResignToNull && targetIsNull;
                    }
                    if (!resign)
                    {
                        resign = isFirstTime && getByXPathAttribute.InitSign && fieldIsNull && !targetIsNull;
                    }

                    if (resign)
                    {
                        if (DoSignPropertyCache(propertyInfo.PropertyCache, true))
                        {
                            propertyInfo.PropertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                        }

                    }
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log("Done");
#endif
            }
        }

//         private static void UpdateSharedCache(GetByXPathGenericCache target, bool isFirstTime, SerializedProperty property, FieldInfo info, bool isImGui)
//         {
//             target.UpdatedLastTime = EditorApplication.timeSinceStartup;
//
//             UpdateSharedCacheBase(target, isFirstTime, property, info, isImGui);
//
//             if (target.Error != "")
//             {
//                 return;
//             }
//
//             bool refreshResources = true;
//             if (isImGui)
//             {
//                 refreshResources = EditorApplication.timeSinceStartup - target.UpdatedLastTime > SaintsFieldConfigUtil.GetByXPathLoopIntervalMsIMGUI() / 1000f;
//                 // ReSharper disable once ConvertIfToOrExpression
//                 if (!refreshResources && target.CachedResults == null)
//                 {
//                     refreshResources = true;
//                 }
//             }
//
//             bool nothingSigner = NothingSigner(target.GetByXPathAttributes[0]);
//
//             if(refreshResources)
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
//                 Debug.Log($"#GetByXPath# refresh resources for {property.propertyPath}");
// #endif
//                 if (!nothingSigner)
//                 {
//                     target.ImGuiResourcesLastTime = EditorApplication.timeSinceStartup;
//                     UpdateSharedCacheSource(target, isFirstTime, property, info, isImGui);
//                 }
//             }
//
//             UpdateSharedCacheSetValue(target, isFirstTime, property, info, isImGui);
//         }

        // no longer process with element remove, because we'll always adjust array size to correct size.
        private static bool DoSignPropertyCache(PropertyCache propertyCache, bool notice)
        {
            try
            {
                string _ = propertyCache.SerializedProperty.propertyPath;
            }
#pragma warning disable CS0168
            catch (NullReferenceException e)
#pragma warning restore CS0168
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif
                return false;
            }
#pragma warning disable CS0168
            catch (ObjectDisposedException e)
#pragma warning restore CS0168
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif
                return false;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# Sign {propertyCache.SerializedProperty.propertyPath} from {propertyCache.OriginalValue} to {propertyCache.TargetValue}");
#endif
            propertyCache.MisMatch = false;

            if(notice)
            {
                EnqueueSceneViewNotification(
                    $"Auto sign {(propertyCache.TargetIsNull ? "null" : propertyCache.TargetValue)} to {propertyCache.SerializedProperty.displayName}");
            }

            // Debug.Log(propertyCache.SerializedProperty.propertyPath);
            // Debug.Log(propertyCache.TargetValue);

            ReflectUtils.SetValue(
                propertyCache.SerializedProperty.propertyPath,
                propertyCache.SerializedProperty.serializedObject.targetObject,
                propertyCache.MemberInfo,
                propertyCache.Parent,
                propertyCache.TargetValue);
            Util.SignPropertyValue(propertyCache.SerializedProperty, propertyCache.MemberInfo, propertyCache.Parent, propertyCache.TargetValue);

            // check prefab instance
            GameObject inspectingGo = null;
            if (propertyCache.SerializedProperty.serializedObject.targetObject is GameObject go)
            {
                inspectingGo = go;
            }
            else if (propertyCache.SerializedProperty.serializedObject.targetObject is Component comp)
            {
                inspectingGo = comp.gameObject;
            }

            if (!(inspectingGo is null))
            {
                Object prefabHandle = PrefabUtility.GetPrefabInstanceHandle(inspectingGo);
                // Debug.Log($"prefabHandle={prefabHandle}");
                if (prefabHandle != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(prefabHandle);
                }
            }

            return true;
        }

        public static bool HelperGetArraySize(SerializedProperty arrayProperty, FieldInfo info, bool isImGui)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }

            if (arrayProperty.arraySize > 0)
            {
                return isImGui;
            }

            string key = arrayProperty.propertyPath;

            GetByXPathGenericCache target = new GetByXPathGenericCache
            {
                // ImGuiRenderCount = 1,
                Error = "",
                // GetByXPathAttributes = attributes,
                ArrayProperty = arrayProperty,
            };

            if (SharedCache.TryGetValue(key, out GetByXPathGenericCache exists))
            {
                target = exists;
            }

            (GetByXPathAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(arrayProperty);
            target.GetByXPathAttributes = attributes;

            if(NothingSigner(target.GetByXPathAttributes[0]))
            {
                return false;
            }

            (string typeError, Type expectType, Type expectInterface) = GetExpectedTypeOfProp(arrayProperty, info);

            // Debug.Log($"array expectType={expectType}");

            if (typeError != "")
            {
                return false;
            }

            target.ExpectedType = expectType;
            target.ExpectedInterface = expectInterface;

            // bool refreshResources = true;
            // if (isImGui)
            // {
            //     refreshResources = EditorApplication.timeSinceStartup - target.ImGuiResourcesLastTime > SaintsFieldConfigUtil.GetByXPathLoopIntervalMsIMGUI() / 1000f;
            // }

            IReadOnlyList<object> expandedResults;
            // if(true)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# refresh resources for {arrayProperty.propertyPath}");
#endif

                GetXPathValuesResult iterResults = GetXPathValues(
                    target.GetByXPathAttributes
                        .Select(xPathAttribute => new XPathResourceInfo
                        {
                            OptimizationPayload = xPathAttribute.OptimizationPayload,
                            OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                        })
                        .ToArray(),
                    target.ExpectedType,
                    target.ExpectedInterface,
                    arrayProperty,
                    info,
                    parent);

                expandedResults = iterResults.Results.ToArray();
                target.CachedResults = expandedResults;
                // target.ImGuiResourcesLastTime = EditorApplication.timeSinceStartup;
            }
            // else
            // {
            //     expandedResults = target.CachedResults;
            //     Debug.Assert(expandedResults != null);
            // }

            if (expandedResults.Count == 0)
            {
                return true;
            }

            arrayProperty.arraySize = expandedResults.Count;
            EnqueueSceneViewNotification($"Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# Helper: Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#endif
            arrayProperty.serializedObject.ApplyModifiedProperties();

            GetByXPathAttribute getByXPathAttribute = target.GetByXPathAttributes[0];

            if(getByXPathAttribute.InitSign)
            {
                foreach ((object targetResult, int propertyCacheKey) in expandedResults.WithIndex())
                {
                    SerializedProperty processingProperty;
                    try
                    {
                        processingProperty =
                            target.ArrayProperty.GetArrayElementAtIndex(propertyCacheKey);
                    }
#pragma warning disable CS0168
                    catch (NullReferenceException e)
#pragma warning restore CS0168
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogException(e);
#endif

                        return false;
                    }

                    (SerializedUtils.FieldOrProp fieldOrProp, object fieldParent) =
                        SerializedUtils.GetFieldInfoAndDirectParent(processingProperty);
                    PropertyCache propertyCache
                        = target.IndexToPropertyCache[propertyCacheKey]
                            = new PropertyCache
                            {
                                // ReSharper disable once RedundantCast
                                MemberInfo = fieldOrProp.IsField ? (MemberInfo)fieldOrProp.FieldInfo : fieldOrProp.PropertyInfo,
                                Parent = fieldParent,
                                SerializedProperty = processingProperty,
                            };

                    propertyCache.OriginalValue = null;
                    propertyCache.TargetValue = targetResult;
                    bool targetIsNull = RuntimeUtil.IsNull(targetResult);
                    propertyCache.TargetIsNull = targetIsNull;

                    propertyCache.MisMatch = !targetIsNull;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                    Debug.Log($"#GetByXPath# Helper: Sign {propertyCache.SerializedProperty.propertyPath} from {propertyCache.OriginalValue} to {propertyCache.TargetValue}");
#endif
                    if (DoSignPropertyCache(propertyCache, true))
                    {
                        propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Helper: Apply changes to {arrayProperty.serializedObject.targetObject}");
#endif

                arrayProperty.serializedObject.ApplyModifiedProperties();
            }


            SharedCache[key] = target;

            return isImGui;
        }

        private static string GetMismatchErrorMessage(object originalValue, object targetValue, bool targetValueIsNull)
        {
            return $"Expected {(targetValueIsNull? "null": targetValue)}, but got {(RuntimeUtil.IsNull(originalValue)? "null": originalValue)}";
        }

        private static bool NothingSigner(GetByXPathAttribute getByXPathAttribute)
        {
            return !getByXPathAttribute.AutoResignToValue && !getByXPathAttribute.AutoResignToNull && !getByXPathAttribute.InitSign
                && !getByXPathAttribute.UseResignButton && !getByXPathAttribute.UseErrorMessage;
        }
    }
}
