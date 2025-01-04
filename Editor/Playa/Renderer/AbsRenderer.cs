using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.XPathDrawers;
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public abstract partial class AbsRenderer: ISaintsRenderer
    {
        // ReSharper disable InconsistentNaming
        public readonly SaintsFieldWithInfo FieldWithInfo;
        // protected readonly SerializedObject SerializedObject;
        // ReSharper enable InconsistentNaming

        protected struct PreCheckResult
        {
            public bool IsShown;
            public bool IsDisabled;
            public int ArraySize;  // NOTE: -1=No Limit, 0=0, 1=More Than 0
            public bool HasRichLabel;
            public string RichLabelXml;
        }

        protected AbsRenderer(SaintsFieldWithInfo fieldWithInfo)
        {
            FieldWithInfo = fieldWithInfo;
            // SerializedObject = serializedObject;
        }

        protected static MemberInfo GetMemberInfo(SaintsFieldWithInfo info)
        {
            switch (info.RenderType)
            {
                case SaintsRenderType.SerializedField:
                    return info.FieldInfo;
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
            List<ToggleCheckInfo> preCheckInternalInfos = new List<ToggleCheckInfo>();
            int arraySize = -1;
            foreach (IPlayaAttribute playaAttribute in fieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case IVisibilityAttribute visibilityAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        {
                            Type = visibilityAttribute.IsShow? ToggleType.Show: ToggleType.Hide,
                            ConditionInfos = visibilityAttribute.ConditionInfos,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaEnableIfAttribute enableIfAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        {
                            Type = ToggleType.Enable,
                            ConditionInfos = enableIfAttribute.ConditionInfos,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaDisableIfAttribute disableIfAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        {
                            Type = ToggleType.Disable,
                            ConditionInfos = disableIfAttribute.ConditionInfos,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case IPlayaArraySizeAttribute arraySizeAttribute:
                        if(fieldWithInfo.SerializedProperty != null)
                        {
                            // Debug.Log(fieldWithInfo.SerializedProperty);
                            arraySize = fieldWithInfo.SerializedProperty.isArray
                                ? GetArraySize(arraySizeAttribute, fieldWithInfo.SerializedProperty,
                                    fieldWithInfo.FieldInfo, fieldWithInfo.Target, isImGui)
                                : -1;
                        }
                        break;
                }
            }

            foreach (ToggleCheckInfo preCheckInternalInfo in preCheckInternalInfos)
            {
                SaintsEditorUtils.FillResult(preCheckInternalInfo);
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

        private int GetArraySize(IPlayaArraySizeAttribute genArraySizeAttribute, SerializedProperty property, FieldInfo info, object parent, bool isImGui)
        {
            switch (genArraySizeAttribute)
            {
#pragma warning disable 0618
                case PlayaArraySizeAttribute playaArraySizeAttribute:
                    return playaArraySizeAttribute.Size;
#pragma warning restore 0618
                case ArraySizeAttribute arraySizeAttribute:
                    return arraySizeAttribute.Min;
                // case GetComponentInParentsAttribute getComponentInParentsAttribute:
                //     return GetComponentInParentsAttributeDrawer.HelperGetArraySize(property, getComponentInParentsAttribute, info);
                // case GetComponentInSceneAttribute getComponentInSceneAttribute:
                //     return GetComponentInSceneAttributeDrawer.HelperGetArraySize(getComponentInSceneAttribute, info);
                // case GetComponentByPathAttribute getComponentByPathAttribute:
                //     return GetComponentByPathAttributeDrawer.HelperGetArraySize(property, getComponentByPathAttribute, info);
                // case GetPrefabWithComponentAttribute getPrefabWithComponentAttribute:
                //     return GetPrefabWithComponentAttributeDrawer.HelperGetArraySize(getPrefabWithComponentAttribute, info);
                // case GetScriptableObjectAttribute getScriptableObjectAttribute:
                //     return GetScriptableObjectAttributeDrawer.HelperGetArraySize(getScriptableObjectAttribute, info);
                case GetByXPathAttribute _:
                {
                    if (!_getByXPathKeepUpdate)
                    {
                        return -1;
                    }
                    _getByXPathKeepUpdate = GetByXPathAttributeDrawer.HelperGetArraySize(property, info, isImGui);
                }
                    return -1;
                default:
                    return -1;
            }
        }


        private static (string error, object rawResult) GetCallback(SaintsFieldWithInfo fieldWithInfo, string by)
        {
            object target = fieldWithInfo.Target;

            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();
            foreach (Type eachType in types)
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




    }
}
