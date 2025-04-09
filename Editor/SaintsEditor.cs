﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer;
using SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer;
using SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings;
using SaintsField.Editor.Playa.Renderer.SpecialRenderer.Table;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using DG.DOTweenEditor;
#endif


namespace SaintsField.Editor
{
    public partial class SaintsEditor: UnityEditor.Editor, IDOTweenPlayRecorder, IMakeRenderer
    {
        private static bool _saintsEditorIMGUI = true;

        // private MonoScript _monoScript;
        // private List<SaintsFieldWithInfo> _fieldWithInfos = new List<SaintsFieldWithInfo>();

        [NonSerialized]
        public bool EditorShowMonoScript = true;

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
        private static readonly HashSet<IDOTweenPlayRecorder> AliveInstances = new HashSet<IDOTweenPlayRecorder>();
        public static void RemoveInstance(IDOTweenPlayRecorder doTweenPlayRecorder)
        {
            AliveInstances.Remove(doTweenPlayRecorder);
            if (AliveInstances.Count == 0)
            {
                DOTweenEditorPreview.Stop();
            }
        }
        public static void AddInstance(IDOTweenPlayRecorder doTweenPlayRecorder)
        {
            AliveInstances.Add(doTweenPlayRecorder);
        }
#endif

        // private Dictionary<string, ISaintsRendererGroup> _layoutKeyToGroup;
        private IReadOnlyList<ISaintsRenderer> _renderers;

        public static MonoScript GetMonoScript(UnityEngine.Object target)
        {
            try
            {
                return MonoScript.FromMonoBehaviour((MonoBehaviour) target);
            }
            catch (Exception)
            {
                try
                {
                    return MonoScript.FromScriptableObject((ScriptableObject)target);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static IReadOnlyList<ISaintsRenderer> Setup(ICollection<string> skipSerializedFields, SerializedObject serializedObject, IMakeRenderer makeRenderer,
            object target)
        {
            string[] serializableFields = GetSerializedProperties(serializedObject).ToArray();
            // Debug.Log($"serializableFields={string.Join(",", serializableFields)}");
            Dictionary<string, SerializedProperty> serializedPropertyDict = serializableFields
                .Where(each => !skipSerializedFields.Contains(each))
                .ToDictionary(each => each, serializedObject.FindProperty);
            // Debug.Log($"serializedPropertyDict.Count={serializedPropertyDict.Count}");
            return HelperGetRenderers(serializedPropertyDict, serializedObject, makeRenderer, target);
        }

        public static IEnumerable<SaintsFieldWithInfo> HelperGetSaintsFieldWithInfo(
            IReadOnlyDictionary<string, SerializedProperty> serializedPropertyDict,
            object target)
        {
            List<SaintsFieldWithInfo> fieldWithInfos = new List<SaintsFieldWithInfo>();
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();

            // Dictionary<string, SerializedProperty> pendingSerializedProperties = new Dictionary<string, SerializedProperty>(serializedPropertyDict);
            Dictionary<string, SerializedProperty> pendingSerializedProperties = serializedPropertyDict.ToDictionary(each => each.Key, each => each.Value);
            pendingSerializedProperties.Remove("m_Script");

            // base type -> this type
            // a later field should override current in different depth
            // but, if the field is not in the same depth, it should be added (method override)
            // Yep, C# is a crap
            for (int inherentDepth = 0; inherentDepth < types.Count; inherentDepth++)
            {
                Type systemType = types[inherentDepth];

                // as we can not get the correct order, we'll make it order as: field(serialized+nonSerialized), property, method
                List<SaintsFieldWithInfo> thisDepthInfos = new List<SaintsFieldWithInfo>();
                List<string> memberDepthIds = new List<string>();

                foreach (MemberInfo memberInfo in systemType
                             .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                         BindingFlags.Public | BindingFlags.DeclaredOnly)
                             .OrderBy(memberInfo => memberInfo.MetadataToken))  // this is still not the correct order, but... a bit better
                {
                    // Debug.Log(memberInfo.Name);
                    IReadOnlyList<IPlayaAttribute> playaAttributes =
                        ReflectCache.GetCustomAttributes<IPlayaAttribute>(memberInfo);

                    // ISaintsLayoutBase[] layoutBases = GetLayoutBases(playaAttributes.OfType<ISaintsLayoutBase>()).ToArray();

                    switch (memberInfo)
                    {
                        case FieldInfo fieldInfo:
                        {
                            #region SerializedField

                            if (serializedPropertyDict.ContainsKey(fieldInfo.Name))
                            {
                                // Debug.Log($"Name            : {fieldInfo.Name}");
                                // Debug.Log($"Declaring Type  : {fieldInfo.DeclaringType}");
                                // Debug.Log($"IsPublic        : {fieldInfo.IsPublic}");
                                // Debug.Log($"MemberType      : {fieldInfo.MemberType}");
                                // Debug.Log($"FieldType       : {fieldInfo.FieldType}");
                                // Debug.Log($"IsFamily        : {fieldInfo.IsFamily}");

                                OrderedAttribute orderProp =
                                    playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                                int order = orderProp?.Order ?? int.MinValue;

                                // Debug.Log($"{fieldInfo.Name}/{string.Join(",", pendingSerializedProperties.Keys)}");
                                thisDepthInfos.Add(new SaintsFieldWithInfo
                                {
                                    PlayaAttributes = playaAttributes,
                                    // PlayaAttributesQueue = playaAttributes,
                                    // LayoutBases = layoutBases,
                                    Target = target,

                                    RenderType = SaintsRenderType.SerializedField,
                                    SerializedProperty = serializedPropertyDict[fieldInfo.Name],
                                    MemberId = fieldInfo.Name,
                                    FieldInfo = fieldInfo,
                                    InherentDepth = inherentDepth,
                                    Order = order,
                                    // serializable = true,
                                });
                                memberDepthIds.Add(fieldInfo.Name);
                                // Debug.Log($"remove key {fieldInfo.Name}");
                                pendingSerializedProperties.Remove(fieldInfo.Name);
                            }
                            #endregion

                            #region nonSerFieldInfo

                            else if (playaAttributes.Count > 0)
                            {
                                OrderedAttribute orderProp = playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                                int order = orderProp?.Order ?? int.MinValue;
                                thisDepthInfos.Add(new SaintsFieldWithInfo
                                {
                                    PlayaAttributes = playaAttributes,
                                    // PlayaAttributesQueue = playaAttributes,
                                    // LayoutBases = layoutBases,
                                    Target = target,

                                    RenderType = SaintsRenderType.NonSerializedField,
                                    // memberType = nonSerFieldInfo.MemberType,
                                    MemberId = fieldInfo.Name,
                                    FieldInfo = fieldInfo,
                                    InherentDepth = inherentDepth,
                                    Order = order,
                                    // serializable = false,
                                });
                                memberDepthIds.Add(fieldInfo.Name);
                            }
                            #endregion
                        }
                            break;
                        case PropertyInfo propertyInfo:
                        {
                            #region NativeProperty
                            if(playaAttributes.Count > 0)
                            {
                                OrderedAttribute orderProp =
                                    playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                                int order = orderProp?.Order ?? int.MinValue;
                                thisDepthInfos.Add(new SaintsFieldWithInfo
                                {
                                    PlayaAttributes = playaAttributes,
                                    // PlayaAttributesQueue = playaAttributes,
                                    // LayoutBases = layoutBases,
                                    Target = target,

                                    RenderType = SaintsRenderType.NativeProperty,
                                    MemberId = propertyInfo.Name,
                                    PropertyInfo = propertyInfo,
                                    InherentDepth = inherentDepth,
                                    Order = order,
                                });
                                memberDepthIds.Add(propertyInfo.Name);
                            }
                            #endregion
                        }
                            break;
                        case MethodInfo methodInfo:
                        {
                            #region Method
                            // method attributes will be collected no matter what, because DOTweenPlayGroup depending on it even
                            // it has no attribute at all

                            // Attribute[] allMethodAttributes = methodInfo.GetCustomAttributes<Attribute>().ToArray();

                            OrderedAttribute orderProp =
                                playaAttributes.FirstOrDefault(each => each is OrderedAttribute) as OrderedAttribute;
                            int order = orderProp?.Order ?? int.MinValue;

                            // wrong: inspector does not care about inherited/new method. It just needs to use the last one
                            // right: we support method override now
                            // fieldWithInfos.RemoveAll(each => each.InherentDepth < inherentDepth && each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);
                            // methodInfos.RemoveAll(each => each.InherentDepth < inherentDepth && each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD
                            Debug.Log($"[{systemType}] method: {methodInfo.Name}");
#endif

                            string buttonExtraId = string.Join(":", methodInfo.GetParameters()
                                    .Select(each => each.ParameterType)
                                    .Append(methodInfo.ReturnType)
                                    .Select(each => each.FullName));

                            string buttonId = $"{methodInfo.Name}.{buttonExtraId}";

                            thisDepthInfos.Add(new SaintsFieldWithInfo
                            {
                                PlayaAttributes = playaAttributes,
                                // PlayaAttributesQueue = playaAttributes,
                                // LayoutBases = layoutBases,
                                Target = target,

                                // memberType = MemberTypes.Method,
                                RenderType = SaintsRenderType.Method,
                                MemberId = buttonId,
                                MethodInfo = methodInfo,
                                InherentDepth = inherentDepth,
                                Order = order,
                            });
                            memberDepthIds.Add(buttonId);
                            #endregion
                        }
                            break;
                    }
                }

                // now handle overrides
                fieldWithInfos.RemoveAll(each => memberDepthIds.Contains(each.MemberId));

                fieldWithInfos.AddRange(thisDepthInfos);
                // fieldWithInfos.AddRange(fieldInfos);
                // fieldWithInfos.AddRange(propertyInfos);
                // fieldWithInfos.AddRange(methodInfos);
            }

            if (pendingSerializedProperties.Count > 0)
            {
                // we got unused serialized properties because Unity directly inject them rather than using a
                // normal workflow
                foreach (KeyValuePair<string, SerializedProperty> pendingSer in pendingSerializedProperties.Reverse())
                {
                    fieldWithInfos.Insert(0, new SaintsFieldWithInfo
                    {
                        PlayaAttributes = Array.Empty<IPlayaAttribute>(),
                        // PlayaAttributesQueue = new List<IPlayaAttribute>(),
                        // LayoutBases = Array.Empty<ISaintsLayoutBase>(),
                        Target = target,

                        RenderType = SaintsRenderType.InjectedSerializedField,
                        SerializedProperty = pendingSer.Value,
                        FieldInfo = null,
                        InherentDepth = types.Count - 1,
                        Order = int.MinValue,
                        // serializable = true,
                    });
                }
            }

            return fieldWithInfos
                .WithIndex()
                .OrderBy(each => each.value.InherentDepth)
                .ThenBy(each => each.value.Order)
                .ThenBy(each => each.index)
                .Select(each => each.value);
        }

        // private static IEnumerable<IPlayaAttribute> WrapPlayaAttributes(IPlayaAttribute[] getCustomAttributes)
        // {
        //     foreach (IPlayaAttribute playaAttribute in getCustomAttributes)
        //     {
        //         switch (playaAttribute)
        //         {
        //             case LayoutTerminateHereAttribute layoutTerminateHereAttribute:
        //                 yield return new LayoutAttribute(".", layoutTerminateHereAttribute.Layout, false, layoutTerminateHereAttribute.MarginTop, layoutTerminateHereAttribute.MarginBottom);
        //                 yield return new LayoutEndAttribute(null, layoutTerminateHereAttribute.MarginTop, layoutTerminateHereAttribute.MarginBottom);
        //                 break;
        //             case LayoutCloseHereAttribute layoutCloseHereAttribute:
        //                 yield return new LayoutAttribute(".", layoutCloseHereAttribute.Layout, false, layoutCloseHereAttribute.MarginTop, layoutCloseHereAttribute.MarginBottom);
        //                 yield return new LayoutEndAttribute(".", layoutCloseHereAttribute.MarginTop, layoutCloseHereAttribute.MarginBottom);
        //                 break;
        //             default:
        //                 yield return playaAttribute;
        //                 break;
        //         }
        //     }
        // }

        // private static IEnumerable<ISaintsLayoutBase> GetLayoutBases(IEnumerable<ISaintsLayoutBase> layoutBases)
        // {
        //     foreach (ISaintsLayoutBase saintsLayoutBase in layoutBases)
        //     {
        //         switch (saintsLayoutBase)
        //         {
        //             case LayoutTerminateHereAttribute layoutTerminateHereAttribute:
        //                 yield return new LayoutAttribute(".", layoutTerminateHereAttribute.Layout, false, layoutTerminateHereAttribute.MarginTop, layoutTerminateHereAttribute.MarginBottom);
        //                 yield return new LayoutEndAttribute(null, layoutTerminateHereAttribute.MarginTop, layoutTerminateHereAttribute.MarginBottom);
        //                 break;
        //             case LayoutCloseHereAttribute layoutCloseHereAttribute:
        //                 yield return new LayoutAttribute(".", layoutCloseHereAttribute.Layout, false, layoutCloseHereAttribute.MarginTop, layoutCloseHereAttribute.MarginBottom);
        //                 yield return new LayoutEndAttribute(".", layoutCloseHereAttribute.MarginTop, layoutCloseHereAttribute.MarginBottom);
        //                 break;
        //             default:
        //                 yield return saintsLayoutBase;
        //                 break;
        //         }
        //     }
        // }

        public static IReadOnlyList<ISaintsRenderer> HelperGetRenderers(
            IReadOnlyDictionary<string, SerializedProperty> serializedPropertyDict, SerializedObject serializedObject,
            IMakeRenderer makeRenderer,
            object target)
        {
            IReadOnlyList<SaintsFieldWithInfo> fieldWithInfosSorted = HelperGetSaintsFieldWithInfo(serializedPropertyDict, target).ToArray();
            IReadOnlyList<RendererGroupInfo> chainedGroups = ChainSaintsFieldWithInfo(fieldWithInfosSorted, serializedObject, makeRenderer);
            // Debug.Log(chainedGroups.Count);
            // ISaintsRenderer[] r = HelperFlattenRendererGroupInfoIntoRenderers(chainedGroups, serializedObject, makeRenderer, target)
            //     .Select(each => each.saintsRenderer)
            //     .ToArray();
            ISaintsRenderer[] r = chainedGroups
                .Select(MakeRendererForGroupIfNeed)
                .ToArray();

            // Debug.Log($"Return renderers {r.Length}");

            return r;
        }

        private static ISaintsRenderer MakeRendererForGroupIfNeed(RendererGroupInfo rendererGroupInfo)
        {
            if (rendererGroupInfo.Renderer != null)
            {
                return rendererGroupInfo.Renderer;
            }

            ISaintsRendererGroup group =
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
                    rendererGroupInfo.Config.IsDoTween
                        // ReSharper disable once RedundantCast
                        ? (ISaintsRendererGroup)new DOTweenPlayGroup(rendererGroupInfo.Target)
                        : new SaintsRendererGroup(rendererGroupInfo.AbsGroupBy, rendererGroupInfo.Config, rendererGroupInfo.Target)
#else
                    new SaintsRendererGroup(rendererGroupInfo.AbsGroupBy, rendererGroupInfo.Config,
                        rendererGroupInfo.Target)
#endif
                ;
            foreach (RendererGroupInfo c in rendererGroupInfo.Children)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                Debug.Log($"Flatten {group} add RendererGroupInfo {c.AbsGroupBy}");
#endif

                group.Add(c.AbsGroupBy, MakeRendererForGroupIfNeed(c));
            }

            return group;
        }

//         private static IEnumerable<(string absGroupBy, ISaintsRenderer saintsRenderer)> HelperFlattenRendererGroupInfoIntoRenderers(IReadOnlyList<RendererGroupInfo> chainedGroups, SerializedObject serializedObject, IMakeRenderer makeRenderer, object target)
//         {
//             foreach (RendererGroupInfo rendererGroupInfo in chainedGroups)
//             {
//                 Debug.Log($"Flatten processing `{rendererGroupInfo.AbsGroupBy}/{rendererGroupInfo.AbsGroupBy == null}`");
//
//                 bool isEndNode = rendererGroupInfo.AbsGroupBy == null && rendererGroupInfo.Children.Count == 0;
//
//                 if (isEndNode)
//                 {
//                     foreach (RendererGroupInfo chainedGroup in chainedGroups)
//                     {
//                         if(chainedGroup.Renderer != null)
//                         {
//                             Debug.Log($"Flatten normal field {chainedGroup.Renderer}");
//                             yield return ("", chainedGroup.Renderer);
//                         }
//                     }
//
// //                     foreach (RendererGroupInfo result in chainedGroups)
// //                     {
// //                         if(result != null)
// //                         {
// // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
// //                             if(rendererGroupInfo.FieldWithInfo.MethodInfo == null)
// //                             {
// //                                 Debug.Log($"Flatten EndNode return {result}");
// //                             }
// // #endif
// //                             yield return ("", result);
// //                         }
// //                     }
//
//                 }
//                 else
//                 {
//                     (string absGroupBy, ISaintsRenderer saintsRenderer)[] children = HelperFlattenRendererGroupInfoIntoRenderers(rendererGroupInfo.Children, serializedObject, makeRenderer, target).ToArray();
//                     if (children.Length > 0)
//                     {
//                         string curGroupAbs = rendererGroupInfo.AbsGroupBy;
//
//                         ISaintsRendererGroup group =
// #if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
//                             rendererGroupInfo.Config.IsDoTween
//                                 // ReSharper disable once RedundantCast
//                                 ? (ISaintsRendererGroup)new DOTweenPlayGroup(target)
//                                 : new SaintsRendererGroup(curGroupAbs, rendererGroupInfo.Config, target)
// #else
//                             new SaintsRendererGroup(curGroupAbs, rendererGroupInfo.Config, target)
// #endif
//                         ;
//
//                         foreach ((string eachChildGroupBy, ISaintsRenderer eachChild) in children)
//                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                             Debug.Log($"Flatten {group} add renderer {eachChild}");
// #endif
//
//                             group.Add(eachChildGroupBy, eachChild);
//                         }
//
// // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                         Debug.Log($"Flatten {group} return with {children.Length} children");
// // #endif
//
//                         yield return (rendererGroupInfo.AbsGroupBy, group);
//                     }
// // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                     else
//                     {
//                         Debug.Log($"Flatten {rendererGroupInfo.AbsGroupBy} empty children, skip");
//                     }
// // #endif
//                 }
//             }
//         }

        private class RendererGroupInfo {
            public string AbsGroupBy;  // ""=normal fields, other=grouped fields
            public List<RendererGroupInfo> Children;
            public SaintsRendererGroup.Config Config;
            public AbsRenderer Renderer;
            public object Target;
        }

        private static IReadOnlyList<RendererGroupInfo> ChainSaintsFieldWithInfo(IReadOnlyList<SaintsFieldWithInfo> fieldWithInfosSorted, SerializedObject serializedObject, IMakeRenderer makeRenderer)
        {
            List<RendererGroupInfo> rendererGroupInfos = new List<RendererGroupInfo>();
            Dictionary<string, RendererGroupInfo> rootToRendererGroupInfo =
                new Dictionary<string, RendererGroupInfo>();

            RendererGroupInfo keepGroupingInfo = null;
            string preAbsGroupBy = null;
            // RendererGroupInfo lastGroupInfo = null;

            int inherent = -1;
            foreach (SaintsFieldWithInfo saintsFieldWithInfo in fieldWithInfosSorted)
            {
                bool isNewInherent = saintsFieldWithInfo.InherentDepth != inherent;
                inherent = saintsFieldWithInfo.InherentDepth;

                // IReadOnlyList<ISaintsLayoutBase> layoutBases = saintsFieldWithInfo.LayoutBases;
                // IReadOnlyList<ISaintsLayout> layouts = layoutBases.OfType<ISaintsLayout>().ToArray();

                if (isNewInherent)
                {
                    keepGroupingInfo = null;
                    // Debug.Log($"set lastGroupInfo to null");
                    // lastGroupInfo = null;
                    preAbsGroupBy = null;
                }

                RendererGroupInfo useGroupInfo = keepGroupingInfo;

                bool stopGrouping = false;

                IEnumerable<SaintsFieldWithRenderer> playaAndRenderers = GetPlayaAndRenderer(saintsFieldWithInfo, serializedObject, makeRenderer);
                List<ISaintsLayoutToggle> layoutToggles = new List<ISaintsLayoutToggle>();

                foreach (SaintsFieldWithRenderer rendererInfo in playaAndRenderers)
                {
                    switch (rendererInfo.Playa)
                    {
                        case ISaintsLayoutToggle layoutToggle:
                            layoutToggles.Add(layoutToggle);
                            break;
                        case LayoutEndAttribute layoutEndAttribute:
                        {
                            // does not work with toggles, just clear it
                            if(layoutToggles.Count > 0)
                            {
                                Debug.LogWarning($"layout toggles does not work with LayoutEnd. Please adjust the order of the attributes. ({string.Join(", ", layoutToggles)})");
                                layoutToggles.Clear();
                            }

                            string endGroupBy = layoutEndAttribute.LayoutBy;
                            if (endGroupBy == null)
                            {
                                useGroupInfo = null;
                                stopGrouping = true;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                Debug.Log($"Layout close null");
#endif
                            }
                            else if (keepGroupingInfo == null)
                            {
                                // do nothing. End a layout when it's not in a layout is meaningless
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                Debug.Log($"Layout close with no scoop inside");
#endif
                            }
                            else
                            {
                                if (endGroupBy.StartsWith("."))
                                {
                                    string closeGroup = JoinGroupBy(keepGroupingInfo.AbsGroupBy, endGroupBy);
                                    if(closeGroup.Contains('/'))
                                    {
                                        List<string> splitCloseGroup = closeGroup.Split('/').ToList();
                                        splitCloseGroup.RemoveAt(splitCloseGroup.Count - 1);
                                        string openGroupTo = string.Join("/", splitCloseGroup);
                                        if (!rootToRendererGroupInfo.TryGetValue(openGroupTo,
                                                out keepGroupingInfo))
                                        {
                                            rootToRendererGroupInfo[openGroupTo] = keepGroupingInfo = new RendererGroupInfo
                                            {
                                                AbsGroupBy = openGroupTo,
                                                Children = new List<RendererGroupInfo>(),
                                                Config = new SaintsRendererGroup.Config(),
                                                Target = saintsFieldWithInfo.Target,
                                            };
                                        }

                                        useGroupInfo = keepGroupingInfo;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"keepGroupingInfo `{keepGroupingInfo.AbsGroupBy}`");
#endif
                                        stopGrouping = !useGroupInfo.Config.KeepGrouping;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Layout close, {closeGroup}->{openGroupTo}: {keepGroupingInfo?.AbsGroupBy}");
#endif
                                    }
                                    else
                                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Layout close, {closeGroup}: null");
#endif
                                        useGroupInfo = null;
                                        stopGrouping = true;
                                    }

                                    // Debug.Log($"closeGroup={closeGroup}; endGroupBy={endGroupBy}; cur={string.Join(",", rootToRendererGroupInfo.Keys)}");
                                }
                                else
                                {
                                    string parentGroupBy;
                                    if (endGroupBy.Contains('/'))
                                    {
                                        List<string> endGroupBySplit = endGroupBy.Split('/').ToList();
                                        endGroupBySplit.RemoveAt(endGroupBySplit.Count - 1);
                                        parentGroupBy = string.Join("/", endGroupBySplit);
                                    }
                                    else
                                    {
                                        parentGroupBy = "";
                                    }

                                    // Debug.Log($"parentGroupBy={parentGroupBy}/{endGroupBy}");

                                    if (parentGroupBy != "" && rootToRendererGroupInfo.TryGetValue(parentGroupBy,
                                            out RendererGroupInfo info))
                                    {
                                        keepGroupingInfo = useGroupInfo = info.Config.KeepGrouping
                                            ? info
                                            : null;

                                        stopGrouping = !info.Config.KeepGrouping;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Layout close, {endGroupBy}->{parentGroupBy}: {keepGroupingInfo?.AbsGroupBy}");
#endif
                                    }
                                    else
                                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Layout close, {endGroupBy}: null");
#endif
                                        keepGroupingInfo = useGroupInfo = null;
                                        stopGrouping = true;
                                    }
                                }
                            }
                        }
                            break;

                        case ISaintsLayout saintsGroup:
                        {
                            // Debug.Log(saintsGroup);
                            string groupBy = saintsGroup.LayoutBy;
                            if (groupBy.StartsWith("."))
                            {
                                string preGroupBy = keepGroupingInfo?.AbsGroupBy ?? preAbsGroupBy;
                                if(preGroupBy != null)
                                {
                                    groupBy = JoinGroupBy(preGroupBy, groupBy);
                                }
                            }
                            preAbsGroupBy = groupBy;
                            // Debug.Log($"{saintsGroup}: {groupBy}({saintsGroup.LayoutBy})");

                            (bool newRoot, RendererGroupInfo targetGroup) = GetOrCreateGroupInfo(rootToRendererGroupInfo, groupBy, saintsFieldWithInfo.Target);
                            if (newRoot)
                            {
                                // Debug.Log($"new root {saintsGroup}: {groupBy}({saintsGroup.LayoutBy})");
                                rendererGroupInfos.Add(targetGroup);
                            }
                            // lastGroupInfo = targetGroup;
                            // Debug.Log($"set lastGroupInfo to {targetGroup.AbsGroupBy}");

                            SaintsRendererGroup.Config newConfig = new SaintsRendererGroup.Config
                            {
                                ELayout = saintsGroup.Layout,
                                IsDoTween = saintsGroup is DOTweenPlayAttribute,
                                MarginTop = saintsGroup.MarginTop,
                                MarginBottom = saintsGroup.MarginBottom,
                            };
                            SaintsRendererGroup.Config oldConfig = targetGroup.Config;
                            targetGroup.Config = new SaintsRendererGroup.Config
                            {
                                ELayout = newConfig.ELayout == 0? oldConfig.ELayout: newConfig.ELayout,
                                IsDoTween = oldConfig.IsDoTween || newConfig.IsDoTween,
                                MarginTop = newConfig.MarginTop >= 0? newConfig.MarginTop: oldConfig.MarginTop,
                                MarginBottom = newConfig.MarginBottom >= 0? newConfig.MarginBottom: oldConfig.MarginBottom,
                                KeepGrouping = saintsGroup.KeepGrouping,
                                Toggles = (oldConfig?.Toggles ?? Array.Empty<ISaintsLayoutToggle>()).Concat(layoutToggles).ToArray(),
                            };

                            // Debug.Log($"targetGroup={targetGroup.AbsGroupBy}/Conf.Toggle={targetGroup.Config.Toggles.Count}");

                            layoutToggles.Clear();

                            stopGrouping = !targetGroup.Config.KeepGrouping;

                            useGroupInfo = keepGroupingInfo = targetGroup;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                            Debug.Log($"Layout item {groupBy}, newRoot={newRoot}, eLayout={targetGroup.Config.ELayout}, keepGroupingInfo={keepGroupingInfo?.AbsGroupBy}, useGroupInfo={useGroupInfo.AbsGroupBy}");
#endif
                        }
                            break;
                        default:
                        {
                            AbsRenderer renderer = rendererInfo.Renderer;
                            if (renderer != null)
                            {
                                bool isMethod = saintsFieldWithInfo.MethodInfo != null;
                                bool hasNoPlaya = saintsFieldWithInfo.PlayaAttributes.Count == 0;
                                bool shouldDraw = !(isMethod && hasNoPlaya);
                                if(shouldDraw)
                                {
                                    // Debug.Log($"default item {renderer}/{rendererInfo.Playa}");

                                    RendererGroupInfo endNode = new RendererGroupInfo
                                    {
                                        AbsGroupBy = preAbsGroupBy ?? "",
                                        Children = new List<RendererGroupInfo>(),
                                        Config = new SaintsRendererGroup.Config(),
                                        Renderer = renderer,
                                    };

                                    if (useGroupInfo == null)
                                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Add normal field {saintsFieldWithInfo}/{rendererInfo.Playa}/{renderer}");
#endif
                                        rendererGroupInfos.Add(endNode);
                                    }
                                    else
                                    {
                                        useGroupInfo.Children.Add(endNode);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Add to `{useGroupInfo.AbsGroupBy}` group: {saintsFieldWithInfo}/{rendererInfo.Playa}; total={useGroupInfo.Children.Count}");
#endif
                                    }
                                }
                            }
                        }
                            break;
                    }
                }

                if (stopGrouping)
                {
                    keepGroupingInfo = null;
                    preAbsGroupBy = null;
                }

//                 if (lastGroupInfo == null && keepGroupingInfo != null)
//                 {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                     Debug.Log($"Layout lastGroupInfo set to keepGrouping: {keepGroupingInfo.AbsGroupBy}");
// #endif
//                     lastGroupInfo = keepGroupingInfo;
//                 }
//
//                 if (lastGroupInfo != null)
//                 {
//                     Debug.Log($"Add `{lastGroupInfo.AbsGroupBy}` to final list");
//                     rendererGroupInfos.Add(lastGroupInfo);
//                 }


//                 if (layouts.Count > 0)
//                 {
//                     string preAbsGroupBy = null;
//                     List<ISaintsLayoutToggle> layoutToggles = new List<ISaintsLayoutToggle>();
//
//                     foreach (ISaintsLayoutBase layoutBase in layoutBases)
//                     {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                         Debug.Log($"Layout processing {layoutBase}");
// #endif
//
//                         switch (layoutBase)
//                         {
//                             case ISaintsLayoutToggle layoutToggle:
//                                 layoutToggles.Add(layoutToggle);
//                                 break;
//                             case LayoutEndAttribute layoutEndAttribute:
//                             {
//                                 // does not work with toggles, just clear it
//                                 if(layoutToggles.Count > 0)
//                                 {
//                                     Debug.LogWarning($"layout toggles does not work with LayoutEnd. Please adjust the order of the attributes. ({string.Join(", ", layoutToggles)})");
//                                     layoutToggles.Clear();
//                                 }
//
//                                 string endGroupBy = layoutEndAttribute.LayoutBy;
//                                 if (endGroupBy == null)
//                                 {
//                                     keepGroupingInfo = null;
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                     Debug.Log($"Layout close null");
// #endif
//                                 }
//                                 else if (keepGroupingInfo == null)
//                                 {
//                                     // do nothing. End a layout when it's not in a layout is meaningless
//
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                     Debug.Log($"Layout close with no scoop inside");
// #endif
//                                 }
//                                 else
//                                 {
//                                     if (endGroupBy.StartsWith("."))
//                                     {
//                                         string closeGroup = JoinGroupBy(keepGroupingInfo.AbsGroupBy, endGroupBy);
//                                         if(closeGroup.Contains('/'))
//                                         {
//                                             List<string> splitCloseGroup = closeGroup.Split('/').ToList();
//                                             splitCloseGroup.RemoveAt(splitCloseGroup.Count - 1);
//                                             string openGroupTo = string.Join("/", splitCloseGroup);
//                                             if (!rootToRendererGroupInfo.TryGetValue(openGroupTo,
//                                                     out RendererGroupInfo info))
//                                             {
//                                                 rootToRendererGroupInfo[openGroupTo] = info = new RendererGroupInfo
//                                                 {
//                                                     AbsGroupBy = openGroupTo,
//                                                     Children = new List<RendererGroupInfo>(),
//                                                     Config = new SaintsRendererGroup.Config(),
//                                                 };
//                                             }
//
//                                             keepGroupingInfo = info.Config.KeepGrouping ? info : null;
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                             Debug.Log($"Layout close, {closeGroup}->{openGroupTo}: {keepGroupingInfo?.AbsGroupBy}");
// #endif
//                                         }
//                                         else
//                                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                             Debug.Log($"Layout close, {closeGroup}: null");
// #endif
//                                             keepGroupingInfo = null;
//                                         }
//                                     }
//                                     else
//                                     {
//                                         string parentGroupBy;
//                                         if (endGroupBy.Contains('/'))
//                                         {
//                                             List<string> endGroupBySplit = endGroupBy.Split('/').ToList();
//                                             endGroupBySplit.RemoveAt(endGroupBySplit.Count - 1);
//                                             parentGroupBy = string.Join("/", endGroupBySplit);
//                                         }
//                                         else
//                                         {
//                                             parentGroupBy = "";
//                                         }
//                                         if (parentGroupBy != "" && rootToRendererGroupInfo.TryGetValue(parentGroupBy,
//                                                 out RendererGroupInfo info))
//                                         {
//                                             keepGroupingInfo = info.Config.KeepGrouping
//                                                 ? info
//                                                 : null;
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                             Debug.Log($"Layout close, {endGroupBy}->{parentGroupBy}: {keepGroupingInfo?.AbsGroupBy}");
// #endif
//                                         }
//                                         else
//                                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                             Debug.Log($"Layout close, {endGroupBy}: null");
// #endif
//                                             keepGroupingInfo = null;
//                                         }
//                                     }
//                                 }
//                             }
//                                 break;
//
//                             case ISaintsLayout saintsGroup:
//                             {
//                                 string groupBy = saintsGroup.LayoutBy;
//                                 if (groupBy.StartsWith("."))
//                                 {
//                                     string preGroupBy = keepGroupingInfo?.AbsGroupBy ?? preAbsGroupBy;
//                                     if(preGroupBy != null)
//                                     {
//                                         groupBy = JoinGroupBy(preGroupBy, groupBy);
//                                     }
//                                 }
//                                 preAbsGroupBy = groupBy;
//                                 // Debug.Log($"{saintsGroup}: {groupBy}({saintsGroup.LayoutBy})");
//
//                                 (bool newRoot, RendererGroupInfo targetGroup) = GetOrCreateGroupInfo(rootToRendererGroupInfo, groupBy);
//                                 if (newRoot)
//                                 {
//                                     rendererGroupInfos.Add(targetGroup);
//                                 }
//                                 lastGroupInfo = targetGroup;
//
//                                 SaintsRendererGroup.Config newConfig = new SaintsRendererGroup.Config
//                                 {
//                                     ELayout = saintsGroup.Layout,
//                                     IsDoTween = saintsGroup is DOTweenPlayAttribute,
//                                     MarginTop = saintsGroup.MarginTop,
//                                     MarginBottom = saintsGroup.MarginBottom,
//                                 };
//                                 SaintsRendererGroup.Config oldConfig = targetGroup.Config;
//                                 targetGroup.Config = new SaintsRendererGroup.Config
//                                 {
//                                     ELayout = newConfig.ELayout == 0? oldConfig.ELayout: newConfig.ELayout,
//                                     IsDoTween = oldConfig.IsDoTween || newConfig.IsDoTween,
//                                     MarginTop = newConfig.MarginTop >= 0? newConfig.MarginTop: oldConfig.MarginTop,
//                                     MarginBottom = newConfig.MarginBottom >= 0? newConfig.MarginBottom: oldConfig.MarginBottom,
//                                     KeepGrouping = saintsGroup.KeepGrouping,
//                                     Toggles = (oldConfig?.Toggles ?? Array.Empty<ISaintsLayoutToggle>()).Concat(layoutToggles).ToArray(),
//                                 };
//                                 layoutToggles.Clear();
//
//                                 if (targetGroup.Config.KeepGrouping)
//                                 {
//                                     keepGroupingInfo = targetGroup;
//                                 }
//                                 else if (keepGroupingInfo != null &&
//                                          targetGroup.AbsGroupBy != keepGroupingInfo.AbsGroupBy)
//                                 {
//                                     keepGroupingInfo = null;
//                                 }
//
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                                 Debug.Log($"Layout item {groupBy}, newRoot={newRoot}, eLayout={targetGroup.Config.ELayout}, keepGroupingInfo={keepGroupingInfo?.AbsGroupBy}");
// #endif
//                             }
//                                 break;
//                         }
//                     }
//                 }
//
//                 RendererGroupInfo endNode = new RendererGroupInfo
//                 {
//                     AbsGroupBy = "",
//                     Children = new List<RendererGroupInfo>(),
//                     Config = new SaintsRendererGroup.Config(),
//                     FieldWithInfo = saintsFieldWithInfo,
//                 };
//
//                 if (lastGroupInfo == null && keepGroupingInfo != null)
//                 {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                     Debug.Log($"Layout lastGroupInfo set to keepGrouping: {keepGroupingInfo.AbsGroupBy}");
// #endif
//                     lastGroupInfo = keepGroupingInfo;
//                 }
//
//                 if (lastGroupInfo == null)
//                 {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                     Debug.Log($"Layout add direct: {saintsFieldWithInfo.FieldInfo?.Name ?? saintsFieldWithInfo.PropertyInfo?.Name ?? saintsFieldWithInfo.MethodInfo?.Name}");
// #endif
//                     rendererGroupInfos.Add(endNode);
//                 }
//                 else
//                 {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                     Debug.Log($"Layout add field under {lastGroupInfo.AbsGroupBy}: {saintsFieldWithInfo.FieldInfo?.Name ?? saintsFieldWithInfo.PropertyInfo?.Name ?? saintsFieldWithInfo.MethodInfo?.Name}");
// #endif
//                     lastGroupInfo.Children.Add(endNode);
//                     // if (!lastGroupInfo.AbsGroupBy.Contains('/') && !rendererGroupInfos.Contains(lastGroupInfo))
//                     // {
//                     //     rendererGroupInfos.Add(lastGroupInfo);
//                     // }
//                 }
            }

            // Debug.Log($"return rendererGroupInfos {rendererGroupInfos.Count}");

            return rendererGroupInfos;
        }

        private readonly struct SaintsFieldWithRenderer
        {
            public readonly IPlayaAttribute Playa;
            public readonly AbsRenderer Renderer;

            public SaintsFieldWithRenderer(IPlayaAttribute playa, AbsRenderer renderer)
            {
                Playa = playa;
                Renderer = renderer;
            }
        }

        private static IEnumerable<SaintsFieldWithRenderer> GetPlayaAndRenderer(SaintsFieldWithInfo fieldWithInfo, SerializedObject serializedObject, IMakeRenderer makeRenderer)
        {
            return WrapAroundSaintsRenderer(makeRenderer.MakeRenderer(serializedObject, fieldWithInfo), fieldWithInfo,
                serializedObject);
        }

        private static IEnumerable<SaintsFieldWithRenderer> WrapAroundSaintsRenderer(AbsRenderer baseRenderer, SaintsFieldWithInfo fieldWithInfo, SerializedObject serializedObject)
        {
            List<SaintsFieldWithRenderer> postRenderer = new List<SaintsFieldWithRenderer>();

            foreach (IPlayaAttribute playaAttribute in fieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case PlayaInfoBoxAttribute playaInfoBoxAttribute:
                    {
                        PlayaInfoBoxRenderer infoBoxRenderer = new PlayaInfoBoxRenderer(serializedObject, fieldWithInfo, playaInfoBoxAttribute);

                        SaintsFieldWithRenderer playaInfoBoxRenderer =
                            new SaintsFieldWithRenderer(playaInfoBoxAttribute, infoBoxRenderer);
                        if (playaInfoBoxAttribute.Below)
                        {
                            postRenderer.Add(playaInfoBoxRenderer);
                        }
                        else
                        {
                            yield return playaInfoBoxRenderer;
                        }
                    }
                        break;
                    case PlayaSeparatorAttribute playaSeparatorAttribute:
                    {
                        PlayaSeparatorRenderer separatorRenderer = new PlayaSeparatorRenderer(serializedObject, fieldWithInfo, playaSeparatorAttribute);
                        SaintsFieldWithRenderer separatorFieldWithRenderer =
                            new SaintsFieldWithRenderer(playaSeparatorAttribute, separatorRenderer);
                        if (playaSeparatorAttribute.Below)
                        {
                            postRenderer.Add(separatorFieldWithRenderer);
                        }
                        else
                        {
                            yield return separatorFieldWithRenderer;
                        }
                    }
                        break;
                    case LayoutTerminateHereAttribute _:
                    {
                        postRenderer.Add(new SaintsFieldWithRenderer(new LayoutEndAttribute(), null));
                    }
                        break;
                    case LayoutCloseHereAttribute _:  // [Layout(".", keepGrouping: false), LayoutEnd(".")]
                    {
                        // yield return new SaintsFieldWithRenderer(fieldWithInfo, new LayoutAttribute(".", keepGrouping: false), null);
                        postRenderer.Add(new SaintsFieldWithRenderer(new LayoutEndAttribute("."), null));
                    }
                        break;
                    default:
                        yield return new SaintsFieldWithRenderer(playaAttribute, null);
                        break;
                }
            }

            if(baseRenderer != null)
            {
                yield return new SaintsFieldWithRenderer(null, baseRenderer);
            }
            foreach (SaintsFieldWithRenderer posRenderer in postRenderer)
            {
                yield return posRenderer;
            }
        }

        private static (bool newRoot, RendererGroupInfo rendererGroupInfo) GetOrCreateGroupInfo(Dictionary<string, RendererGroupInfo> rootToRendererGroupInfo, string path, object target)
        {
            bool newRoot = false;
            if (!path.Contains('/'))
            {
                if(!rootToRendererGroupInfo.TryGetValue(path, out RendererGroupInfo info))
                {
                    newRoot = true;
                    rootToRendererGroupInfo[path] = info = new RendererGroupInfo
                    {
                        AbsGroupBy = path,
                        Children = new List<RendererGroupInfo>(),
                        Config = new SaintsRendererGroup.Config(),
                        Target = target,
                    };
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                    Debug.Log($"Layout add node root default for {path}");
#endif
                }

                return (newRoot, info);
            }

            string[] groupByParts = path.Split('/');
            string rootGroup = groupByParts[0];
            if (!rootToRendererGroupInfo.TryGetValue(rootGroup, out RendererGroupInfo accInfo))
            {
                newRoot = true;
                rootToRendererGroupInfo[rootGroup] = accInfo = new RendererGroupInfo
                {
                    AbsGroupBy = rootGroup,
                    Children = new List<RendererGroupInfo>(),
                    Config = new SaintsRendererGroup.Config(),
                    Target = target,
                };


#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                Debug.Log($"Layout add node root default for {rootGroup}");
#endif
            }

            string pathAcc = rootGroup;

            foreach (string part in groupByParts.Skip(1))
            {
                pathAcc += $"/{part}";
                RendererGroupInfo found = accInfo.Children.FirstOrDefault(each => each.AbsGroupBy == pathAcc);
                if (found == null)
                {
                    found = new RendererGroupInfo
                    {
                        AbsGroupBy = pathAcc,
                        Children = new List<RendererGroupInfo>(),
                        Config = new SaintsRendererGroup.Config(),
                        Target = target,
                    };
                    accInfo.Children.Add(found);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                    Debug.Log($"Layout add node child default {pathAcc} under {accInfo.AbsGroupBy}");
#endif
                }
                accInfo = found;
            }

            return (newRoot, accInfo);
        }

        private static string JoinGroupBy(string layoutGroupByAcc, string curGroupBy)
        {
            List<string> ori = layoutGroupByAcc.Split('/').ToList();

            foreach (string eachPart in curGroupBy.Split('/'))
            {
                switch (eachPart)
                {
                    case ".":
                        break;
                    case "..":
                        if (ori.Count > 0)
                        {
                            ori.RemoveAt(ori.Count - 1);
                        }

                        break;
                    default:
                        ori.Add(eachPart);
                        break;
                }
            }

            return ori.Count == 0? "": string.Join("/", ori);
        }

        private static IEnumerable<(string parentGroupBy, string subGroupBy)> ChunkGroupBy(string longestGroupGroupBy)
        {
            // e.g "a/b/c/d"
            // first yield: "a/b/c", "a/b/c/d"
            // then yield: "a/b", "a/b/c"
            // then yield: "a", "a/b"
            string[] groupChunk = longestGroupGroupBy.Split('/');

            for (int i = groupChunk.Length - 1; i > 0; i--)
            {
                yield return (string.Join("/", groupChunk, 0, i), string.Join("/", groupChunk, 0, i + 1));
            }
        }

        public static IEnumerable<string> GetSerializedProperties(SerializedObject serializedObject)
        {
            // outSerializedProperties.Clear();
            // ReSharper disable once ConvertToUsingDeclaration
            using (SerializedProperty iterator = serializedObject.GetIterator())
            {
                // ReSharper disable once InvertIf
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        // outSerializedProperties.Add(serializedObject.FindProperty(iterator.name));
                        yield return iterator.name;
                    } while (iterator.NextVisible(false));
                }
            }
        }

        // private static void SetupRendererGroup(ISaintsRendererGroup saintsRendererGroup, LayoutInfo layoutInfo)
        // {
        //     ISaintsRendererGroup group = MakeRendererGroup(layoutInfo);
        //     saintsRendererGroup.Add(group);
        //     foreach (KeyValuePair<string, LayoutInfo> kv in layoutInfo.Children)
        //     {
        //         Debug.Log($"add sub group {kv.Key}({kv.Value.Config})");
        //         SetupRendererGroup(group, kv.Value);
        //     }
        // }

        // private static ISaintsRendererGroup MakeRendererGroup(LayoutInfo layoutInfo)
        // {
        //     if (layoutInfo.Config.HasFlagFast(ELayout.Vertical))
        //     {
        //         return new VerticalGroup(layoutInfo.Config);
        //     }
        //     return new HorizontalGroup(layoutInfo.Config);
        // }
        // private static ISaintsRendererGroup MakeRendererGroup(ELayout layoutInfo)
        // {
        //     if (layoutInfo.HasFlagFast(ELayout.Tab))
        //     {
        //         return new SaintsRendererGroup(layoutInfo);
        //     }
        //     if (layoutInfo.HasFlagFast(ELayout.Horizontal))
        //     {
        //         return new HorizontalGroup(layoutInfo);
        //     }
        //     return new VerticalGroup(layoutInfo);
        // }

        public static AbsRenderer HelperMakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            // Debug.Log($"field {fieldWithInfo.fieldInfo?.Name}/{fieldWithInfo.fieldInfo?.GetCustomAttribute<ExtShowHideConditionBase>()}");
            switch (fieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                {
                    foreach (IPlayaAttribute playaAttribute in fieldWithInfo.PlayaAttributes)
                    {
                        switch (playaAttribute)
                        {
                            case TableAttribute _:
                                return new TableRenderer(serializedObject, fieldWithInfo);

                            case ListDrawerSettingsAttribute _:
                                return new ListDrawerSettingsRenderer(serializedObject, fieldWithInfo);
                        }
                    }

                    return new SerializedFieldRenderer(serializedObject, fieldWithInfo);
                }
                case SaintsRenderType.InjectedSerializedField:
                    return new SerializedFieldBareRenderer(serializedObject, fieldWithInfo);

                case SaintsRenderType.NonSerializedField:
                case SaintsRenderType.NativeProperty:
                    return new NativeFieldPropertyRenderer(serializedObject, fieldWithInfo);

                case SaintsRenderType.Method:
                    return new MethodRenderer(serializedObject, fieldWithInfo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.RenderType), fieldWithInfo.RenderType, null);
            }
        }

        public virtual AbsRenderer MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            return HelperMakeRenderer(so, fieldWithInfo);
        }
    }
}
