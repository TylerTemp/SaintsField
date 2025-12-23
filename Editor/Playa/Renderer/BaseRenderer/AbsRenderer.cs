using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Drawers.GUIColor;
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
#if (WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE
using SaintsField.Editor.Drawers.Wwise.GetWwiseDrawer;
using SaintsField.Wwise;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer: ISaintsRenderer
    {
        public bool InAnyHorizontalLayout { get; set; }
        public bool InDirectHorizontalLayout { get; set; }
        public bool NoLabel { get; set; }

        protected abstract bool AllowGuiColor { get; }

        // ReSharper disable InconsistentNaming
        public readonly SaintsFieldWithInfo FieldWithInfo;
        // protected readonly SerializedObject SerializedObject;
        // ReSharper enable InconsistentNaming

        public readonly struct PreCheckResult
        {
            public readonly bool IsShown;
            public readonly bool IsDisabled;
            // public int ArraySize;  // NOTE: -1=No Limit, 0=0, 1=More Than 0
            public readonly (int min, int max) ArraySize;
            public readonly bool HasRichLabel;
            public readonly string RichLabelXml;

            public PreCheckResult(bool isShown, bool isDisabled, (int min, int max) arraySize,
                bool hasRichLabel, string richLabelXml)
            {
                IsShown = isShown;
                IsDisabled = isDisabled;
                ArraySize = arraySize;
                HasRichLabel = hasRichLabel;
                RichLabelXml = richLabelXml;
            }
        }

        private readonly GUIColorAttribute _guiColorAttribute;

        protected AbsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            FieldWithInfo = fieldWithInfo;
            _guiColorAttribute = fieldWithInfo.PlayaAttributes?.OfType<GUIColorAttribute>().FirstOrDefault();
        }

        public bool HasGuiColor()
        {
            if(!AllowGuiColor)
            {
                return false;
            }
            return _guiColorAttribute != null;
        }

        public string ApplyGuiColor(VisualElement result)
        {
            if (!HasGuiColor())
            {
                return "";
            }
            (string error, Color color) = GUIColorAttributeDrawer.GetColor(_guiColorAttribute, FieldWithInfo.SerializedProperty,
                (MemberInfo)FieldWithInfo.FieldInfo ?? (MemberInfo)FieldWithInfo.PropertyInfo ?? FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0]);

            if (error == "")
            {
                UIToolkitUtils.ApplyColor(result, color);
            }

            return error;
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
            if (fieldWithInfo.PlayaAttributes == null)
            {
                return new PreCheckResult(true, false, (-1, -1), false, "");
            }

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
                preCheckInternalInfos[i] = SaintsEditorUtils.FillResult(preCheckInternalInfos[i], fieldWithInfo.SerializedProperty);
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


            LabelTextAttribute richLabelAttribute = fieldWithInfo.PlayaAttributes.OfType<LabelTextAttribute>().FirstOrDefault();
            bool hasRichLabel = richLabelAttribute != null;

            string richLabelXml = null;
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
                        Debug.LogError(error);
                        richLabelXml = ObjectNames.NicifyVariableName(GetMemberInfo(fieldWithInfo).Name);
                    }
                }
                else
                {
                    richLabelXml = richLabelAttribute.RichTextXml;
                }
            }

            return new PreCheckResult(showIfResult, disableIfResult, arraySize, hasRichLabel, richLabelXml);
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
                // ReSharper disable once RedundantDiscardDesignation
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
                // ReSharper disable once RedundantDiscardDesignation
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

//         protected IReadOnlyList<object> GetFreshTargets()
//         {
//             bool isStruct = ReflectUtils.TypeIsStruct(FieldWithInfo.Targets[0].GetType());
//             if (!isStruct || FieldWithInfo.TargetParent == null || FieldWithInfo.TargetMemberInfo == null)
//             {
//                 return FieldWithInfo.Targets;
//             }
//
//             List<object> refreshedTargets = new List<object>(FieldWithInfo.Targets.Count);
//             foreach (var target in FieldWithInfo.Targets)
//             {
//                 switch (FieldWithInfo.TargetMemberInfo)
//                 {
//                     case FieldInfo fieldInfo:
//                     {
//                         try
//                         {
//                             object useTarget = fieldInfo.GetValue(FieldWithInfo.TargetParent);
//                             if (FieldWithInfo.TargetMemberIndex != -1)
//                             {
//                                 useTarget = GetCollectionIndex(useTarget, FieldWithInfo.TargetMemberIndex);
//                             }
//                             refreshedTargets.Add(useTarget);
//                             // Debug.Log($"useTarget={useTarget}");
//                         }
//                         catch (Exception e)
//                         {
// #if SAINTSFIELD_DEBUG
//                             Debug.LogException(e);
// #endif
//                             refreshedTargets.Add(target);
//                         }
//                     }
//                         break;
//                     case PropertyInfo propertyInfo:
//                     {
//                         if (propertyInfo.CanRead)
//                         {
//                             try
//                             {
//                                 object useTarget = propertyInfo.GetValue(FieldWithInfo.TargetParent);
//                                 if (FieldWithInfo.TargetMemberIndex != -1)
//                                 {
//                                     useTarget = GetCollectionIndex(useTarget, FieldWithInfo.TargetMemberIndex);
//                                 }
//                                 refreshedTargets.Add(useTarget);
//                             }
//                             catch (Exception e)
//                             {
// #if SAINTSFIELD_DEBUG
//                                 Debug.LogException(e);
// #endif
//                             }
//                         }
//                     }
//                         break;
//                 }
//             }
//         }
//
        // This is wrapped inside try-catch already
        protected static object GetCollectionIndex(object collectionInfo, int targetMemberIndex)
        {
            switch (collectionInfo)
            {
                case Array arr:
                {
                    return arr.GetValue(targetMemberIndex);
                }
                case IList lis:
                {
                    return lis[targetMemberIndex];
                }
                default:
                    throw new Exception(
                        $"{collectionInfo} is not a supported collection type: {collectionInfo?.GetType()}");
            }
        }

        protected static (string error, object rawResult) GetCallback(SaintsFieldWithInfo fieldWithInfo, string by)
        {
            object target = fieldWithInfo.Targets[0];
            bool isStruct = ReflectUtils.TypeIsStruct(fieldWithInfo.Targets[0].GetType());
            // Debug.Log($"isStruct={isStruct}/fieldWithInfo.TargetParent={fieldWithInfo.TargetParent}/fieldWithInfo.TargetMemberInfo={fieldWithInfo.TargetMemberInfo}/fieldWithInfo.TargetMemberIndex={fieldWithInfo.TargetMemberIndex}");
            if (isStruct && fieldWithInfo.TargetParent != null && fieldWithInfo.TargetMemberInfo != null)
            {
                switch (fieldWithInfo.TargetMemberInfo)
                {
                    case FieldInfo fieldInfo:
                    {
                        try
                        {
                            target = fieldInfo.GetValue(fieldWithInfo.TargetParent);
                            if (fieldWithInfo.TargetMemberIndex >= 0)
                            {
                                target = GetCollectionIndex(fieldInfo.GetValue(fieldWithInfo.TargetParent),
                                    fieldWithInfo.TargetMemberIndex);
                            }
                        }
                        catch (Exception e)
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogException(e);
#endif
                        }
                    }
                        break;
                    case PropertyInfo propertyInfo:
                    {
                        if (propertyInfo.CanRead)
                        {
                            try
                            {
                                target = propertyInfo.GetValue(fieldWithInfo.TargetParent);
                                if (fieldWithInfo.TargetMemberIndex >= 0)
                                {
                                    target = GetCollectionIndex(propertyInfo.GetValue(fieldWithInfo.TargetParent),
                                        fieldWithInfo.TargetMemberIndex);
                                }
                            }
                            catch (Exception e)
                            {
#if SAINTSFIELD_DEBUG
                                Debug.LogException(e);
#endif
                            }
                        }
                    }
                        break;
                }
            }

            (string error, object result) result = Util.GetOf<object>(by, null, fieldWithInfo.SerializedProperty,
                (MemberInfo)fieldWithInfo.FieldInfo ?? fieldWithInfo.PropertyInfo, target, null);
            // Debug.Log(r);
            return result;
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
