#if (WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AK.Wwise;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace SaintsField.Editor.Drawers.Wwise.GetWwiseDrawer
{
    public static class GetWwiseAttributeDrawerHelper
    {
        public static WwiseObjectType GetWwiseObjectType(Type fieldType)
        {
            if (typeof(AuxBus).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.AuxBus;
            }

            if (typeof(Event).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.Event;
            }

            if (typeof(Bank).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.Soundbank;
            }

            if (typeof(State).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.State;
            }

            if (typeof(Switch).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.Switch;
            }

            if (typeof(RTPC).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.GameParameter;
            }

            if (typeof(Trigger).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.Trigger;
            }

            if (typeof(AcousticTexture).IsAssignableFrom(fieldType))
            {
                return WwiseObjectType.AcousticTexture;
            }
#if SAINTSFIELD_DEBUG
            Debug.LogWarning($"Unsupported wwise type {fieldType}");
#endif
            return WwiseObjectType.None;
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

            GetByXPathAttributeDrawer.GetByXPathGenericCache target = new GetByXPathAttributeDrawer.GetByXPathGenericCache
            {
                // ImGuiRenderCount = 1,
                Error = "",
                // GetByXPathAttributes = attributes,
                ArrayProperty = arrayProperty,
            };

            if (GetByXPathAttributeDrawer.SharedCache.TryGetValue(key, out GetByXPathAttributeDrawer.GetByXPathGenericCache exists))
            {
                target = exists;
            }

            (GetByXPathAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(arrayProperty);
            target.GetByXPathAttributes = attributes;

            if(GetByXPathAttributeDrawer.NothingSigner(target.GetByXPathAttributes[0]))
            {
                return false;
            }

            (string typeError, Type expectType, Type expectInterface) = GetByXPathAttributeDrawer.GetExpectedTypeOfProp(arrayProperty, info);

            // Debug.Log($"array expectType={expectType}");

            if (typeError != "")
            {
                return false;
            }

            target.ExpectedType = expectType;
            target.ExpectedInterface = expectInterface;

            IReadOnlyList<object> expandedResults;
            // if(true)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# refresh resources for {arrayProperty.propertyPath}");
#endif
                Type rawType = ReflectUtils.GetElementType(info.FieldType);

                GetByXPathAttributeDrawer.GetXPathValuesResult iterResults = GetWwiseAttributeDrawer.CalcXPathValues(
                    GetWwiseObjectType(rawType),
                    target.GetByXPathAttributes
                        .Select(xPathAttribute => new GetByXPathAttributeDrawer.XPathResourceInfo
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
            }

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
                SaintsPropertyDrawer.EnqueueSceneViewNotification(
                    $"Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Helper: Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#endif
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }

            if(getByXPathAttribute.InitSign)
            {
                foreach ((object targetResult, int propertyCacheKey) in expandedResults.WithIndex())
                {
                    if(propertyCacheKey >= target.ArrayProperty.arraySize)
                    {
                        continue;
                    }

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
                    GetByXPathAttributeDrawer.PropertyCache propertyCache
                        = target.IndexToPropertyCache[propertyCacheKey]
                            = new GetByXPathAttributeDrawer.PropertyCache
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

                    bool canSign = GetByXPathAttributeDrawer.HelperPreDoSignPropertyCache(propertyCache, true);

                    // ReSharper disable once InvertIf
                    if (canSign)
                    {
                        GetWwiseAttributeDrawer.HelperDoSignPropertyCache(propertyCache);
                        GetByXPathAttributeDrawer.HelperPostDoSignPropertyCache(propertyCache);
                        propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Helper: Apply changes to {arrayProperty.serializedObject.targetObject}");
#endif
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }


            GetByXPathAttributeDrawer.SharedCache[key] = target;

            return isImGui;
        }
    }
}

#endif
