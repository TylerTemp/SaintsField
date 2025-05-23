using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer: IAutoRunnerFixDrawer
    {
        // this will give array and array items; it needs to be processed separately
        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            // ReSharper disable once UseNegatedPatternInIsExpression
            if(!(propertyAttribute is GetByXPathAttribute getByXPathAttribute))
            {
                return null;
            }

            GetByXPathAttribute[] getByXPathAttributes = allAttributes.OfType<GetByXPathAttribute>().ToArray();

            if (!ReferenceEquals(getByXPathAttribute, getByXPathAttributes[0]))
            {
                return null;
            }

            if(NothingSigner(getByXPathAttribute))
            {
                return null;
            }

            (string typeError, Type expectType, Type expectInterface) = GetExpectedTypeOfProp(property, memberInfo);
            if (typeError != "")
            {
                return new AutoRunnerFixerResult
                {
                    Error = "",
                    ExecError = typeError,
                };
            }

            GetByXPathGenericCache target = new GetByXPathGenericCache
            {
                // ImGuiRenderCount = 1,
                Error = "",
                ExpectedType = expectType,
                ExpectedInterface = expectInterface,
            };

            GetXPathValuesResult iterResults = GetXPathValues(
                getByXPathAttributes
                    .Select(xPathAttribute => new XPathResourceInfo
                    {
                        OptimizationPayload = xPathAttribute.OptimizationPayload,
                        OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                    })
                    .ToArray(),
                target.ExpectedType,
                target.ExpectedInterface,
                property,
                memberInfo,
                parent);
            if(iterResults.XPathError != "")
            {
                return new AutoRunnerFixerResult
                {
                    Error = "",
                    ExecError = iterResults.XPathError,
                };
            }

            object[] expandedResults = iterResults.Results.ToArray();
            // Debug.Log($"#AUTO# {string.Join(",", expandedResults)}");
            // Debug.Log($"#AUTO# {string.Join(",", getByXPathAttribute.XPathInfoAndList[0])}");
            // Selection.activeObject = property.serializedObject.targetObject;
            int resultsLength = expandedResults.Length;

            // Debug.Log(property.propertyPath);
            if (property.isArray)
            {
                if (property.arraySize == resultsLength)
                {
                    return null;
                }

                return new AutoRunnerFixerResult
                {
                    ExecError = "",
                    Error =
                        $"{property.displayName}({property.propertyPath}): Array size expected {expandedResults.Length}, got {property.arraySize}\nYou need to re-run the check after this is fixed",
                    CanFix = true,
                    Callback = () =>
                    {
                        property.arraySize = resultsLength;
                        property.serializedObject.ApplyModifiedProperties();
                    },
                };
            }

            // this is actually: isArrayItem
            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;
            if (isArray)
            {
                (SerializedProperty arrProperty, int _, string arrError) =
                    Util.GetArrayProperty(property, memberInfo, parent);
                if (arrError != "")
                {
                    return new AutoRunnerFixerResult
                    {
                        Error = "",
                        ExecError = arrError,
                    };
                }

                target.ArrayProperty = arrProperty;
            }

            object[] useResult = isArray ? expandedResults : new[] { expandedResults.FirstOrDefault() };

            bool forceReOrder = getByXPathAttributes[0].ForceReOrder;

            List<object> processingTargets = new List<object>();
            List<ProcessPropertyInfo> processingProperties = new List<ProcessPropertyInfo>();

            // not directly array target
            foreach ((object targetResult, int index) in useResult.WithIndex())
            {
                SerializedProperty processingProperty;
                if (isArray)
                {
                    if (index >= target.ArrayProperty.arraySize)
                    {
                        return null; // let the array target to deal with this
                    }

                    processingProperty = target.ArrayProperty.GetArrayElementAtIndex(index);
                }
                else
                {
                    processingProperty = property;
                }

                (SerializedUtils.FieldOrProp fieldOrProp, object fieldParent) =
                    SerializedUtils.GetFieldInfoAndDirectParent(processingProperty);

                PropertyCache propertyCache = new PropertyCache
                {
                    Error = "",
                    // ReSharper disable once RedundantCast
                    MemberInfo = fieldOrProp.IsField ? (MemberInfo)fieldOrProp.FieldInfo : fieldOrProp.PropertyInfo,
                    Parent = fieldParent,
                    SerializedProperty = processingProperty,
                };

                // propertyCache.SerializedProperty = processingProperty;

                (string originalValueError, object originalValue) = GetCurValue(processingProperty,
                    propertyCache.MemberInfo, propertyCache.Parent);
                if (originalValueError != "")
                {
                    return new AutoRunnerFixerResult
                    {
                        Error = "",
                        ExecError = originalValueError,
                    };
                }

                processingTargets.Add(targetResult);
                processingProperties.Add(new ProcessPropertyInfo
                (
                    originalValue,
                    propertyCache
                ));
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
            foreach ((object targetResult, ProcessPropertyInfo propertyInfo) in processingTargets.Zip(processingProperties, (targetResult, propertyInfo) => (targetResult, propertyInfo)))
            {
                propertyInfo.PropertyCache.OriginalValue = propertyInfo.Value;
                bool fieldIsNull = RuntimeUtil.IsNull(propertyInfo.Value);
                propertyInfo.PropertyCache.TargetValue = targetResult;
                bool targetIsNull = RuntimeUtil.IsNull(targetResult);
                propertyInfo.PropertyCache.TargetIsNull = targetIsNull;

                propertyInfo.PropertyCache.MisMatch = Mismatch(propertyInfo.Value, targetResult);


                if(propertyInfo.PropertyCache.MisMatch)
                {
                    bool resign = getByXPathAttribute.AutoResignToValue && !targetIsNull;
                    if (!resign)
                    {
                        resign = getByXPathAttribute.AutoResignToNull && targetIsNull;
                    }
                    if (!resign)
                    {
                        resign = getByXPathAttribute.InitSign && fieldIsNull;
                    }

                    if (resign)
                    {
                        return new AutoRunnerFixerResult
                        {
                            Error = $"{property.displayName}({property.propertyPath}): Value mismatch {propertyInfo.Value} -> {targetResult}",
                            ExecError = "",
                            CanFix = true,
                            Callback = () =>
                            {
                                if (DoSignPropertyCache(propertyInfo.PropertyCache, true))
                                {
                                    propertyInfo.PropertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                                }
                            },
                        };
                    }
                }
            }

            return null;
        }
    }
}
