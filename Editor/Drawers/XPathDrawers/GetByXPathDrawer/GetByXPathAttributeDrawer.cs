using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GetByXPathAttribute), true)]
    [CustomPropertyDrawer(typeof(GetComponentAttribute), true)]
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute), true)]
    [CustomPropertyDrawer(typeof(GetComponentInParentAttribute), true)]
    [CustomPropertyDrawer(typeof(GetComponentInParentsAttribute), true)]
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute), true)]
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute), true)]
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute), true)]
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute), true)]
    [CustomPropertyDrawer(typeof(FindComponentAttribute), true)]
    public partial class GetByXPathAttributeDrawer: SaintsPropertyDrawer
    {
        private static (bool hasRoot, GameObject prefabRoot) GetPrefabRoot()
        {
#if UNITY_2021_3_OR_NEWER
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            // ReSharper disable once InvertIf
            if(prefabStage != null)
            {
                GameObject prefabRoot = prefabStage.prefabContentsRoot;
                if (prefabRoot != null)
                {
                    return (true, prefabRoot);
                }
            }
#endif
            return (false, null);
        }

        private static bool PrefabCanSignCheck(Object signToObj, object signFrom)
        {
            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(signToObj is Component signToComp))
            {
                return true;
            }

            GameObject signToGo = signToComp.gameObject;
            UnityEngine.SceneManagement.Scene signToScene = signToGo.scene;

            switch (signFrom)
            {
                case GameObject singFromGo:
                    if (!SceneCompare(singFromGo.scene, signToScene))
                    {
                        return false;
                    }

                    break;
                case Component signFromComp:
                    if (!SceneCompare(signFromComp.gameObject.scene, signToScene))
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        private static bool SceneCompare(UnityEngine.SceneManagement.Scene signFrom, UnityEngine.SceneManagement.Scene signTo)
        {
            if (signFrom.IsValid())   // `from` is a scene object
            {
                return signFrom == signTo;  // `to` must also be a scene object
            }

            // `from` is an asset
            return true;  // `to` can be anything
        }

        private static (bool valid, object value) ValidateXPathResult(Object fieldContainingObject, object each, Type expectType, Type expectInterface)
        {
            if(!PrefabCanSignCheck(fieldContainingObject, each))
            {
                return (false, null);
            }

            object result;
            // Debug.Log($"{each}");
            if (expectType.IsInstanceOfType(each))
            {
                result = each;
            }
            else if (each is Object uObject)
            {
                Object r = Util.GetTypeFromObj(uObject, expectType);

                if (!r)
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
                    if (!component)  // some broken component
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

        public static (string error, Type expectType, Type expectInterface) GetExpectedTypeOfProp(
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

        private void UpdateSharedCacheSource(GetByXPathGenericCache target, SerializedProperty property, FieldInfo info)
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

        private readonly struct ProcessPropertyInfo
        {
            public readonly object Value;
            public readonly PropertyCache PropertyCache;

            public ProcessPropertyInfo(object value, PropertyCache propertyCache)
            {
                Value = value;
                PropertyCache = propertyCache;
            }
        }

        private void UpdateSharedCacheSetValue(GetByXPathGenericCache target, bool isFirstTime, SerializedProperty property)
        {
            if (target.Error != "")
            {
                return;
            }

            IReadOnlyList<object> expandedResults = target.CachedResults;
            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;


            GetByXPathAttribute getByXPathAttribute = target.GetByXPathAttributes[0];

            bool nothingSigner = NothingSigner(getByXPathAttribute);

            bool forceReOrder = getByXPathAttribute.ForceReOrder;


            if(!nothingSigner && isArray && target.ArrayProperty.arraySize != expandedResults.Count)
            {
                bool arrayShiftNeedApply = false;
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
                                    arrayShiftNeedApply = true;
                                }

                                accValueIndex += 1;
                            }
                        }
                    }
                }

                // TODO: add size for it because null ones still takes the places
                if (expandedResults.Count < target.ArrayProperty.arraySize && !getByXPathAttribute.AutoResignToNull)
                {

                }
                else
                {
                    target.ArrayProperty.arraySize = expandedResults.Count;
                    EnqueueSceneViewNotification(
                        $"Adjust array {target.ArrayProperty.displayName} to length {target.ArrayProperty.arraySize}");
                    arrayShiftNeedApply = true;
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Raw: Adjust array {target.ArrayProperty.displayName} to length {target.ArrayProperty.arraySize}");
#endif
                if(arrayShiftNeedApply)
                {
                    target.ArrayProperty.serializedObject.ApplyModifiedProperties();
                }
            }


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

                (string originalValueError, object originalValue) = GetCurValue(processingProperty, propertyCache.MemberInfo, propertyCache.Parent);
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
                (
                    originalValue,
                    propertyCache
                ));
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

                // Debug.Log($"mismatch {propertyInfo.PropertyCache.MisMatch}: {propertyInfo.Value} => {targetResult}");

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
                        // Debug.Log($"{resign}=getByXPathAttribute.AutoResignToNull={getByXPathAttribute.AutoResignToNull}/targetIsNull={targetIsNull}");
                    }
                    if (!resign)
                    {
                        resign = isFirstTime && getByXPathAttribute.InitSign && fieldIsNull && !targetIsNull;
                        // Debug.Log($"{resign}: isFirstTime={isFirstTime}&&getByXPathAttribute.InitSign={getByXPathAttribute.InitSign}&&fieldIsNull={fieldIsNull}&&!targetIsNull={!targetIsNull}");
                    }


                    // Debug.Log($"resign={resign}: {propertyInfo.PropertyCache.SerializedProperty.propertyPath}");
                    if (resign)
                    {
                        // Debug.Log($"start to sign {propertyInfo.PropertyCache.SerializedProperty.propertyPath}");
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

        protected virtual void ActualSignPropertyCache(PropertyCache propertyCache)
        {
            HelperDoSignPropertyCache(propertyCache);
        }

        public static bool HelperPreDoSignPropertyCache(PropertyCache propertyCache, bool notice)
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

            return true;
        }

        private static void HelperDoSignPropertyCache(PropertyCache propertyCache)
        {
            ReflectUtils.SetValue(
                propertyCache.SerializedProperty.propertyPath,
                propertyCache.SerializedProperty.serializedObject.targetObject,
                propertyCache.MemberInfo,
                propertyCache.Parent,
                propertyCache.TargetValue);
            Util.SignPropertyValue(propertyCache.SerializedProperty, propertyCache.MemberInfo, propertyCache.Parent, propertyCache.TargetValue);
        }

        public static void HelperPostDoSignPropertyCache(PropertyCache propertyCache)
        {
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

            // ReSharper disable once UseNegatedPatternInIsExpression
            // ReSharper disable once InvertIf
            if (!(inspectingGo is null))
            {
                Object prefabHandle = PrefabUtility.GetPrefabInstanceHandle(inspectingGo);
                // Debug.Log($"prefabHandle={prefabHandle}");
                if (prefabHandle)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(prefabHandle);
                }
            }
        }

        // no longer process with element remove, because we'll always adjust array size to correct size.
        private bool DoSignPropertyCache(PropertyCache propertyCache, bool notice)
        {
            if (!HelperPreDoSignPropertyCache(propertyCache, notice))
            {
                return false;
            }
            ActualSignPropertyCache(propertyCache);
            HelperPostDoSignPropertyCache(propertyCache);
            return true;
        }

        protected virtual (string error, object value) GetCurValue(SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            (string getValueError, int _, object curValue) = Util.GetValue(property, memberInfo, parent);
            if (getValueError == "")
            {
                if (curValue is IWrapProp wrapProp)
                {
                    object originalWrapValue = Util.GetWrapValue(wrapProp);
                    return ("", originalWrapValue);
                }
            }
            return (getValueError, curValue);
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

                GetXPathValuesResult iterResults = CalcXPathValues(
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

            GetByXPathAttribute getByXPathAttribute = target.GetByXPathAttributes[0];

            if (arrayProperty.arraySize != 0 && arrayProperty.arraySize < expandedResults.Count && !getByXPathAttribute.AutoResignToNull)
            {
            }
            else
            {
                arrayProperty.arraySize = expandedResults.Count;
                EnqueueSceneViewNotification(
                    $"Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
            }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# Helper: Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#endif
            arrayProperty.serializedObject.ApplyModifiedProperties();

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

                    bool canSign = HelperPreDoSignPropertyCache(propertyCache, true);

                    // ReSharper disable once InvertIf
                    if (canSign)
                    {
                        HelperDoSignPropertyCache(propertyCache);
                        HelperPostDoSignPropertyCache(propertyCache);
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

        public static bool NothingSigner(GetByXPathAttribute getByXPathAttribute)
        {
            return !getByXPathAttribute.AutoResignToValue && !getByXPathAttribute.AutoResignToNull && !getByXPathAttribute.InitSign
                && !getByXPathAttribute.UseResignButton && !getByXPathAttribute.UseErrorMessage;
        }
    }
}
