using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
#if (WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE
using SaintsField.Editor.Drawers.Wwise.GetWwiseDrawer;
#endif
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
#if (WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE
using SaintsField.Wwise;
#endif
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer: ISaintsRenderer
    {
        public bool InAnyHorizontalLayout { get; set; }
        public bool InDirectHorizontalLayout { get; set; }
        public bool NoLabel { get; set; }

        // ReSharper disable InconsistentNaming
        public readonly SaintsFieldWithInfo FieldWithInfo;
        // protected readonly SerializedObject SerializedObject;
        // ReSharper enable InconsistentNaming

        public struct PreCheckResult
        {
            public bool IsShown;
            public bool IsDisabled;
            // public int ArraySize;  // NOTE: -1=No Limit, 0=0, 1=More Than 0
            public (int min, int max) ArraySize;
            public bool HasRichLabel;
            public string RichLabelXml;
        }

        // ReSharper disable once UnusedParameter.Local
        protected AbsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            FieldWithInfo = fieldWithInfo;
            // SerializedObject = serializedObject;
        }

        protected static MemberInfo GetMemberInfo(SaintsFieldWithInfo info)
        {
            switch (info.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.NonSerializedField:
                    return info.FieldInfo;
                case SaintsRenderType.Method:
                    return info.MethodInfo;
                case SaintsRenderType.NativeProperty:
                    return info.PropertyInfo;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.RenderType), info.RenderType, null);
            }
        }

        protected PreCheckResult GetPreCheckResult(SaintsFieldWithInfo fieldWithInfo, bool isImGui)
        {
            List<ToggleCheckInfo> preCheckInternalInfos = new List<ToggleCheckInfo>(fieldWithInfo.PlayaAttributes.Count);
            (int, int) arraySize = (-1, -1);
            foreach (IPlayaAttribute playaAttribute in fieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case IVisibilityAttribute visibilityAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        (
                            visibilityAttribute.IsShow? ToggleType.Show: ToggleType.Hide,
                            visibilityAttribute.ConditionInfos,
                            fieldWithInfo.Targets[0]
                        ));
                        break;
                    case PlayaEnableIfAttribute enableIfAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Enable,
                            enableIfAttribute.ConditionInfos,
                            fieldWithInfo.Targets[0]
                        ));
                        break;
                    case PlayaDisableIfAttribute disableIfAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Disable,
                            disableIfAttribute.ConditionInfos,
                            fieldWithInfo.Targets[0]
                        ));
                        break;
                    case IPlayaArraySizeAttribute arraySizeAttribute:
                        if(SerializedUtils.IsOk(fieldWithInfo.SerializedProperty))
                        {
                            // ReSharper disable once ArrangeRedundantParentheses
                            arraySize = (fieldWithInfo.SerializedProperty.propertyType == SerializedPropertyType.Generic
                                         && fieldWithInfo.SerializedProperty.isArray)
                                ? GetArraySize(arraySizeAttribute, fieldWithInfo.SerializedProperty,
                                    fieldWithInfo.FieldInfo, fieldWithInfo.Targets[0], isImGui)
                                : (-1, -1);
                        }
                        break;
                }
            }

            for (int i = 0; i < preCheckInternalInfos.Count; i++)
            {
                preCheckInternalInfos[i] = SaintsEditorUtils.FillResult(preCheckInternalInfos[i]);
            }

            (bool showIfResult, bool disableIfResult) = SaintsEditorUtils.GetToggleResult(preCheckInternalInfos);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
            Debug.Log(
                $"showIfResult={showIfResult} (hasShow={hasShow}, show={show}, hide={hide})");
#endif
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
            Debug.Log(
                $"disableIfResult={disableIfResult} (disable={disable}, enable={enable})");
#endif


            PlayaRichLabelAttribute richLabelAttribute = fieldWithInfo.PlayaAttributes.OfType<PlayaRichLabelAttribute>().FirstOrDefault();
            bool hasRichLabel = richLabelAttribute != null;

            string richLabelXml = "";
            // ReSharper disable once InvertIf
            if (hasRichLabel)
            {
                if (richLabelAttribute.IsCallback)
                {
                    (string error, object rawResult) = GetCallback(fieldWithInfo, richLabelAttribute.RichTextXml);
                    if (error == "")
                    {
                        richLabelXml = rawResult == null ? "" : rawResult.ToString();
                    }
                    else
                    {
                        richLabelXml = ObjectNames.NicifyVariableName(GetMemberInfo(fieldWithInfo).Name);
                    }
                }
                else
                {
                    richLabelXml = richLabelAttribute.RichTextXml;
                }
            }

            return new PreCheckResult
            {
                IsDisabled = disableIfResult,
                IsShown = showIfResult,
                ArraySize = arraySize,

                HasRichLabel = hasRichLabel,
                RichLabelXml = richLabelXml,
            };
        }

        private bool _getByXPathKeepUpdate = true;

        private (int min, int max) GetArraySize(IPlayaArraySizeAttribute genArraySizeAttribute, SerializedProperty property, FieldInfo info, object parent, bool isImGui)
        {
            switch (genArraySizeAttribute)
            {
#pragma warning disable 0618
                case PlayaArraySizeAttribute playaArraySizeAttribute:
                    return (playaArraySizeAttribute.Size, playaArraySizeAttribute.Size);
#pragma warning restore 0618
                case ArraySizeAttribute arraySizeAttribute:
                {
                    (string error, bool _, int min, int max) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, property, info, parent);
#if SAINTSFIELD_DEBUG
                    if (error != "")
                    {
                        Debug.LogError(error);
                    }
#endif
                    return (min, max);
                }
#if (WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE
                case GetWwiseAttribute _:
                {
                    if (!_getByXPathKeepUpdate)
                    {
                        return (-1, -1);
                    }
                    _getByXPathKeepUpdate = GetWwiseAttributeDrawerHelper.HelperGetArraySize(property, info, isImGui);
                }
                    return (-1, -1);
#endif
                case GetByXPathAttribute _:
                {
                    if (!_getByXPathKeepUpdate)
                    {
                        return (-1, -1);
                    }
                    _getByXPathKeepUpdate = GetByXPathAttributeDrawer.HelperGetArraySize(property, info, isImGui);
                }
                    return (-1, -1);
                default:
                    return (-1, -1);
            }
        }

        protected static (string error, object rawResult) GetCallback(SaintsFieldWithInfo fieldWithInfo, string by)
        {
            object target = fieldWithInfo.Targets[0];

            // types.Reverse();
            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypes(target))
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                    ReflectUtils.GetProp(eachType, by);
                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.Field:
                    {
                        return ("", ((FieldInfo)fieldOrMethodInfo).GetValue(target));
                    }

                    case ReflectUtils.GetPropType.Property:
                    {
                        return ("", ((PropertyInfo)fieldOrMethodInfo).GetValue(target));
                    }
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        object curValue;

                        switch (fieldWithInfo.RenderType)
                        {
                            case SaintsRenderType.SerializedField:
                                // this can not be a list element because Editor component do not obtain it
                                curValue = fieldWithInfo.FieldInfo.GetValue(target);
                                break;
                            case SaintsRenderType.NonSerializedField:
                                curValue = fieldWithInfo.FieldInfo.GetValue(target);
                                break;
                            case SaintsRenderType.NativeProperty:
                                curValue = fieldWithInfo.PropertyInfo.GetValue(target);
                                break;
                            case SaintsRenderType.Method:
                                curValue = fieldWithInfo.MethodInfo;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.RenderType), fieldWithInfo.RenderType, null);
                        }

                        object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]{curValue});

                        // Debug.Log($"passParams={passParams[0]==null}, length={passParams.Length}, curValue==null={curValue==null}");

                        try
                        {
                            return ("", methodInfo.Invoke(
                                target,
                                passParams
                            ));
                        }
                        catch (TargetInvocationException e)
                        {
                            Debug.LogException(e);
                            Debug.Assert(e.InnerException != null);
                            return (e.InnerException.Message, null);
                        }
                        catch (Exception e)
                        {
                            // _error = e.Message;
                            Debug.LogException(e);
                            return (e.Message, null);
                        }
                    }
                    case ReflectUtils.GetPropType.NotFound:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }
            return ($"{by} not found in {GetMemberInfo(fieldWithInfo).Name}", null);
        }

        public abstract void OnDestroy();
        public abstract void OnSearchField(string searchString);

        protected SerializedProperty _serializedProperty;

        public void SetSerializedProperty(SerializedProperty property)
        {
            _serializedProperty = property;
        }

        public static string GetFriendlyName(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.SerializedProperty != null)
            {
                return fieldWithInfo.SerializedProperty.displayName;
            }

            if (fieldWithInfo.MethodInfo != null)
            {
                return ObjectNames.NicifyVariableName(fieldWithInfo.MethodInfo.Name);
            }
            if (fieldWithInfo.FieldInfo != null)
            {
                return ObjectNames.NicifyVariableName(fieldWithInfo.FieldInfo.Name);
            }
            if (fieldWithInfo.PropertyInfo != null)
            {
                return ObjectNames.NicifyVariableName(fieldWithInfo.PropertyInfo.Name);
            }

            return "";
        }

    }
}
