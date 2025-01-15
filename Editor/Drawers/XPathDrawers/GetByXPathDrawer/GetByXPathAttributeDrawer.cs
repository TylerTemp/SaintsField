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
    // Note about this drawer in UI Toolkit:
    // if the list/array is not drawn by a ListView, the 1st item the drawer will fail to deliver the correct message to the rest of the items
    // and if the list is drawn by a custom drawer, then the 1st item will not even get drawn
    // the correct way is to think is that there is no way to correctly store the cached data -> use a static
    // it's not granted that the 1st element will get drawn -> use any drawn element
    // because some config just update the data, some config will only update the UI, and some both -> the responsible drawer should only update the data, and any drawn element just fetch the cached data to update it's own UI accordingly

    // ATM: we just think the 1st element will always be drawn, and there is always a listView parent
    // and use the _selfChange to prevent loop calling on element dragging
    [CustomPropertyDrawer(typeof(GetByXPathAttribute))]
#if !SAINTSFIELD_OLD_GETTER
    [CustomPropertyDrawer(typeof(GetComponentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentsAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute))]
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute))]
    [CustomPropertyDrawer(typeof(FindComponentAttribute))]
#endif
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
            Type rawType = ReflectUtils.GetElementType(info is FieldInfo fi? fi.FieldType: ((PropertyInfo) info).PropertyType);
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

        private static void UpdateSharedCache(GetByXPathGenericCache target, bool isFirstTime, SerializedProperty property, FieldInfo info, bool isImGui)
        {
            target.UpdatedLastTime = EditorApplication.timeSinceStartup;

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

            // if (NothingSigner(target.GetByXPathAttributes[0]))
            // {
            //     return;
            // }

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
                        return;
                    }

                    target.ArrayProperty = arrProperty;
                }
            }

            if (target.ExpectedType == null)
            {
                (string typeError, Type expectType, Type expectInterface) = GetExpectedTypeOfProp(property, info);
                if (typeError != "")
                {
                    // Debug.Log(typeError);
                    target.Error = typeError;
                    return;
                }

                target.ExpectedType = expectType;
                target.ExpectedInterface = expectInterface;
            }

            bool refreshResources = true;
            if (isImGui)
            {
                refreshResources = EditorApplication.timeSinceStartup - target.UpdatedLastTime > SaintsFieldConfigUtil.GetByXPathLoopIntervalMsIMGUI() / 1000f;
                // ReSharper disable once ConvertIfToOrExpression
                if (!refreshResources && target.CachedResults == null)
                {
                    refreshResources = true;
                }
            }

            bool nothingSigner = NothingSigner(target.GetByXPathAttributes[0]);

            IReadOnlyList<object> expandedResults;
            if(refreshResources)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# refresh resources for {property.propertyPath}");
#endif
                if (nothingSigner)
                {
                    expandedResults
                        = target.CachedResults
                            = isArray ? new object[] { } : new object[] { null };
                }
                else
                {
                    target.ImGuiResourcesLastTime = EditorApplication.timeSinceStartup;
                    GetXPathValuesResult iterResults = GetXPathValues(
                        target.GetByXPathAttributes.SelectMany(each => each.XPathInfoAndList).ToArray(),
                        target.ExpectedType,
                        target.ExpectedInterface,
                        property,
                        info,
                        target.Parent);

                    expandedResults
                        = target.CachedResults
                            = isArray
                                ? iterResults.Results.ToArray()
                                : new[] { iterResults.Results.FirstOrDefault() };
                }
            }
            else
            {
                expandedResults = target.CachedResults;
            }


            if(!nothingSigner && isArray && target.ArrayProperty.arraySize != expandedResults.Count)
            {
                target.ArrayProperty.arraySize = expandedResults.Count;
                EnqueueSceneViewNotification($"Adjust array {target.ArrayProperty.displayName} to length {target.ArrayProperty.arraySize}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Raw: Adjust array {target.ArrayProperty.displayName} to length {target.ArrayProperty.arraySize}");
#endif
                target.ArrayProperty.serializedObject.ApplyModifiedProperties();
            }

            GetByXPathAttribute getByXPathAttribute = target.GetByXPathAttributes[0];

            // Debug.Log($"expandedResults count = {expandedResults.Count}");

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
                // if(!target.IndexToPropertyCache.TryGetValue(propertyCacheKey, out PropertyCache propertyCache))
                // {
                //     (SerializedUtils.FieldOrProp fieldOrProp, object fieldParent) = SerializedUtils.GetFieldInfoAndDirectParent(processingProperty);
                //     propertyCache = target.IndexToPropertyCache[propertyCacheKey] = new PropertyCache
                //     {
                //         MemberInfo = fieldOrProp.IsField? fieldOrProp.FieldInfo: fieldOrProp.PropertyInfo,
                //         Parent = fieldParent,
                //         SerializedProperty = processingProperty,
                //     };
                // }
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

                // propertyCache.SerializedProperty = processingProperty;

                (string originalValueError, int _, object originalValue) = Util.GetValue(processingProperty, propertyCache.MemberInfo, propertyCache.Parent);
                if (originalValueError != "")
                {
                    propertyCache.Error = originalValueError;
                    return;
                }

                propertyCache.OriginalValue = originalValue;
                bool fieldIsNull = Util.IsNull(originalValue);
                propertyCache.TargetValue = targetResult;
                bool targetIsNull = Util.IsNull(targetResult);
                propertyCache.TargetIsNull = targetIsNull;

                propertyCache.MisMatch = Mismatch(originalValue, targetResult);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# mismatch={propertyCache.MisMatch}, {originalValue}, {targetResult}: {propertyCache.SerializedProperty.propertyPath}");
                // Debug.Log(property.objectReferenceValue);
                // Debug.Log(Event.current);
#endif

                if(propertyCache.MisMatch)
                {
                    bool resign = getByXPathAttribute.AutoResignToValue && !targetIsNull;
                    if (!resign)
                    {
                        resign = getByXPathAttribute.AutoResignToNull && targetIsNull;
                    }
                    if (!resign)
                    {
                        resign = isFirstTime && getByXPathAttribute.InitSign && fieldIsNull;
                    }

                    if (resign)
                    {
                        DoSignPropertyCache(propertyCache, true);
                    }
                }
            }
        }

        // no longer process with element remove, because we'll always adjust array size to correct size.
        private static void DoSignPropertyCache(PropertyCache propertyCache, bool notice)
        {
            try
            {
                string _ = propertyCache.SerializedProperty.propertyPath;
            }
            catch (NullReferenceException e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif
                return;
            }
            catch (ObjectDisposedException e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif
                return;
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

            ReflectUtils.SetValue(
                propertyCache.SerializedProperty.propertyPath,
                propertyCache.SerializedProperty.serializedObject.targetObject,
                propertyCache.MemberInfo,
                propertyCache.Parent,
                propertyCache.TargetValue);
            Util.SignPropertyValue(propertyCache.SerializedProperty, propertyCache.MemberInfo, propertyCache.Parent, propertyCache.TargetValue);
        }

        public static bool HelperGetArraySize(SerializedProperty arrayProperty, FieldInfo info, bool isImGui)
        {
            if (EditorApplication.isPlaying)
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
                ImGuiRenderCount = 1,
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
            if (typeError != "")
            {
                return false;
            }

            target.ExpectedType = expectType;
            target.ExpectedInterface = expectInterface;

            bool refreshResources = true;
            if (isImGui)
            {
                refreshResources = EditorApplication.timeSinceStartup - target.ImGuiResourcesLastTime > SaintsFieldConfigUtil.GetByXPathLoopIntervalMsIMGUI() / 1000f;
            }

            IReadOnlyList<object> expandedResults;
            if(refreshResources)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# refresh resources for {arrayProperty.propertyPath}");
#endif

                GetXPathValuesResult iterResults = GetXPathValues(
                    target.GetByXPathAttributes.SelectMany(each => each.XPathInfoAndList).ToArray(),
                    target.ExpectedType,
                    target.ExpectedInterface,
                    arrayProperty,
                    null,
                    parent);

                expandedResults = iterResults.Results.ToArray();
                target.CachedResults = expandedResults;
                target.ImGuiResourcesLastTime = EditorApplication.timeSinceStartup;
            }
            else
            {
                expandedResults = target.CachedResults;
                Debug.Assert(expandedResults != null);
            }

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
                    catch (NullReferenceException e)
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
                                MemberInfo = fieldOrProp.IsField ? (MemberInfo)fieldOrProp.FieldInfo : fieldOrProp.PropertyInfo,
                                Parent = fieldParent,
                                SerializedProperty = processingProperty,
                            };

                    propertyCache.OriginalValue = null;
                    propertyCache.TargetValue = targetResult;
                    bool targetIsNull = Util.IsNull(targetResult);
                    propertyCache.TargetIsNull = targetIsNull;

                    propertyCache.MisMatch = !targetIsNull;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                    Debug.Log($"#GetByXPath# Helper: Sign {propertyCache.SerializedProperty.propertyPath} from {propertyCache.OriginalValue} to {propertyCache.TargetValue}");
#endif
                    DoSignPropertyCache(propertyCache, true);
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
            return $"Expected {(targetValueIsNull? "null": targetValue)}, but got {(Util.IsNull(originalValue)? "null": originalValue)}";
        }

        private static bool NothingSigner(GetByXPathAttribute getByXPathAttribute)
        {
            return !getByXPathAttribute.AutoResignToValue && !getByXPathAttribute.AutoResignToNull && !getByXPathAttribute.InitSign
                && !getByXPathAttribute.UseResignButton && !getByXPathAttribute.UseErrorMessage;
        }
    }
}
