using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.ComponentHeader;
using SaintsField.Editor.Core;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Playa.Renderer.ButtonCustomContextMenuFakeRenderer;
using SaintsField.Editor.Playa.Renderer.ButtonFakeRenderer;
using SaintsField.Editor.Playa.Renderer.DecoratorRenderer;
using SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer;
using SaintsField.Editor.Playa.Renderer.ListDrawerSettings;
using SaintsField.Editor.Playa.Renderer.MethodBindFakeRenderer;
using SaintsField.Editor.Playa.Renderer.OnValueChangedCollectionFakeRenderer;
using SaintsField.Editor.Playa.Renderer.PlayaFullWidthRichLabelFakeRenderer;
using SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer;
using SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer;
using SaintsField.Editor.Playa.Renderer.RealTimeCalculatorFakeRenderer;
using SaintsField.Editor.Playa.Renderer.ShowInInspectorFieldFakeRenderer;
using SaintsField.Editor.Playa.Renderer.Table;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
#if SAINTSFIELD_DEBUG
using Unity.Profiling;
#endif
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
using DG.DOTweenEditor;
#endif


namespace SaintsField.Editor
{
    public partial class SaintsEditor: UnityEditor.Editor, IDOTweenPlayRecorder, IMakeRenderer, ISearchable
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static bool _saintsEditorIMGUI = true;
        private SaintsEditorCore _coreEditor;

        // private MonoScript _monoScript;
        // private List<SaintsFieldWithInfo> _fieldWithInfos = new List<SaintsFieldWithInfo>();

        [NonSerialized]
        public bool EditorShowMonoScript = true;

#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
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
            IReadOnlyList<object> targets)
        {
            string[] serFields = GetSerializedProperties(serializedObject).ToArray();
            Dictionary<string, SerializedProperty> serializedPropertyDict = serFields
                .Where(each => !skipSerializedFields.Contains(each))
                .ToDictionary(each => each, serializedObject.FindProperty);
            // Debug.Log($"serializedPropertyDict.Count={serializedPropertyDict.Count}");
            // return HelperGetRenderers(serializedPropertyDict, saintsSerializedProp, serializedObject, makeRenderer, targets);
            return HelperGetRenderers(serializedPropertyDict, serializedObject, makeRenderer,  null, null, -1, targets);
        }

        public static IEnumerable<SaintsFieldWithInfo> HelperGetSaintsFieldWithInfo(
            SerializedObject serializedObject,
            IReadOnlyDictionary<string, SerializedProperty> serializedPropertyDict,
            object targetParent,
            MemberInfo targetMemberInfo,
            int targetMemberIndex,
            IReadOnlyList<object> targets)
        {
#if SAINTSFIELD_DEBUG
            using ProfilerMarker.AutoScope autoMarker = new ProfilerMarker("HelperGetSaintsFieldWithInfo").Auto();
#endif
            List<SaintsFieldWithInfo> fieldWithInfos = new List<SaintsFieldWithInfo>();


            // Dictionary<string, SerializedProperty> pendingSerializedProperties = new Dictionary<string, SerializedProperty>(serializedPropertyDict);

            Dictionary<string, SerializedProperty> pendingSerializedProperties = serializedPropertyDict.ToDictionary(
                static each => each.Key,
                static each => each.Value);
            pendingSerializedProperties.Remove("m_Script");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
            Debug.Log($"serializedPropertyDict: {string.Join(", ", serializedPropertyDict.Keys)}");
#endif

            List<Type> types = new List<Type>();
            if (targets.Count == 0 || targets.All(RuntimeUtil.IsNull))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning("Target is null, use fallback workaround, #200");
#endif
                // do nothing
            }
            else
            {
                object target = targets[0];
                types = ReflectUtils.GetSelfAndBaseTypesFromInstance(target);
                types.Reverse();
                // base type -> this type
                // a later field should override current in different depth
                // but, if the field is not in the same depth, it should be added (method override)
                // Yep, C# is a crap
                for (int inherentDepth = 0; inherentDepth < types.Count; inherentDepth++)
                {
                    Type systemType = types[inherentDepth];
                    // Debug.Log($"{inherentDepth}: {systemType}");
                    // if (systemType == typeof(UnityEngine.Component) ||
                    //     systemType == typeof(UnityEngine.ScriptableObject) ||
                    //     systemType == typeof(UnityEngine.MonoBehaviour))
                    // {
                    //     continue;
                    // }
                    IPlayaClassAttribute[] playaClassAttributes = ReflectCache.GetTypeCustomAttributes<IPlayaClassAttribute>(systemType, false);

                    IPlayaClassAttribute[] startClassAttributes = playaClassAttributes.Where(static each => !each.EndDecorator).ToArray();
                    if (startClassAttributes.Length > 0)
                    {
                        // Debug.Log($"Add start for systemType {systemType}={string.Join<IPlayaClassAttribute>(", ", playaClassAttributes)}");
                        fieldWithInfos.Add(new SaintsFieldWithInfo
                        {
                            InherentDepth = inherentDepth,
                            // Order = int.MinValue,
                            PlayaAttributes = startClassAttributes,
                            TargetParent = targetParent,
                            TargetMemberInfo = targetMemberInfo,
                            TargetMemberIndex = targetMemberIndex,
                            Targets = targets,
                            RenderType = SaintsRenderType.ClassStruct,
                            MemberId = "StartClassStruct",
                            FieldInfo = null,
                            MethodInfo = null,
                            PropertyInfo = null,
                            ClassStructType = systemType,
                        });
                    }

                    // as we can not get the correct order, we'll make it order as: field(serialized+nonSerialized), property, method
                    List<SaintsFieldWithInfo> thisDepthInfos = new List<SaintsFieldWithInfo>();
                    List<string> memberDepthIds = new List<string>();

                    // MemberInfoComparerPreParsed comparison is of complexity O(YES) and urgently needs refactoring
                    IComparer<MemberInfo> memberOrderComparer = MemberInfoComparerPreParsed.GetComparer(systemType);

// #if SAINTSFIELD_CODE_ANALYSIS
//                     memberOrderComparer ??= MemberInfoComparerCodeAnalysis.GetComparer(systemType);
// #endif

                    memberOrderComparer ??= new MemberInfoComparerReflection();


                    MemberInfo[] members = systemType
                        .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                    BindingFlags.Public | BindingFlags.DeclaredOnly);

                    OrderedParallelQuery<MemberInfo> memberLis = members.AsParallel().OrderBy(static memberInfo => memberInfo, memberOrderComparer);

// #if SAINTSFIELD_CODE_ANALYSIS
                    // memberLis.Sort((a, b) => MemberLisCompare(a, b, codeAnalysisMembers));
// #endif

                    //
                    Dictionary<MemberInfo, IPlayaAttribute[]> memberInfoToPlaya =
                        CollectionPool<Dictionary<MemberInfo, IPlayaAttribute[]>, KeyValuePair<MemberInfo, IPlayaAttribute[]>>.Get();
                    List<MemberInfo> usedMemberInfos = CollectionPool<List<MemberInfo>, MemberInfo>.Get();
                    Dictionary<string, MemberInfo> saintsSerializedActualNameToMemberInfo =
                        CollectionPool<Dictionary<string, MemberInfo>, KeyValuePair<string, MemberInfo>>.Get();

                    try
                    {
                        foreach (MemberInfo memberInfo in memberLis)
                        {
                            IPlayaAttribute[] playaAttributes = ReflectCache.GetCustomAttributes<IPlayaAttribute>(memberInfo);
                            SaintsSerializedActualAttribute saintsSerializedActualAttribute =
                                playaAttributes.OfType<SaintsSerializedActualAttribute>().FirstOrDefault();
                            if (saintsSerializedActualAttribute == null)
                            {
                                memberInfoToPlaya[memberInfo] = playaAttributes;
                                usedMemberInfos.Add(memberInfo);
                            }
                            else
                            {
                                saintsSerializedActualNameToMemberInfo[saintsSerializedActualAttribute.Name] = memberInfo;

                                pendingSerializedProperties.Remove(memberInfo.Name);
                                pendingSerializedProperties.Remove(RuntimeUtil.GetAutoPropertyName(memberInfo.Name));

#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                                Debug.Log($"remove {memberInfo.Name} from pendingSer and put {saintsSerializedActualAttribute.Name} as actual serialize field");
#endif
                            }
                        }

                        // foreach (KeyValuePair<MemberInfo, IPlayaAttribute[]> kv in memberInfoToPlaya)
                        foreach (MemberInfo memberInfo in usedMemberInfos)
                        {
                            // Debug.Log($"{systemType}: {memberInfo.Name}/{memberInfo.MemberType}");
                            // MemberInfo memberInfo = kv.Key;
                            // IReadOnlyList<IPlayaAttribute> playaAttributes = kv.Value;
                            IReadOnlyList<IPlayaAttribute> playaAttributes = memberInfoToPlaya[memberInfo];
                            // IReadOnlyList<IPlayaAttribute> playaAttributes =
                            //     ReflectCache.GetCustomAttributes<IPlayaAttribute>(memberInfo);

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

                                        // OrderedAttribute orderProp =
                                        //     playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                                        // int order = orderProp?.Order ?? int.MinValue;

                                        // Debug.Log($"{fieldInfo.Name}/{string.Join(",", pendingSerializedProperties.Keys)}");
                                        thisDepthInfos.Add(new SaintsFieldWithInfo
                                        {
                                            ClassStructType = systemType,
                                            PlayaAttributes = playaAttributes,
                                            // PlayaAttributesQueue = playaAttributes,
                                            // LayoutBases = layoutBases,
                                            TargetParent      = targetParent,
                                            TargetMemberInfo  = targetMemberInfo,
                                            TargetMemberIndex = targetMemberIndex,
                                            Targets           = targets,
                                            AttributeMemberInfo = fieldInfo,

                                            RenderType         = SaintsRenderType.SerializedField,
                                            SerializedProperty = serializedPropertyDict[fieldInfo.Name],
                                            MemberId           = fieldInfo.Name,
                                            FieldInfo          = fieldInfo,
                                            InherentDepth      = inherentDepth,
                                            // Order = order,
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
                                        SaintsSerializedAttribute saintsSerializedAttribute = null;
                                        // OrderedAttribute orderProp = null;
                                        foreach (IPlayaAttribute playa in playaAttributes)
                                        {
                                            if (playa is SaintsSerializedAttribute ssa)
                                            {
                                                saintsSerializedAttribute = ssa;
                                            }

                                            if(saintsSerializedAttribute != null)
                                            {
                                                break;
                                            }
                                        }
                                        // int order = orderProp?.Order ?? int.MinValue;

                                        if(saintsSerializedAttribute == null)
                                        {
                                            thisDepthInfos.Add(new SaintsFieldWithInfo
                                            {
                                                ClassStructType = systemType,
                                                PlayaAttributes = playaAttributes,
                                                // PlayaAttributesQueue = playaAttributes,
                                                // LayoutBases = layoutBases,
                                                TargetParent      = targetParent,
                                                TargetMemberInfo  = targetMemberInfo,
                                                TargetMemberIndex = targetMemberIndex,
                                                Targets           = targets,
                                                AttributeMemberInfo = fieldInfo,

                                                RenderType = SaintsRenderType.NonSerializedField,
                                                // memberType = nonSerFieldInfo.MemberType,
                                                MemberId      = fieldInfo.Name,
                                                FieldInfo     = fieldInfo,
                                                InherentDepth = inherentDepth,
                                                // Order = order,
                                                // serializable = false,
                                            });
                                        }
                                        else
                                        {
                                            string thisName = SerializedUtils.TrimKBackingField(fieldInfo.Name);
                                            if (!saintsSerializedActualNameToMemberInfo.TryGetValue(thisName, out MemberInfo serInfo))
                                            {
                                                Debug.LogWarning($"failed to find serialized actual field for {fieldInfo.Name}");
                                                continue;
                                            }

                                            // Attribute[] injectedAttrs = ReflectCache
                                            //     .GetCustomAttributes(fieldInfo)
                                            //     .Where(each => each is not NonSerializedAttribute
                                            //                    && each is not HideInInspector
                                            //                    && each is not SaintsSerializedAttribute)
                                            //     .Prepend(ReflectCache.GetCustomAttributes<SaintsSerializedActualAttribute>(serInfo).First())
                                            //     .ToArray();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                                            Debug.Log($"wrap {fieldInfo.Name} to {serInfo.Name}");
#endif

                                            // Debug.Log($"keys={string.Join(",", serializedPropertyDict.Keys)}");

                                            thisDepthInfos.Add(new SaintsFieldWithInfo
                                            {
                                                ClassStructType = systemType,
                                                PlayaAttributes = playaAttributes,
                                                // PlayaAttributesQueue = playaAttributes,
                                                // LayoutBases = layoutBases,
                                                TargetParent      = targetParent,
                                                TargetMemberInfo  = targetMemberInfo,
                                                TargetMemberIndex = targetMemberIndex,
                                                Targets           = targets,
                                                AttributeMemberInfo = fieldInfo,

                                                RenderType = SaintsRenderType.SerializedField,
                                                // memberType = nonSerFieldInfo.MemberType,
                                                MemberId      = serInfo.Name,
                                                FieldInfo     = (FieldInfo)serInfo,
                                                InherentDepth = inherentDepth,
                                                // Order = order,
                                                // serializable = false,

                                                SerializedProperty = serializedPropertyDict[serInfo.Name],
                                            });
                                        }
                                        memberDepthIds.Add(fieldInfo.Name);
                                    }

                                    #endregion
                                }
                                    break;
                                case PropertyInfo propertyInfo:
                                {
                                    // Debug.Log(propertyInfo.Name);
                                    #region NativeProperty

                                    if (playaAttributes.Count > 0)
                                    {
                                        // OrderedAttribute orderProp =
                                        //     playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                                        // int order = orderProp?.Order ?? int.MinValue;
                                        thisDepthInfos.Add(new SaintsFieldWithInfo
                                        {
                                            ClassStructType = systemType,
                                            PlayaAttributes = playaAttributes,
                                            // PlayaAttributesQueue = playaAttributes,
                                            // LayoutBases = layoutBases,
                                            TargetParent      = targetParent,
                                            TargetMemberInfo  = targetMemberInfo,
                                            TargetMemberIndex = targetMemberIndex,
                                            Targets           = targets,
                                            AttributeMemberInfo = propertyInfo,

                                            RenderType    = SaintsRenderType.NativeProperty,
                                            MemberId      = propertyInfo.Name,
                                            PropertyInfo  = propertyInfo,
                                            InherentDepth = inherentDepth,
                                            // Order = order,
                                        });
                                        memberDepthIds.Add(propertyInfo.Name);
                                    }

                                    #endregion
                                }
                                    break;
                                case MethodInfo methodInfo:
                                {
                                    // Debug.Log(methodInfo.Name);
                                    #region Method

                                    // method attributes will be collected no matter what, because DOTweenPlayGroup depending on it even
                                    // it has no attribute at all

                                    // Attribute[] allMethodAttributes = methodInfo.GetCustomAttributes<Attribute>().ToArray();

                                    // OrderedAttribute orderProp =
                                    //     playaAttributes.FirstOrDefault(each =>
                                    //         each is OrderedAttribute) as OrderedAttribute;
                                    // int order = orderProp?.Order ?? int.MinValue;

                                    // wrong: inspector does not care about inherited/new method. It just needs to use the last one
                                    // right: we support method override now
                                    // fieldWithInfos.RemoveAll(each => each.InherentDepth < inherentDepth && each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);
                                    // methodInfos.RemoveAll(each => each.InherentDepth < inherentDepth && each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD
                                    Debug.Log($"[{systemType}] method: {methodInfo.Name}");
#endif

                                    string buttonExtraId = string.Join(":", methodInfo.GetParameters()
                                        .Select(static each => each.ParameterType)
                                        .Append(methodInfo.ReturnType)
                                        .Select(static each => each.FullName));

                                    string buttonId = $"{methodInfo.Name}.{buttonExtraId}";

                                    thisDepthInfos.Add(new SaintsFieldWithInfo
                                    {
                                        ClassStructType = systemType,
                                        PlayaAttributes = playaAttributes,
                                        // PlayaAttributesQueue = playaAttributes,
                                        // LayoutBases = layoutBases,
                                        TargetParent      = targetParent,
                                        TargetMemberInfo  = targetMemberInfo,
                                        TargetMemberIndex = targetMemberIndex,
                                        Targets           = targets,
                                        AttributeMemberInfo = methodInfo,

                                        // memberType = MemberTypes.Method,
                                        RenderType    = SaintsRenderType.Method,
                                        MemberId      = buttonId,
                                        MethodInfo    = methodInfo,
                                        InherentDepth = inherentDepth,
                                        // Order = order,
                                    });
                                    memberDepthIds.Add(buttonId);

                                    #endregion
                                }
                                    break;
                                default:
                                {
                                    #region whatever
                                    if (playaAttributes.Count == 0)
                                    {
                                        break;
                                    }

                                    // ReSharper disable once UseNegatedPatternInIsExpression
                                    if (playaAttributes.All(each => !(each is ISaintsLayout)))
                                    {
                                        break;
                                    }

                                    // OrderedAttribute orderProp =
                                    //     playaAttributes.FirstOrDefault(each =>
                                    //         each is OrderedAttribute) as OrderedAttribute;
                                    // int order = orderProp?.Order ?? int.MinValue;

                                    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD
                                    Debug.Log($"[{systemType}] event: {eventInfo.Name}");
                                    #endif
                                    thisDepthInfos.Add(new SaintsFieldWithInfo
                                    {
                                        ClassStructType = systemType,
                                        PlayaAttributes = playaAttributes,
                                        // PlayaAttributesQueue = playaAttributes,
                                        // LayoutBases = layoutBases,
                                        TargetParent      = targetParent,
                                        TargetMemberInfo  = targetMemberInfo,
                                        TargetMemberIndex = targetMemberIndex,
                                        Targets           = targets,
                                        AttributeMemberInfo = memberInfo,

                                        // memberType = MemberTypes.Method,
                                        RenderType    = SaintsRenderType.Other,
                                        MemberId      = $"?:{memberInfo.Name}",
                                        InherentDepth = inherentDepth,
                                        // Order = order,
                                    });
                                    break;
                                    #endregion
                                }
                            }
                        }

                        // now handle overrides
                        fieldWithInfos.RemoveAll(each => memberDepthIds.Contains(each.MemberId));

                        fieldWithInfos.AddRange(thisDepthInfos);
                        // fieldWithInfos.AddRange(fieldInfos);
                        // fieldWithInfos.AddRange(propertyInfos);
                        // fieldWithInfos.AddRange(methodInfos);

                        // Debug.Log($"systemType{systemType}={string.Join<IPlayaClassAttribute>(", ", playaClassAttributes)}");
                        List<IPlayaAttribute> endClassAttributes = playaClassAttributes.Where(static each => each.EndDecorator).Cast<IPlayaAttribute>().ToList();
                        if (endClassAttributes.Count > 0)
                        {
                            endClassAttributes.Insert(0, new LayoutEndAttribute());
                            // Debug.Log($"Add end for systemType {systemType}={string.Join<IPlayaClassAttribute>(", ", playaClassAttributes)}");
                            fieldWithInfos.Add(new SaintsFieldWithInfo
                            {
                                InherentDepth = inherentDepth,
                                // Order = int.MinValue,
                                PlayaAttributes   = endClassAttributes,
                                TargetParent      = targetParent,
                                TargetMemberInfo  = targetMemberInfo,
                                TargetMemberIndex = targetMemberIndex,
                                Targets           = targets,
                                RenderType        = SaintsRenderType.ClassStruct,
                                MemberId          = "EndClassStruct",
                                FieldInfo         = null,
                                MethodInfo        = null,
                                PropertyInfo      = null,
                                ClassStructType   = systemType,
                            });
                        }
                    }
                    finally
                    {
                        // Release rented collections
                        CollectionPool<Dictionary<MemberInfo, IPlayaAttribute[]>, KeyValuePair<MemberInfo, IPlayaAttribute[]>>.Release(memberInfoToPlaya);
                        CollectionPool<Dictionary<string, MemberInfo>, KeyValuePair<string, MemberInfo>>.Release(saintsSerializedActualNameToMemberInfo);
                        CollectionPool<List<MemberInfo>, MemberInfo>.Release(usedMemberInfos);
                    }
                }
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
                        TargetParent = targetParent,
                        TargetMemberInfo = targetMemberInfo,
                        TargetMemberIndex = targetMemberIndex,
                        Targets = targets,

                        RenderType = SaintsRenderType.InjectedSerializedField,
                        SerializedProperty = pendingSer.Value,
                        FieldInfo = null,
                        InherentDepth = types.Count == 0? 0: types.Count - 1,
                        // Order = int.MinValue,
                        // serializable = true,
                    });
                }
            }

            return fieldWithInfos
                .WithIndex()
                .OrderBy(static each => each.value.InherentDepth)
                // .ThenBy(each => each.value.Order)
                .ThenBy(static each => each.index)
                .Select(static each => each.value)
            ;
        }

        public static IReadOnlyList<ISaintsRenderer> HelperGetRenderers(
            IReadOnlyDictionary<string, SerializedProperty> serializedPropertyDict,
            SerializedObject serializedObject,
            IMakeRenderer makeRenderer,
            object targetParent,
            MemberInfo targetMemberInfo,
            int targetMemberIndex,
            IReadOnlyList<object> targets)
        {
            IReadOnlyList<SaintsFieldWithInfo> fieldWithInfosSorted = HelperGetSaintsFieldWithInfo(serializedObject, serializedPropertyDict, targetParent, targetMemberInfo, targetMemberIndex, targets).ToArray();


            if(DrawHeaderGUI.EnsureInitLoad())
            {
                // let's handle some HeaderGUI here... not a good idea but...
                bool anyChange = false;
                AbsComponentHeaderAttribute[] classAttributes =
                    ReflectCache.GetCustomAttributes<AbsComponentHeaderAttribute>(targets[0].GetType());
                foreach ((AbsComponentHeaderAttribute componentHeaderAttribute, int order) in classAttributes.WithIndex(
                             -classAttributes.Length))
                {
                    bool added = DrawHeaderGUI.AddAttributeIfNot(
                        componentHeaderAttribute,
                        null,
                        targets[0],
                        order);
                    if (added)
                    {
                        anyChange = true;
                    }
                }

                foreach ((SaintsFieldWithInfo saintsFieldWithInfo, int index) in fieldWithInfosSorted.WithIndex())
                {
                    IReadOnlyList<IPlayaAttribute> playaAttributes = saintsFieldWithInfo.PlayaAttributes;
                    foreach (AbsComponentHeaderAttribute componentHeaderAttribute in playaAttributes
                                 .OfType<AbsComponentHeaderAttribute>())
                    {
                        bool added = DrawHeaderGUI.AddAttributeIfNot(
                            componentHeaderAttribute,
                            saintsFieldWithInfo.MethodInfo ?? (MemberInfo)saintsFieldWithInfo.FieldInfo ??
                            saintsFieldWithInfo.PropertyInfo,
                            targets[0],
                            index);
                        if (added)
                        {
                            anyChange = true;
                        }
                    }
                }

                if (anyChange)
                {
                    DrawHeaderGUI.RefreshAddAttributeIfNot(targets[0].GetType());
                }
            }

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

        public static ISaintsRenderer MakeRendererForGroupIfNeed(RendererGroupInfo rendererGroupInfo)
        {
            if (rendererGroupInfo.Renderer != null)
            {
                return rendererGroupInfo.Renderer;
            }

            ISaintsRendererGroup group =
#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
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

        public class RendererGroupInfo {
            public string AbsGroupBy;  // ""=normal fields, other=grouped fields
            public List<RendererGroupInfo> Children;
            public SaintsRendererGroup.Config Config;
            public AbsRenderer Renderer;
            public object Target;
        }

        public static IReadOnlyList<RendererGroupInfo> ChainSaintsFieldWithInfo(IReadOnlyList<SaintsFieldWithInfo> fieldWithInfosSorted, SerializedObject serializedObject, IMakeRenderer makeRenderer)
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

                SaintsFieldWithRenderer[] playaAndRenderers = GetPlayaAndRenderer(saintsFieldWithInfo, serializedObject, makeRenderer).ToArray();
                List<ISaintsLayoutToggle> layoutToggles = new List<ISaintsLayoutToggle>();

                foreach (SaintsFieldWithRenderer rendererInfo in playaAndRenderers)
                {
                    // Debug.Log(rendererInfo);
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
                                    IReadOnlyList<string> closeGroupParts = JoinGroupBy(RuntimeUtil.SeparatePath(keepGroupingInfo.AbsGroupBy).ToArray(), RuntimeUtil.SeparatePath(endGroupBy).ToArray());
                                    // string closeGroup;
                                    // if(closeGroup.Contains('/'))
                                    if(closeGroupParts.Count >= 2)
                                    {
                                        // List<string> splitCloseGroup = closeGroup.Split('/').ToList();
                                        List<string> splitCloseGroup = new List<string>(closeGroupParts);
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
                                                Target = saintsFieldWithInfo.Targets[0],
                                            };
                                        }

                                        useGroupInfo = keepGroupingInfo;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"keepGroupingInfo `{keepGroupingInfo.AbsGroupBy}`");
#endif
                                        stopGrouping = !useGroupInfo.Config.KeepGrouping;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Layout close, {string.Join('/', closeGroupParts)}->{openGroupTo}: {keepGroupingInfo?.AbsGroupBy}");
#endif
                                    }
                                    else
                                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                        Debug.Log($"Layout close, {string.Join('/', closeGroupParts)}: null");
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
                                        // List<string> endGroupBySplit = endGroupBy.Split('/').ToList();
                                        List<string> endGroupBySplit = RuntimeUtil.SeparatePath(endGroupBy).ToList();
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
                                    IReadOnlyList<string> joinGroupBy = JoinGroupBy(RuntimeUtil.SeparatePath(preGroupBy).ToArray(), RuntimeUtil.SeparatePath(groupBy).ToArray());
                                    groupBy = string.Join('/', joinGroupBy);
                                }
                            }
                            preAbsGroupBy = groupBy;
                            // Debug.Log($"{saintsGroup}: {groupBy}({saintsGroup.LayoutBy})");

                            (bool newRoot, RendererGroupInfo targetGroup) = GetOrCreateGroupInfo(rootToRendererGroupInfo, groupBy, saintsFieldWithInfo.Targets[0]);
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
                                PaddingLeft = saintsGroup.PaddingLeft,
                                PaddingRight = saintsGroup.PaddingRight,
                            };
                            SaintsRendererGroup.Config oldConfig = targetGroup.Config;
                            targetGroup.Config = new SaintsRendererGroup.Config
                            {
                                ELayout = newConfig.ELayout == 0? oldConfig.ELayout: newConfig.ELayout,
                                IsDoTween = oldConfig.IsDoTween || newConfig.IsDoTween,
                                MarginTop = newConfig.MarginTop >= 0? newConfig.MarginTop: oldConfig.MarginTop,
                                MarginBottom = newConfig.MarginBottom >= 0? newConfig.MarginBottom: oldConfig.MarginBottom,
                                PaddingLeft = Mathf.Approximately(newConfig.PaddingLeft, 0)? oldConfig.PaddingLeft: newConfig.PaddingLeft,
                                PaddingRight = Mathf.Approximately(newConfig.PaddingRight, 0)? oldConfig.PaddingRight: newConfig.PaddingRight,
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
                                // bool isMethod = saintsFieldWithInfo.MethodInfo != null;
                                // bool hasNoPlaya = saintsFieldWithInfo.PlayaAttributes.Count == 0;
                                bool shouldDraw = SaintsFieldInfoShouldDraw(saintsFieldWithInfo);
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
            }

            // Debug.Log($"return rendererGroupInfos {rendererGroupInfos.Count}");

            return rendererGroupInfos;
        }

        public static bool SaintsFieldInfoShouldDraw(SaintsFieldWithInfo saintsFieldWithInfo)
        {
            bool isMethod = saintsFieldWithInfo.MethodInfo != null;
            bool hasNoPlaya = saintsFieldWithInfo.PlayaAttributes.Count == 0;
            bool shouldDraw = !(isMethod && hasNoPlaya);
            return shouldDraw;
        }

        private static IEnumerable<SaintsFieldWithRenderer> GetPlayaAndRenderer(SaintsFieldWithInfo fieldWithInfo, SerializedObject serializedObject, IMakeRenderer makeRenderer)
        {
            foreach (IReadOnlyList<SaintsFieldWithRenderer> saintsFieldWithRenderers in makeRenderer.MakeRenderer(serializedObject, fieldWithInfo))
            {
                foreach (SaintsFieldWithRenderer renderer in saintsFieldWithRenderers)
                {
                    yield return renderer;
                }
            }
        }

        public static IEnumerable<IReadOnlyList<SaintsFieldWithRenderer>> HelperMakeRenderer(
            SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            IReadOnlyList<SaintsFieldWithRenderer> renderers = MakeRendererGroup(null, serializedObject, fieldWithInfo);
            if (renderers.Count > 0)
            {
                yield return renderers;
            }
        }

        public virtual IEnumerable<IReadOnlyList<SaintsFieldWithRenderer>> MakeRenderer(SerializedObject so,
            SaintsFieldWithInfo fieldWithInfo)
        {
            return HelperMakeRenderer(so, fieldWithInfo);
        }

        public static IReadOnlyList<SaintsFieldWithRenderer> WrapAroundSaintsRenderer(
            IReadOnlyList<AbsRenderer> targetRenderers, SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo)
        {
            if (targetRenderers == null)
            {
                throw new ArgumentNullException(nameof(targetRenderers));
            }

            return MakeRendererGroup(targetRenderers, serializedObject, fieldWithInfo);
        }

        private static IReadOnlyList<SaintsFieldWithRenderer> MakeRendererGroup(
            IReadOnlyList<AbsRenderer> customTargetRenderers, SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo)
        {
            List<SaintsFieldWithRenderer> renderers = new List<SaintsFieldWithRenderer>();
            List<SaintsFieldWithRenderer> tailRenderers = new List<SaintsFieldWithRenderer>();
            bool customTarget = customTargetRenderers != null;
            bool targetInserted = false;
            bool hasSerializedTarget = false;

            foreach (Attribute attribute in GetRendererAttributes(fieldWithInfo))
            {
                IPlayaAttribute playaAttribute = attribute as IPlayaAttribute;
                switch (playaAttribute)
                {
                    case OnValueChangedAttribute onValueChangedAttribute:
                        if (fieldWithInfo.SerializedProperty is
                            { propertyType: SerializedPropertyType.Generic, isArray: true })
                        {
                            renderers.Add(new SaintsFieldWithRenderer(onValueChangedAttribute,
                                new OnValueChangedCollectionRenderer(onValueChangedAttribute, serializedObject,
                                    fieldWithInfo)));
                        }
                        continue;
                    case IPlayaMethodBindAttribute methodBindAttribute:
                        renderers.Add(new SaintsFieldWithRenderer(methodBindAttribute as IPlayaAttribute,
                            new MethodBindRenderer(methodBindAttribute, serializedObject, fieldWithInfo)));
                        continue;
                    case InfoBoxAttribute playaInfoBoxAttribute:
                    {
                        SaintsFieldWithRenderer infoBox = new SaintsFieldWithRenderer(playaInfoBoxAttribute,
                            new PlayaInfoBoxRenderer(serializedObject, fieldWithInfo, playaInfoBoxAttribute));
                        (playaInfoBoxAttribute.Below ? tailRenderers : renderers).Add(infoBox);
                        continue;
                    }
                    case BelowTextAttribute playaBelowRichLabelAttribute:
                    {
                        SaintsFieldWithRenderer richLabel = new SaintsFieldWithRenderer(
                            playaBelowRichLabelAttribute,
                            new PlayaFullWidthRichLabelRenderer(serializedObject, fieldWithInfo,
                                playaBelowRichLabelAttribute));
                        (playaBelowRichLabelAttribute.Below ? tailRenderers : renderers).Add(richLabel);
                        continue;
                    }
                    case SeparatorAttribute playaSeparatorAttribute:
                    {
                        SaintsFieldWithRenderer separator = new SaintsFieldWithRenderer(playaSeparatorAttribute,
                            new PlayaSeparatorRenderer(serializedObject, fieldWithInfo, playaSeparatorAttribute));
                        (playaSeparatorAttribute.Below ? tailRenderers : renderers).Add(separator);
                        continue;
                    }
                    case LayoutTerminateHereAttribute _:
                        tailRenderers.Add(new SaintsFieldWithRenderer(new LayoutEndAttribute(), null));
                        continue;
                    case LayoutCloseHereAttribute _:  // [Layout(".", keepGrouping: false), LayoutEnd(".")]
                        tailRenderers.Add(new SaintsFieldWithRenderer(new LayoutEndAttribute("."), null));
                        continue;
                }

                AbsRenderer attributeRenderer = null;
                bool targetAnchor = false;

                switch (fieldWithInfo.RenderType)
                {
                    case SaintsRenderType.SerializedField:
                        switch (attribute)
                        {
                            case TableAttribute _:
                                targetAnchor = true;
                                if (!hasSerializedTarget)
                                {
                                    attributeRenderer = new TableRenderer(serializedObject, fieldWithInfo);
                                    hasSerializedTarget = true;
                                }
                                break;
                            case ListDrawerSettingsAttribute _:
                                targetAnchor = true;
                                if (!hasSerializedTarget)
                                {
                                    attributeRenderer = new ListDrawerSettingsRenderer(serializedObject,
                                        fieldWithInfo);
                                    hasSerializedTarget = true;
                                }
                                break;
                        }
                        break;
                    case SaintsRenderType.NonSerializedField:
                    case SaintsRenderType.NativeProperty:
                        if (attribute is ShowInInspectorAttribute)
                        {
                            targetAnchor = true;
                            attributeRenderer = new ShowInInspectorFieldRenderer(serializedObject, fieldWithInfo);
                        }
                        break;
                    case SaintsRenderType.Method:
                        switch (attribute)
                        {
                            case ButtonAttribute buttonAttribute:
                                targetAnchor = true;
                                attributeRenderer = new ButtonRenderer(buttonAttribute, serializedObject,
                                    fieldWithInfo);
                                break;
                            case ShowInInspectorAttribute _:
                                targetAnchor = true;
                                attributeRenderer = new RealTimeCalculatorRenderer(serializedObject, fieldWithInfo);
                                break;
                            case CustomContextMenuAttribute customContextMenuAttribute:
                                AddRendererWithMetadata(renderers, customContextMenuAttribute,
                                    new ButtonCustomContextMenuRenderer(customContextMenuAttribute,
                                        serializedObject, fieldWithInfo));
                                continue;
#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
                            case DOTweenPlayAttribute doTweenPlayAttribute:
                                AddRendererWithMetadata(renderers, doTweenPlayAttribute,
                                    new DOTweenPlayRenderer(serializedObject, fieldWithInfo));
                                continue;
#endif
                        }
                        break;
                }

                if (targetAnchor)
                {
                    if (customTarget)
                    {
                        if (!targetInserted)
                        {
                            AddTargetRenderers(renderers, playaAttribute, customTargetRenderers);
                            targetInserted = true;
                        }
                        else if (playaAttribute != null)
                        {
                            renderers.Add(new SaintsFieldWithRenderer(playaAttribute, null));
                        }
                    }
                    else if (attributeRenderer != null)
                    {
                        AddRendererWithMetadata(renderers, playaAttribute, attributeRenderer);
                    }
                    else if (playaAttribute != null)
                    {
                        renderers.Add(new SaintsFieldWithRenderer(playaAttribute, null));
                    }
                    continue;
                }

                if (attribute is PropertyAttribute propertyAttribute)
                {
                    Type decoratorDrawerType =
                        SaintsPropertyDrawer.PropertyGetDecoratorDrawer(propertyAttribute.GetType());
                    if (decoratorDrawerType != null)
                    {
                        AddRendererWithMetadata(renderers, playaAttribute,
                            new DecoratorDrawerRenderer(propertyAttribute, decoratorDrawerType, serializedObject,
                                fieldWithInfo));
                        continue;
                    }
                }

                if (playaAttribute != null)
                {
                    renderers.Add(new SaintsFieldWithRenderer(playaAttribute, null));
                }
            }

            if (customTarget)
            {
                if (!targetInserted)
                {
                    AddTargetRenderers(renderers, null, customTargetRenderers);
                }
            }
            else
            {
                AddFallbackRenderer(renderers, serializedObject, fieldWithInfo, hasSerializedTarget);
            }

            renderers.AddRange(tailRenderers);
            return renderers;
        }

        private static IEnumerable<Attribute> GetRendererAttributes(SaintsFieldWithInfo fieldWithInfo)
        {
            MemberInfo memberInfo = fieldWithInfo.AttributeMemberInfo ?? fieldWithInfo.FieldInfo ??
                                    (MemberInfo)fieldWithInfo.PropertyInfo ?? fieldWithInfo.MethodInfo;
            return memberInfo == null
                ? fieldWithInfo.PlayaAttributes.Cast<Attribute>()
                : ReflectCache.GetCustomAttributes<Attribute>(memberInfo);
        }

        private static void AddTargetRenderers(ICollection<SaintsFieldWithRenderer> renderers,
            IPlayaAttribute playaAttribute, IEnumerable<AbsRenderer> targetRenderers)
        {
            if (playaAttribute != null)
            {
                renderers.Add(new SaintsFieldWithRenderer(playaAttribute, null));
            }

            foreach (AbsRenderer targetRenderer in targetRenderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                renderers.Add(new SaintsFieldWithRenderer(null, targetRenderer));
            }
        }

        private static void AddRendererWithMetadata(ICollection<SaintsFieldWithRenderer> renderers,
            IPlayaAttribute playaAttribute, AbsRenderer renderer)
        {
            if (playaAttribute != null)
            {
                renderers.Add(new SaintsFieldWithRenderer(playaAttribute, null));
            }

            renderers.Add(new SaintsFieldWithRenderer(null, renderer));
        }

        private static void AddFallbackRenderer(ICollection<SaintsFieldWithRenderer> renderers,
            SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool hasSerializedTarget)
        {
            switch (fieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField when !hasSerializedTarget:
                    AbsRenderer serializedRenderer = fieldWithInfo.SerializedProperty.propertyType ==
                                                     SerializedPropertyType.Generic &&
                                                     fieldWithInfo.SerializedProperty.isArray
                        ? new ListDrawerSettingsRenderer(serializedObject, fieldWithInfo)
                        : new SerializedFieldRenderer(serializedObject, fieldWithInfo);
                    renderers.Add(new SaintsFieldWithRenderer(null, serializedRenderer));
                    break;
                case SaintsRenderType.InjectedSerializedField:
                    renderers.Add(new SaintsFieldWithRenderer(null,
                        new SerializedFieldBareRenderer(serializedObject, fieldWithInfo)));
                    break;
                case SaintsRenderType.ClassStruct when fieldWithInfo.PlayaAttributes
                    .OfType<IPlayaClassAttribute>().Any():
                    renderers.Add(new SaintsFieldWithRenderer(null, new EmptyRenderer()));
                    break;
                default:
                    if (fieldWithInfo.RenderType != SaintsRenderType.SerializedField &&
                        fieldWithInfo.RenderType != SaintsRenderType.NonSerializedField &&
                        fieldWithInfo.RenderType != SaintsRenderType.NativeProperty &&
                        fieldWithInfo.RenderType != SaintsRenderType.Method &&
                        fieldWithInfo.PlayaAttributes.OfType<ISaintsLayout>().Any())
                    {
                        renderers.Add(new SaintsFieldWithRenderer(null, new EmptyRenderer()));
                    }
                    break;
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

            // string[] groupByParts = path.Split('/');
            string[] groupByParts = RuntimeUtil.SeparatePath(path).ToArray();
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

        // private static string JoinGroupBy(string layoutGroupByAcc, string curGroupBy)
        private static IReadOnlyList<string> JoinGroupBy(IReadOnlyList<string> layoutGroupByAcc, IReadOnlyList<string> curGroupBy)
        {
            // List<string> ori = layoutGroupByAcc.Split('/').ToList();
            List<string> ori = new List<string>(layoutGroupByAcc);

            // foreach (string eachPart in curGroupBy.Split('/'))
            foreach (string eachPart in curGroupBy)
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

            // return ori.Count == 0? "": string.Join("/", ori);
            return ori;
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

        public bool IsSearchableOn() => _searchableShown;

        public virtual void OnEnable()
        {
            DrawHeaderGUI.EnsureInitLoad();
#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
            AliveInstances.Add(this);
#endif

            OnEnableIMGUI();
// #if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
//             OnEnableUIToolkit();
// #endif
        }

        public virtual void OnDestroy()
        {
#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
            RemoveInstance(this);
#endif

            OnDestroyIMGUI();
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
            OnDestroyUIToolkit();
#endif
        }

        private bool _searchableShown;

        public void OnHeaderButtonClick()
        {
            _searchableShown = !_searchableShown;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
            OnHeaderButtonClickUIToolkit();
#endif

            if (!_searchableShown)
            {
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
                ResetSearchUIToolkit();
#endif
            }
        }

        // private UnityEvent<string> _onSearchUIToolkit = new UnityEvent<string>();
    }
}
