using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
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
    public abstract partial class AbsRenderer: ISaintsRenderer, IRichTextTagProvider
    {
        public bool InAnyHorizontalLayout { get; set; }
        public bool InDirectHorizontalLayout { get; set; }
        public bool NoLabel { get; set; }

        protected abstract bool AllowGuiColor { get; }

        // ReSharper disable InconsistentNaming
        public SaintsFieldWithInfo FieldWithInfo { get; private set; }
        // protected readonly SerializedObject SerializedObject;
        // ReSharper enable InconsistentNaming

        public readonly struct PreCheckResult
        {
            public readonly string Error;
            public readonly bool IsShown;
            public readonly bool IsDisabled;
            // public int ArraySize;  // NOTE: -1=No Limit, 0=0, 1=More Than 0
            public readonly (int min, int max) ArraySize;
            public readonly bool HasRichLabel;
            public readonly string RichLabelXml;

            public PreCheckResult(string error, bool isShown, bool isDisabled, (int min, int max) arraySize,
                bool hasRichLabel, string richLabelXml)
            {
                Error = error;
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
                return new PreCheckResult("", true, false, (-1, -1), false, "");
            }

            List<ToggleCheckInfo> preCheckInternalInfos = new List<ToggleCheckInfo>(fieldWithInfo.PlayaAttributes.Count);
            (int, int) arraySize = (-1, -1);

            object parent = fieldWithInfo.Targets[0];
            // object parent = GetRefreshedTarget(FieldWithInfo, FieldWithInfo.Targets[0]).useTarget;

            foreach (IPlayaAttribute playaAttribute in fieldWithInfo.PlayaAttributes)
            {
                // Debug.Log($"parent={parent} for {fieldWithInfo.SerializedProperty.propertyPath}({fieldWithInfo.SerializedProperty.serializedObject.targetObject})");
                switch (playaAttribute)
                {
                    case IVisibilityAttribute visibilityAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        (
                            visibilityAttribute.IsShow? ToggleType.Show: ToggleType.Hide,
                            visibilityAttribute.ConditionInfos,
                            parent
                        ));
                        break;
                    case EnableIfAttribute enableIfAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Enable,
                            enableIfAttribute.ConditionInfos,
                            parent
                        ));
                        break;
                    case DisableIfAttribute disableIfAttribute:
                        preCheckInternalInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Disable,
                            disableIfAttribute.ConditionInfos,
                            parent
                        ));
                        break;
                    case IPlayaArraySizeAttribute arraySizeAttribute:
                        if(SerializedUtils.IsOk(fieldWithInfo.SerializedProperty))
                        {
                            // ReSharper disable once ArrangeRedundantParentheses
                            arraySize = (fieldWithInfo.SerializedProperty.propertyType == SerializedPropertyType.Generic
                                         && fieldWithInfo.SerializedProperty.isArray)
                                ? GetArraySize(arraySizeAttribute, fieldWithInfo.SerializedProperty,
                                    fieldWithInfo.FieldInfo, parent, isImGui)
                                : (-1, -1);
                        }
                        break;
                }
            }

            SaintsContext.SerializedProperty = fieldWithInfo.SerializedProperty;

            for (int i = 0; i < preCheckInternalInfos.Count; i++)
            {
                preCheckInternalInfos[i] = SaintsEditorUtils.FillResult(preCheckInternalInfos[i], fieldWithInfo.SerializedProperty);
            }

            List<string> errors = preCheckInternalInfos.SelectMany(each => each.Errors).ToList();

            (bool showIfResult, bool disableIfResult) = SaintsEditorUtils.GetToggleResult(preCheckInternalInfos);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
            Debug.Log(
                $"showIfResult={showIfResult} (hasShow={hasShow}, show={show}, hide={hide})");
#endif
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
//             Debug.Log(
//                 $"disableIfResult={disableIfResult}");
// #endif


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
                        errors.Add(error);
                        // Debug.LogError(error);
                        richLabelXml = ObjectNames.NicifyVariableName(GetMemberInfo(fieldWithInfo).Name);
                    }
                }
                else
                {
                    richLabelXml = richLabelAttribute.RichTextXml;
                }
            }

            return new PreCheckResult(string.Join("\n", errors), showIfResult, disableIfResult, arraySize, hasRichLabel, richLabelXml);
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
#pragma warning disable CS0168 // Variable is declared but never used
                        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                        {
                            // ignored
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
#pragma warning disable CS0168 // Variable is declared but never used
                            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                            {
                                // ignored
#if SAINTSFIELD_DEBUG
                                Debug.LogException(e);
#endif
                            }
                        }
                    }
                        break;
                }
            }

            (string error, MemberInfo _, object result) result = Util.GetOf<object>(by, null, fieldWithInfo.SerializedProperty,
                (MemberInfo)fieldWithInfo.FieldInfo ?? fieldWithInfo.PropertyInfo, target, null);
            // Debug.Log(r);
            return (result.error, result.result);
        }

        public abstract void OnDestroy();
        public abstract void OnSearchField(string searchString);

        protected SerializedProperty _serializedProperty;

        public void SetSerializedProperty(SerializedProperty property)
        {
            _serializedProperty = property;
        }

        public void RefreshTargets(object[] targets)
        {
            FieldWithInfo = FieldWithInfo.RefreshTargets(targets);
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


        public static (object rawMemberValue, object useTarget) GetRefreshedTarget(SaintsFieldWithInfo fieldWithInfo, object eachTarget)
        {
            // bool isStruct = ReflectUtils.TypeIsStruct(eachTarget.GetType());
            object useTarget = eachTarget;
            object rawMemberValue = eachTarget;
            if (fieldWithInfo.TargetParent != null && fieldWithInfo.TargetMemberInfo != null)
            {
                switch (fieldWithInfo.TargetMemberInfo)
                {
                    case FieldInfo fieldInfo:
                    {
                        try
                        {
                            useTarget = rawMemberValue =  fieldInfo.GetValue(fieldWithInfo.TargetParent);
                            if (fieldWithInfo.TargetMemberIndex != -1)
                            {
                                useTarget = GetCollectionIndex(useTarget, fieldWithInfo.TargetMemberIndex);
                            }
                            // Debug.Log($"useTarget={useTarget}");
                        }
#pragma warning disable CS0168 // Variable is declared but never used
                        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                        {
                            // ignored
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
                                useTarget = rawMemberValue = propertyInfo.GetValue(fieldWithInfo.TargetParent);
                                if (fieldWithInfo.TargetMemberIndex != -1)
                                {
                                    useTarget = GetCollectionIndex(useTarget, fieldWithInfo.TargetMemberIndex);
                                }
                            }
#pragma warning disable CS0168 // Variable is declared but never used
                            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                            {
                                // ignored
#if SAINTSFIELD_DEBUG
                                Debug.LogException(e);
#endif
                            }
                        }
                    }
                        break;
                }
            }

            return (rawMemberValue, useTarget);
        }

        private static readonly Type[] SkipTypes = { typeof(IntPtr), typeof(UIntPtr), typeof(void) };

        public static bool SkipTypeDrawing(Type checkType)
        {
            foreach (Type disallowType in SkipTypes)
            {
                if (disallowType.IsAssignableFrom(checkType))
                {
                    return true;
                }
            }

            return false;
        }

        public string GetLabel()
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return FieldWithInfo.SerializedProperty.displayName;
                    }

                    return "";
                }
                case SaintsRenderType.NonSerializedField:
                {
                    if (FieldWithInfo.FieldInfo != null)
                    {
                        return ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name);
                    }

                    return "";
                }
                case SaintsRenderType.Method:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name);
                case SaintsRenderType.NativeProperty:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name);
                case SaintsRenderType.ClassStruct:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.ClassStructType.Name);
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        public string GetContainerType()
        {
            return GetTargetType().Name;
        }

        private Type GetTargetType()
        {
            return  FieldWithInfo.ClassStructType ?? FieldWithInfo.Targets[0].GetType();
        }

        public string GetContainerTypeBaseType()
        {
            return GetTargetType().BaseType?.Name ?? "";
        }

        public string GetIndex(string formatter)
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return "";
                    }

                    int propPath = SerializedUtils.PropertyPathIndex(FieldWithInfo.SerializedProperty.propertyPath);
                    return propPath < 0 ? "" : propPath.ToString();
                }
                case SaintsRenderType.NonSerializedField:
                case SaintsRenderType.Method:
                case SaintsRenderType.NativeProperty:
                case SaintsRenderType.ClassStruct:
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        public string GetField(string rawContent, string tagName, string tagValue)
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    if (!SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return "";
                    }

                    // string error = "";

                    (string error, int index, object value) result = Util.GetValue(FieldWithInfo.SerializedProperty, FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
                    // (string error, int index, object value) accResult = result;
                    if (result.error != "")
                    {
                        // error = result.error;
                    }
                    else
                    {
                        if (tagName == "field")
                        {
                        }
                        else
                        {
                            string revName = tagName["field.".Length..];

                            (string error, MemberInfo _, object result) getOfValue = Util.GetOf<object>(revName, null,
                                FieldWithInfo.SerializedProperty,
                                FieldWithInfo.FieldInfo, result.value, null);

                            result = (getOfValue.error, result.index, getOfValue.result);
                        }
                    }

                    // ReSharper disable once InvertIf
                    if (result.error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning(result.error);
#endif
                        return rawContent;
                    }

                    return RichTextDrawer.TagStringFormatter(result.value, tagValue);
                }
                case SaintsRenderType.NonSerializedField:
                {
                    FieldInfo memberInfo = FieldWithInfo.FieldInfo;
                    object value;
                    try
                    {
                        value = memberInfo.GetValue(FieldWithInfo.Targets[0]);
                    }
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning(e);
#endif
                        return "";
                    }

                    if (tagName == "field")
                    {
                    }
                    else
                    {
                        string revName = tagName["field.".Length..];

                        (string error, MemberInfo _, object result) getOfValue = Util.GetOf<object>(revName, null,
                            null,
                            memberInfo, value, null);
                        if (!string.IsNullOrEmpty(getOfValue.error))
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(getOfValue.error);
#endif
                            return "";
                        }

                        value = getOfValue.result;
                    }

                    return RichTextDrawer.TagStringFormatter(value, tagValue);
                }
                case SaintsRenderType.NativeProperty:
                {
                    PropertyInfo memberInfo = FieldWithInfo.PropertyInfo;
                    object value;
                    try
                    {
                        value = memberInfo.GetValue(FieldWithInfo.Targets[0]);
                    }
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning(e);
#endif
                        return "";
                    }

                    if (tagName == "field")
                    {
                    }
                    else
                    {
                        string revName = tagName["field.".Length..];

                        (string error, MemberInfo _, object result) getOfValue = Util.GetOf<object>(revName, null,
                            null,
                            memberInfo, value, null);
                        if (!string.IsNullOrEmpty(getOfValue.error))
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogWarning(getOfValue.error);
#endif
                            return "";
                        }

                        value = getOfValue.result;
                    }

                    return RichTextDrawer.TagStringFormatter(value, tagValue);
                }
                case SaintsRenderType.Method:
                case SaintsRenderType.ClassStruct:
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }
    }
}
