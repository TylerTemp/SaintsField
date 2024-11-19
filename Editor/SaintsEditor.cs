using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using DG.DOTweenEditor;
#endif


namespace SaintsField.Editor
{
    public class SaintsEditor: UnityEditor.Editor, IDOTweenPlayRecorder
    {
        // private MonoScript _monoScript;
        // private List<SaintsFieldWithInfo> _fieldWithInfos = new List<SaintsFieldWithInfo>();

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

        #region UI

        #region UIToolkit

//         protected virtual bool TryFixUIToolkit =>
// #if SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
//             false
// #else
//             true
// #endif
//         ;

        [Obsolete("No longer needed")]
        protected virtual bool TryFixUIToolkit => false;

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        public override VisualElement CreateInspectorGUI()
        {
            // Debug.Log("CreateInspectorGUI");

            if (target == null)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            MonoScript monoScript = GetMonoScript(target);
            if(monoScript)
            {
                ObjectField objectField = new ObjectField("Script")
                {
                    bindingPath = "m_Script",
                    value = monoScript,
                    allowSceneObjects = false,
                    objectType = typeof(MonoScript),
                };
                objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                objectField.Bind(serializedObject);
                objectField.SetEnabled(false);
                root.Add(objectField);
            }

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            IReadOnlyList<ISaintsRenderer> renderers = Setup(serializedObject, target);

            // Debug.Log($"renderers.Count={renderers.Count}");
            foreach (ISaintsRenderer saintsRenderer in renderers)
            {
                // Debug.Log($"renderer={saintsRenderer}");
                root.Add(saintsRenderer.CreateVisualElement());
            }

            // root.Add(CreateVisualElement(renderers));

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            root.RegisterCallback<AttachToPanelEvent>(_ => AddInstance(this));
            root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance(this));
#endif
            return root;
        }

#endif
        #endregion

        #region IMGUI

        public override bool RequiresConstantRepaint() => true;

        public virtual void OnEnable()
        {
            // Debug.Log($"OnEnable");
            try
            {
                _renderers = Setup(serializedObject, target);
            }
            catch (Exception)
            {
                _renderers = null;  // just... let IMGUI renderer to deal with it...
            }
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            AliveInstances.Add(this);
#endif
        }

        public virtual void OnDestroy()
        {
            if (_renderers != null)
            {
                foreach (ISaintsRenderer renderer in _renderers)
                {
                    renderer.OnDestroy();
                }
            }
            _renderers = null;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            RemoveInstance(this);
#endif
        }

#if UNITY_2019_2_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
#if UNITY_2019_3_OR_NEWER
        [InitializeOnEnterPlayMode]
#endif
        private void ResetRenderersImGui()
        {
            _renderers = null;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            AliveInstances.Clear();
            DOTweenEditorPreview.Stop();
#endif
        }

        public override void OnInspectorGUI()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_renderers == null)
            {
                _renderers = Setup(serializedObject, target);
            }
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            AliveInstances.Add(this);
#endif

            MonoScript monoScript = GetMonoScript(target);
            if(monoScript)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    try
                    {
                        EditorGUILayout.ObjectField("Script", monoScript, GetType(), false);
                    }
                    catch (NullReferenceException)
                    {
                        // ignored
                    }
                }
            }

            serializedObject.Update();

            foreach (ISaintsRenderer renderer in _renderers)
            {
                renderer.Render();
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #endregion

        private static MonoScript GetMonoScript(UnityEngine.Object target)
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

        private static IReadOnlyList<ISaintsRenderer> Setup(SerializedObject serializedObject,
            object target)
        {
            string[] serializableFields = GetSerializedProperties(serializedObject).ToArray();
            // Debug.Log($"serializableFields={string.Join(",", serializableFields)}");
            Dictionary<string, SerializedProperty> serializedPropertyDict = serializableFields
                .ToDictionary(each => each, serializedObject.FindProperty);
            // Debug.Log($"serializedPropertyDict.Count={serializedPropertyDict.Count}");
            return GetRenderers(serializedPropertyDict, serializedObject, target);
        }

        public static IReadOnlyList<ISaintsRenderer> GetRenderers(
            IReadOnlyDictionary<string, SerializedProperty> serializedPropertyDict, SerializedObject serializedObject,
            object target)
        {
            List<SaintsFieldWithInfo> fieldWithInfos = new List<SaintsFieldWithInfo>();
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);

            // Dictionary<string, SerializedProperty> pendingSerializedProperties = new Dictionary<string, SerializedProperty>(serializedPropertyDict);
            Dictionary<string, SerializedProperty> pendingSerializedProperties = serializedPropertyDict.ToDictionary(each => each.Key, each => each.Value);
            // Debug.Log($"{string.Join(",", pendingSerializedProperties.Keys)}");
            pendingSerializedProperties.Remove("m_Script");

            foreach (int inherentDepth in Enumerable.Range(0, types.Count))
            {
                Type systemType = types[inherentDepth];

                // as we can not get the correct order, we'll make it order as: field(serialized+nonSerialized), property, method
                List<SaintsFieldWithInfo> fieldInfos = new List<SaintsFieldWithInfo>();
                List<SaintsFieldWithInfo> propertyInfos = new List<SaintsFieldWithInfo>();
                List<SaintsFieldWithInfo> methodInfos = new List<SaintsFieldWithInfo>();

                foreach (MemberInfo memberInfo in systemType
                             .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                         BindingFlags.Public | BindingFlags.DeclaredOnly)
                             .OrderBy(memberInfo => memberInfo.MetadataToken))  // this is still not the correct order, but... a bit better
                {
                    IReadOnlyList<IPlayaAttribute> playaAttributes = memberInfo
                        .GetCustomAttributes<Attribute>().OfType<IPlayaAttribute>().ToArray();
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

                                fieldInfos.Add(new SaintsFieldWithInfo
                                {
                                    PlayaAttributes = playaAttributes,
                                    Groups = playaAttributes.OfType<ISaintsGroup>().ToArray(),
                                    Target = target,

                                    RenderType = SaintsRenderType.SerializedField,
                                    SerializedProperty = pendingSerializedProperties[fieldInfo.Name],
                                    FieldInfo = fieldInfo,
                                    InherentDepth = inherentDepth,
                                    Order = order,
                                    // serializable = true,
                                });
                                pendingSerializedProperties.Remove(fieldInfo.Name);
                            }
                            #endregion

                            #region nonSerFieldInfo

                            else if (playaAttributes.Count > 0)
                            {
                                OrderedAttribute orderProp = playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                                int order = orderProp?.Order ?? int.MinValue;
                                fieldInfos.Add(new SaintsFieldWithInfo
                                {
                                    PlayaAttributes = playaAttributes,
                                    Groups = playaAttributes.OfType<ISaintsGroup>().ToArray(),
                                    Target = target,

                                    RenderType = SaintsRenderType.NonSerializedField,
                                    // memberType = nonSerFieldInfo.MemberType,
                                    FieldInfo = fieldInfo,
                                    InherentDepth = inherentDepth,
                                    Order = order,
                                    // serializable = false,
                                });
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
                                propertyInfos.Add(new SaintsFieldWithInfo
                                {
                                    PlayaAttributes = playaAttributes,
                                    Groups = playaAttributes.OfType<ISaintsGroup>().ToArray(),
                                    Target = target,

                                    RenderType = SaintsRenderType.NativeProperty,
                                    PropertyInfo = propertyInfo,
                                    InherentDepth = inherentDepth,
                                    Order = order,
                                });
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

                            // inspector does not care about inherited/new method. It just needs to use the last one
                            fieldWithInfos.RemoveAll(each => each.InherentDepth < inherentDepth && each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);
                            methodInfos.RemoveAll(each => each.InherentDepth < inherentDepth && each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD
                                Debug.Log($"[{systemType}] method: {methodInfo.Name}");
#endif

                            methodInfos.Add(new SaintsFieldWithInfo
                            {
                                PlayaAttributes = playaAttributes,
                                Groups = playaAttributes.OfType<ISaintsGroup>().ToArray(),
                                Target = target,

                                // memberType = MemberTypes.Method,
                                RenderType = SaintsRenderType.Method,
                                MethodInfo = methodInfo,
                                InherentDepth = inherentDepth,
                                Order = order,
                            });
                            #endregion
                        }
                            break;
                    }
                }

                fieldWithInfos.AddRange(fieldInfos);
                fieldWithInfos.AddRange(propertyInfos);
                fieldWithInfos.AddRange(methodInfos);
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
                        Groups = Array.Empty<ISaintsGroup>(),
                        Target = target,

                        RenderType = SaintsRenderType.SerializedField,
                        SerializedProperty = pendingSer.Value,
                        FieldInfo = null,
                        InherentDepth = types.Count - 1,
                        Order = int.MinValue,
                        // serializable = true,
                    });
                }
            }

            List<SaintsFieldWithInfo> fieldWithInfosSorted = fieldWithInfos
                .WithIndex()
                .OrderBy(each => each.value.InherentDepth)
                .ThenBy(each => each.value.Order)
                .ThenBy(each => each.index)
                .Select(each => each.value)
                .ToList();

            IReadOnlyList<RendererGroupInfo> chainedGroups = ChainSaintsFieldWithInfo(fieldWithInfosSorted);
            return FlattenRendererGroupInfoIntoRenderers(chainedGroups, serializedObject, target).Select(each => each.saintsRenderer).ToArray();
        }

        private static IEnumerable<(string absGroupBy, ISaintsRenderer saintsRenderer)> FlattenRendererGroupInfoIntoRenderers(IReadOnlyList<RendererGroupInfo> chainedGroups, SerializedObject serializedObject, object target)
        {
            foreach (RendererGroupInfo rendererGroupInfo in chainedGroups)
            {
                bool isEndNode = rendererGroupInfo.AbsGroupBy == "" && rendererGroupInfo.Children.Count == 0;

                if (isEndNode)
                {
                    AbsRenderer result =  MakeRenderer(serializedObject, rendererGroupInfo.FieldWithInfo);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                    if(rendererGroupInfo.FieldWithInfo.MethodInfo == null)
                    {
                        Debug.Log($"Flatten EndNode return {result}");
                    }
#endif
                    yield return ("", result);
                }
                else
                {
                    (string absGroupBy, ISaintsRenderer saintsRenderer)[] children = FlattenRendererGroupInfoIntoRenderers(rendererGroupInfo.Children, serializedObject, target).ToArray();
                    if (children.Length > 0)
                    {

                        string curGroupAbs = rendererGroupInfo.AbsGroupBy;

                        ISaintsRendererGroup group =
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
                            rendererGroupInfo.Config.IsDoTween
                                ? (ISaintsRendererGroup)new DOTweenPlayGroup(target)
                                : new SaintsRendererGroup(curGroupAbs, rendererGroupInfo.Config)
#else
                            new SaintsRendererGroup(curGroupAbs, rendererGroupInfo.Config)
#endif
                        ;

                        foreach ((string eachChildGroupBy, ISaintsRenderer eachChild) in children)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                            Debug.Log($"Flatten {group} add renderer {eachChild}");
#endif

                            group.Add(eachChildGroupBy, eachChild);
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                        Debug.Log($"Flatten {group} return with {children.Length} children");
#endif

                        yield return (rendererGroupInfo.AbsGroupBy, group);
                    }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                    else
                    {
                        Debug.Log($"Flatten {rendererGroupInfo.AbsGroupBy} empty children, skip");
                    }
#endif
                }
            }
        }

        private class RendererGroupInfo {
            public string AbsGroupBy;  // ""=normal fields, other=grouped fields
            public List<RendererGroupInfo> Children;
            public SaintsRendererGroup.Config Config;
            public SaintsFieldWithInfo FieldWithInfo;
        }

        private static IReadOnlyList<RendererGroupInfo> ChainSaintsFieldWithInfo(List<SaintsFieldWithInfo> fieldWithInfosSorted)
        {
            List<RendererGroupInfo> rendererGroupInfos = new List<RendererGroupInfo>();
            Dictionary<string, RendererGroupInfo> rootToRendererGroupInfo =
                new Dictionary<string, RendererGroupInfo>();

            RendererGroupInfo keepGroupingInfo = null;
            int inherent = -1;
            foreach (SaintsFieldWithInfo saintsFieldWithInfo in fieldWithInfosSorted)
            {
                bool isNewInherent = saintsFieldWithInfo.InherentDepth != inherent;
                inherent = saintsFieldWithInfo.InherentDepth;

                IReadOnlyList<ISaintsGroup> groups = saintsFieldWithInfo.Groups;
                RendererGroupInfo lastGroupInfo = null;

                if (isNewInherent)
                {
                    keepGroupingInfo = null;
                    lastGroupInfo = null;
                }

                if (groups.Count > 0)
                {
                    string preAbsGroupBy = null;
                    foreach (ISaintsGroup saintsGroup in groups)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                        Debug.Log($"Layout processing {saintsGroup}/{saintsGroup.LayoutBy}");
#endif

                        switch (saintsGroup)
                        {
                            case LayoutEndAttribute layoutEndAttribute:
                            {
                                string endGroupBy = layoutEndAttribute.LayoutBy;
                                if (endGroupBy == null)
                                {
                                    keepGroupingInfo = null;
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
                                                    out RendererGroupInfo info))
                                            {
                                                rootToRendererGroupInfo[openGroupTo] = info = new RendererGroupInfo
                                                {
                                                    AbsGroupBy = openGroupTo,
                                                    Children = new List<RendererGroupInfo>(),
                                                    Config = new SaintsRendererGroup.Config(),
                                                };
                                            }

                                            keepGroupingInfo = info.Config.KeepGrouping ? info : null;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                            Debug.Log($"Layout close, {closeGroup}->{openGroupTo}: {keepGroupingInfo?.AbsGroupBy}");
#endif
                                        }
                                        else
                                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                            Debug.Log($"Layout close, {closeGroup}: null");
#endif
                                            keepGroupingInfo = null;
                                        }
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
                                        if (parentGroupBy != "" && rootToRendererGroupInfo.TryGetValue(parentGroupBy,
                                                out RendererGroupInfo info))
                                        {
                                            keepGroupingInfo = info.Config.KeepGrouping
                                                ? info
                                                : null;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                            Debug.Log($"Layout close, {endGroupBy}->{parentGroupBy}: {keepGroupingInfo?.AbsGroupBy}");
#endif
                                        }
                                        else
                                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                            Debug.Log($"Layout close, {endGroupBy}: null");
#endif
                                            keepGroupingInfo = null;
                                        }
                                    }
                                }
                            }
                                break;
                            default:
                            {
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

                                (bool newRoot, RendererGroupInfo targetGroup) = GetOrCreateGroupInfo(rootToRendererGroupInfo, groupBy);
                                if (newRoot)
                                {
                                    rendererGroupInfos.Add(targetGroup);
                                }
                                lastGroupInfo = targetGroup;

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
                                };

                                if (targetGroup.Config.KeepGrouping)
                                {
                                    keepGroupingInfo = targetGroup;
                                }
                                else if (keepGroupingInfo != null &&
                                         targetGroup.AbsGroupBy != keepGroupingInfo.AbsGroupBy)
                                {
                                    keepGroupingInfo = null;
                                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                                Debug.Log($"Layout item {groupBy}, newRoot={newRoot}, eLayout={targetGroup.Config.eLayout}, keepGroupingInfo={keepGroupingInfo?.AbsGroupBy}");
#endif
                            }
                                break;
                        }
                    }
                }

                RendererGroupInfo endNode = new RendererGroupInfo
                {
                    AbsGroupBy = "",
                    Children = new List<RendererGroupInfo>(),
                    Config = new SaintsRendererGroup.Config(),
                    FieldWithInfo = saintsFieldWithInfo,
                };

                if (lastGroupInfo == null && keepGroupingInfo != null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                    Debug.Log($"Layout lastGroupInfo set to keepGrouping: {keepGroupingInfo.AbsGroupBy}");
#endif
                    lastGroupInfo = keepGroupingInfo;
                }

                if (lastGroupInfo == null)
                {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
//                     Debug.Log($"Layout add direct: {saintsFieldWithInfo.FieldInfo?.Name ?? saintsFieldWithInfo.PropertyInfo?.Name ?? saintsFieldWithInfo.MethodInfo?.Name}");
// #endif
                    rendererGroupInfos.Add(endNode);
                }
                else
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_LAYOUT
                    Debug.Log($"Layout add field under {lastGroupInfo.AbsGroupBy}: {saintsFieldWithInfo.FieldInfo?.Name ?? saintsFieldWithInfo.PropertyInfo?.Name ?? saintsFieldWithInfo.MethodInfo?.Name}");
#endif
                    lastGroupInfo.Children.Add(endNode);
                    // if (!lastGroupInfo.AbsGroupBy.Contains('/') && !rendererGroupInfos.Contains(lastGroupInfo))
                    // {
                    //     rendererGroupInfos.Add(lastGroupInfo);
                    // }
                }
            }

            return rendererGroupInfos;
        }

        private static (bool newRoot, RendererGroupInfo rendererGroupInfo) GetOrCreateGroupInfo(Dictionary<string, RendererGroupInfo> rootToRendererGroupInfo, string path)
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

        private static IEnumerable<string> GetSerializedProperties(SerializedObject serializedObject)
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
        //     if (layoutInfo.Config.HasFlag(ELayout.Vertical))
        //     {
        //         return new VerticalGroup(layoutInfo.Config);
        //     }
        //     return new HorizontalGroup(layoutInfo.Config);
        // }
        // private static ISaintsRendererGroup MakeRendererGroup(ELayout layoutInfo)
        // {
        //     if (layoutInfo.HasFlag(ELayout.Tab))
        //     {
        //         return new SaintsRendererGroup(layoutInfo);
        //     }
        //     if (layoutInfo.HasFlag(ELayout.Horizontal))
        //     {
        //         return new HorizontalGroup(layoutInfo);
        //     }
        //     return new VerticalGroup(layoutInfo);
        // }

        protected static AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            // Debug.Log($"field {fieldWithInfo.fieldInfo?.Name}/{fieldWithInfo.fieldInfo?.GetCustomAttribute<ExtShowHideConditionBase>()}");
            switch (fieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                    return new SerializedFieldRenderer(serializedObject, fieldWithInfo);
                case SaintsRenderType.NonSerializedField:
                    return new NonSerializedFieldRenderer(serializedObject, fieldWithInfo);
                case SaintsRenderType.Method:
                    return new MethodRenderer(serializedObject, fieldWithInfo);
                case SaintsRenderType.NativeProperty:
                    return new NativePropertyRenderer(serializedObject, fieldWithInfo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.RenderType), fieldWithInfo.RenderType, null);
            }
        }
    }
}
