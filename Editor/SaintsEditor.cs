using System;
using System.Collections.Generic;
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
                objectField.AddToClassList("unity-base-field__aligned");
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
                    EditorGUILayout.ObjectField("Script", monoScript, GetType(),
                        false);
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

                FieldInfo[] allFields = systemType
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                               BindingFlags.Public | BindingFlags.DeclaredOnly);

                // foreach (FieldInfo fieldInfo in allFields)
                // {
                //     Debug.Log($"[{systemType}]: {fieldInfo.Name}");
                // }
                // Debug.Log($"[{systemType}] allFields count: {allFields.Length}");

                #region SerializedField

                IEnumerable<FieldInfo> serializableFieldInfos =
                    allFields.Where(fieldInfo =>
                        {
                            if (serializedPropertyDict.ContainsKey(fieldInfo.Name))
                            {
                                return true;
                            }

                            // Name            : <GetHitPoint>k__BackingField
                            // if (fieldInfo.Name.StartsWith("<") && fieldInfo.Name.EndsWith(">k__BackingField"))
                            // {
                            //     return serializedObject.FindProperty(fieldInfo.Name) != null;
                            // }

                            // return !fieldInfo.IsLiteral // const
                            //        && !fieldInfo.IsStatic // static
                            //        && !fieldInfo.IsInitOnly;
                            return false;
                        }
                        // readonly
                    );

                foreach (FieldInfo fieldInfo in serializableFieldInfos)
                {
                    // Debug.Log($"Name            : {fieldInfo.Name}");
                    // Debug.Log($"Declaring Type  : {fieldInfo.DeclaringType}");
                    // Debug.Log($"IsPublic        : {fieldInfo.IsPublic}");
                    // Debug.Log($"MemberType      : {fieldInfo.MemberType}");
                    // Debug.Log($"FieldType       : {fieldInfo.FieldType}");
                    // Debug.Log($"IsFamily        : {fieldInfo.IsFamily}");

                    IReadOnlyList<IPlayaAttribute> playaAttributes = fieldInfo.GetCustomAttributes<Attribute>().OfType<IPlayaAttribute>().ToArray();

                    OrderedAttribute orderProp = playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                    int order = orderProp?.Order ?? int.MinValue;

                    fieldWithInfos.Add(new SaintsFieldWithInfo
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
                IEnumerable<FieldInfo> nonSerFieldInfos = allFields
                    .Where(f => f.GetCustomAttributes(typeof(ShowInInspectorAttribute), true).Length > 0);
                foreach (FieldInfo nonSerFieldInfo in nonSerFieldInfos)
                {
                    IReadOnlyList<IPlayaAttribute> playaAttributes = nonSerFieldInfo.GetCustomAttributes<Attribute>().OfType<IPlayaAttribute>().ToArray();

                    OrderedAttribute orderProp = playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                    int order = orderProp?.Order ?? int.MinValue;
                    fieldWithInfos.Add(new SaintsFieldWithInfo
                    {
                        PlayaAttributes = playaAttributes,
                        Groups = playaAttributes.OfType<ISaintsGroup>().ToArray(),
                        Target = target,

                        RenderType = SaintsRenderType.NonSerializedField,
                        // memberType = nonSerFieldInfo.MemberType,
                        FieldInfo = nonSerFieldInfo,
                        InherentDepth = inherentDepth,
                        Order = order,
                        // serializable = false,
                    });
                }
                #endregion

                #region Method

                MethodInfo[] methodInfos = systemType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly);

                // Debug.Log($"[{systemType}] methodInfos count: {methodInfos.Length}");
                // foreach (MethodInfo methodInfo in methodInfos)
                // {
                //     Debug.Log($"[{systemType}] method: {methodInfo.Name}");
                // }

                // var methodAllAttribute = methodInfos
                //     .SelectMany(each => each.GetCustomAttributes<Attribute>())
                //     .Where(each => each is ISaintsMethodAttribute)
                //     .ToArray();

                // IEnumerable<ISaintsMethodAttribute> buttonMethodInfos = methodAllAttribute.OfType<ISaintsMethodAttribute>().Length > 0);

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    IReadOnlyList<IPlayaAttribute> playaAttributes = methodInfo.GetCustomAttributes<Attribute>().OfType<IPlayaAttribute>().ToArray();
                    
                    // Attribute[] allMethodAttributes = methodInfo.GetCustomAttributes<Attribute>().ToArray();

                    if (playaAttributes.Any(each => each is IPlayaMethodAttribute))
                    {
                        OrderedAttribute orderProp =
                            playaAttributes.FirstOrDefault(each => each is OrderedAttribute) as OrderedAttribute;
                        int order = orderProp?.Order ?? int.MinValue;

                        // inspector does not care about inherited/new method. It just need to use the last one
                        fieldWithInfos.RemoveAll(each => each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);

                        fieldWithInfos.Add(new SaintsFieldWithInfo
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
                    }
                }
                #endregion

                #region NativeProperty
                IEnumerable<PropertyInfo> propertyInfos = systemType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(p => p.GetCustomAttributes(typeof(ShowInInspectorAttribute), true).Length > 0);

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    IReadOnlyList<IPlayaAttribute> playaAttributes = propertyInfo.GetCustomAttributes<Attribute>().OfType<IPlayaAttribute>().ToArray();

                    OrderedAttribute orderProp =
                        playaAttributes.OfType<OrderedAttribute>().FirstOrDefault();
                    int order = orderProp?.Order ?? int.MinValue;
                    fieldWithInfos.Add(new SaintsFieldWithInfo
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

            if (pendingSerializedProperties.Count > 0)
            {
                // we got unused serialized properties because Unity directly inject them rather than using a
                // normal workflow
                foreach (KeyValuePair<string, SerializedProperty> pendingSer in pendingSerializedProperties)
                {
                    int order = int.MinValue;

                    fieldWithInfos.Add(new SaintsFieldWithInfo
                    {
                        PlayaAttributes = Array.Empty<IPlayaAttribute>(),
                        Groups = Array.Empty<ISaintsGroup>(),
                        Target = target,

                        RenderType = SaintsRenderType.SerializedField,
                        SerializedProperty = pendingSer.Value,
                        FieldInfo = null,
                        InherentDepth = types.Count - 1,
                        Order = order,
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

            // handle GroupAllFieldsUntilNextGroupAttribute
            List<SaintsFieldWithInfo> fieldInfosWithGroups = new List<SaintsFieldWithInfo>();
            int previousGroupIndex = -1;
            for (int i = 0; i < fieldWithInfosSorted.Count; i++)
            {
                IReadOnlyList<ISaintsGroup> groups;
                if (fieldWithInfosSorted[i].Groups.Count == 0 && previousGroupIndex != -1)
                {
                    groups = fieldInfosWithGroups.Count > previousGroupIndex
                        ? fieldInfosWithGroups[previousGroupIndex].Groups
                        : fieldWithInfosSorted[previousGroupIndex].Groups;
                }
                else
                {
                    groups = fieldWithInfosSorted[i].Groups;
                }

                var fieldWithInfo = fieldWithInfosSorted[i];
                fieldWithInfo.Groups = groups;
                fieldInfosWithGroups.Add(fieldWithInfo);
                
                foreach (var group in fieldWithInfo.Groups)
                {
                    if (group.GroupAllFieldsUntilNextGroupAttribute)
                    {
                        previousGroupIndex = fieldInfosWithGroups.Count - 1;
                        break;
                    }
                    
                    previousGroupIndex = -1;
                }
            }
            fieldWithInfosSorted = fieldInfosWithGroups;
            
            Dictionary<string, (ELayout eLayout, bool isDOTween, bool closedByDefault)> layoutKeyToInfo = new Dictionary<string, (ELayout eLayout, bool isDOTween, bool closedByDefault)>();
            foreach (ISaintsGroup sortedGroup in fieldWithInfosSorted.SelectMany(each => each.Groups))
            {
                string groupBy = sortedGroup.GroupBy;
                ELayout config = sortedGroup.Layout;
                bool configExists = layoutKeyToInfo.TryGetValue(groupBy, out (ELayout eLayout, bool isDOTween, bool closedByDefault) info);
                if (!configExists || (info.eLayout == 0 && config != 0) || (!info.isDOTween && sortedGroup is DOTweenPlayAttribute))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                    Debug.Log($"add key {groupBy}: {config}.{config==0} (origin: {info.eLayout}.{info.eLayout==0})");
#endif
                    layoutKeyToInfo[groupBy] = (config, info.isDOTween || sortedGroup is DOTweenPlayAttribute, sortedGroup.ClosedByDefault);
                }
                else
                {
                    Debug.Assert(info.eLayout == config || config == 0, $"layout config conflict: [{groupBy}] {info.eLayout} vs {config}");
                }
            }

            Dictionary<string, ISaintsRendererGroup> layoutKeyToGroup = layoutKeyToInfo
                .ToDictionary(
                    each => each.Key,
                    each =>
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
                        each.Value.isDOTween
                            ? (ISaintsRendererGroup)new DOTweenPlayGroup(target)
                            : new SaintsRendererGroup(each.Key, each.Value.eLayout, each.Value.closedByDefault)
#else
                        (ISaintsRendererGroup)new SaintsRendererGroup(each.Key, each.Value.eLayout, each.Value.closedByDefault)
#endif
                );

            Dictionary<string, ISaintsRendererGroup> unconnectedSubLayoutKeyToGroup = layoutKeyToGroup
                .Where(each => each.Key.Contains('/'))
                .ToDictionary(each => each.Key, each => each.Value);

            List<ISaintsRenderer> renderers = new List<ISaintsRenderer>();
            HashSet<string> rootGroupAdded = new HashSet<string>();
            while (fieldWithInfosSorted.Count > 0)
            {
                SaintsFieldWithInfo fieldWithInfo = fieldWithInfosSorted[0];
                fieldWithInfosSorted.RemoveAt(0);
                if (fieldWithInfo.Groups.Count > 0)
                {
                    ISaintsGroup longestGroup = fieldWithInfo.Groups
                        .OrderByDescending(each => each.GroupBy.Length)
                        .First();

                    // check if this group need to be connected
                    if (unconnectedSubLayoutKeyToGroup.ContainsKey(longestGroup.GroupBy))
                    {
                        foreach((string parentGroupBy, string subGroupBy) in ChunkGroupBy(longestGroup.GroupBy)
                                    .Where(each => unconnectedSubLayoutKeyToGroup.ContainsKey(each.subGroupBy)))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                            Debug.Log($"Layout connect: {parentGroupBy}->{subGroupBy}");
#endif
                            ISaintsRendererGroup parentGroup = layoutKeyToGroup[parentGroupBy];
                            ISaintsRendererGroup subGroup = layoutKeyToGroup[subGroupBy];
                            parentGroup.Add(subGroupBy, subGroup);
                            unconnectedSubLayoutKeyToGroup.Remove(subGroupBy);
                        }
                    }

                    string rootGroup = longestGroup.GroupBy.Split('/')[0];
                    if (rootGroupAdded.Add(rootGroup))
                    {
                        renderers.Add(layoutKeyToGroup[rootGroup]);
                    }

                    AbsRenderer itemResult = MakeRenderer(serializedObject, fieldWithInfo);

                    ISaintsRendererGroup targetGroup = layoutKeyToGroup[longestGroup.GroupBy];

                    if (itemResult != null)
                    {
    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                        Debug.Log($"add renderer {itemResult} to {longestGroup.GroupBy}({targetGroup})");
    #endif
                        targetGroup.Add(longestGroup.GroupBy, itemResult);
                    }
                    continue;
                }

                AbsRenderer result = MakeRenderer(serializedObject, fieldWithInfo);
                // Debug.Log($"direct render {result}, {fieldWithInfo.RenderType}, {fieldWithInfo.MethodInfo?.Name}");

                if (result != null)
                {
                    renderers.Add(result);
                }
                // renderer?.Render();
                // renderer.AfterRender();
            }

            return renderers;
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
