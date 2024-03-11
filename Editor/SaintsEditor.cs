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
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
#if SAINTSFIELD_DOTWEEN
using DG.DOTweenEditor;
#endif


namespace SaintsField.Editor
{
    public class SaintsEditor: UnityEditor.Editor, IDOTweenPlayRecorder
    {
        // private MonoScript _monoScript;
        // private List<SaintsFieldWithInfo> _fieldWithInfos = new List<SaintsFieldWithInfo>();

#if SAINTSFIELD_DOTWEEN
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
        private IReadOnlyList<ISaintsRenderer> _renderers = new List<ISaintsRenderer>();

        #region UI

        #region UIToolkit

        protected virtual bool TryFixUIToolkit =>
#if SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
            false
#else
            true
#endif
        ;

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        public static VisualElement CreateVisualElement(IEnumerable<ISaintsRenderer> renderers)
        {
            // Debug.Log($"SaintsEditor: {target}");

            VisualElement root = new VisualElement();

            foreach (ISaintsRenderer renderer in renderers)
            {
                // Debug.Log(renderer);
                root.Add(renderer.CreateVisualElement());
            }

            return root;
        }

        public override VisualElement CreateInspectorGUI()
        {
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
                objectField.Bind(serializedObject);
                objectField.SetEnabled(false);
                root.Add(objectField);
            }

            IReadOnlyList<ISaintsRenderer> renderers = Setup(TryFixUIToolkit, serializedObject, target);

            root.Add(CreateVisualElement(renderers));

#if SAINTSFIELD_DOTWEEN
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
            _renderers = Setup(false, serializedObject, target);
#if SAINTSFIELD_DOTWEEN
            AliveInstances.Add(this);
#endif
        }

        public virtual void OnDestroy()
        {
#if SAINTSFIELD_DOTWEEN
            RemoveInstance(this);
#endif
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", GetMonoScript(target), GetType(),
                    false);
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

        // private void CheckMonoScript()
        // {
        //     if (_monoScript != null)
        //     {
        //         return;
        //     }
        //
        //
        //     if (target)
        //     {
        //         try
        //         {
        //             _monoScript = MonoScript.FromMonoBehaviour((MonoBehaviour) target);
        //         }
        //         catch (Exception)
        //         {
        //             try
        //             {
        //                 _monoScript = MonoScript.FromScriptableObject((ScriptableObject)target);
        //             }
        //             catch (Exception)
        //             {
        //                 _monoScript = null;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         _monoScript = null;
        //     }
        // }

        private static IReadOnlyList<ISaintsRenderer> Setup(bool tryFixUIToolkit, SerializedObject serializedObject,
            object target)
        {
            string[] serializableFields = GetSerializedProperties(serializedObject).ToArray();
            Dictionary<string, SerializedProperty> serializedPropertyDict = serializableFields
                .ToDictionary(each => each, serializedObject.FindProperty);
            return GetRenderers(tryFixUIToolkit, serializedPropertyDict, serializedObject, target);
        }

        public static IReadOnlyList<ISaintsRenderer> GetRenderers(bool tryFixUIToolkit, IReadOnlyDictionary<string, SerializedProperty> serializedPropertyDict, SerializedObject serializedObject, object target)
        {
            List<SaintsFieldWithInfo> fieldWithInfos = new List<SaintsFieldWithInfo>();
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);

            foreach (int inherentDepth in Enumerable.Range(0, types.Count))
            {
                Type systemType = types[inherentDepth];

                FieldInfo[] allFields = systemType
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                               BindingFlags.Public | BindingFlags.DeclaredOnly);

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
                    OrderedAttribute orderProp = fieldInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? int.MinValue;

                    fieldWithInfos.Add(new SaintsFieldWithInfo
                    {
                        Groups = fieldInfo.GetCustomAttributes<Attribute>().OfType<ISaintsGroup>().ToArray(),
                        Target = target,

                        RenderType = SaintsRenderType.SerializedField,
                        SerializedProperty = serializedPropertyDict[fieldInfo.Name],
                        FieldInfo = fieldInfo,
                        InherentDepth = inherentDepth,
                        Order = order,
                        // serializable = true,
                    });
                }
                #endregion

                #region nonSerFieldInfo
                IEnumerable<FieldInfo> nonSerFieldInfos = allFields
                    .Where(f => f.GetCustomAttributes(typeof(ShowInInspectorAttribute), true).Length > 0);
                foreach (FieldInfo nonSerFieldInfo in nonSerFieldInfos)
                {
                    OrderedAttribute orderProp = nonSerFieldInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? int.MinValue;
                    fieldWithInfos.Add(new SaintsFieldWithInfo
                    {
                        Groups = nonSerFieldInfo.GetCustomAttributes<Attribute>().OfType<ISaintsGroup>().ToArray(),
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

                // var methodAllAttribute = methodInfos
                //     .SelectMany(each => each.GetCustomAttributes<Attribute>())
                //     .Where(each => each is ISaintsMethodAttribute)
                //     .ToArray();

                // IEnumerable<ISaintsMethodAttribute> buttonMethodInfos = methodAllAttribute.OfType<ISaintsMethodAttribute>().Length > 0);

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    Attribute[] allMethodAttributes = methodInfo.GetCustomAttributes<Attribute>().ToArray();

                    if(allMethodAttributes.Any(each => each is ISaintsMethodAttribute))
                    {
                        OrderedAttribute orderProp =
                            allMethodAttributes.FirstOrDefault(each => each is OrderedAttribute) as OrderedAttribute;
                        int order = orderProp?.Order ?? int.MinValue;

                        // inspector does not care about inherited/new method. It just need to use the last one
                        fieldWithInfos.RemoveAll(each => each.RenderType == SaintsRenderType.Method && each.MethodInfo.Name == methodInfo.Name);

                        fieldWithInfos.Add(new SaintsFieldWithInfo
                        {
                            Groups = allMethodAttributes.OfType<ISaintsGroup>().ToArray(),
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
                    OrderedAttribute orderProp =
                        propertyInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? int.MinValue;
                    fieldWithInfos.Add(new SaintsFieldWithInfo
                    {
                        Groups = propertyInfo.GetCustomAttributes<Attribute>().OfType<ISaintsGroup>().ToArray(),
                        Target = target,

                        RenderType = SaintsRenderType.NativeProperty,
                        PropertyInfo = propertyInfo,
                        InherentDepth = inherentDepth,
                        Order = order,
                    });
                }
                #endregion
            }

            List<SaintsFieldWithInfo> fieldWithInfosSorted = fieldWithInfos
                .WithIndex()
                .OrderBy(each => each.value.InherentDepth)
                .ThenBy(each => each.value.Order)
                .ThenBy(each => each.index)
                .Select(each => each.value)
                .ToList();

            Dictionary<string, (ELayout eLayout, bool isDOTween)> layoutKeyToInfo = new Dictionary<string, (ELayout eLayout, bool isDOTween)>();
            foreach (ISaintsGroup sortedGroup in fieldWithInfosSorted.SelectMany(each => each.Groups))
            {
                string groupBy = sortedGroup.GroupBy;
                ELayout config = sortedGroup.Layout;
                bool configExists = layoutKeyToInfo.TryGetValue(groupBy, out (ELayout eLayout, bool isDOTween) info);
                if (!configExists || (info.eLayout == 0 && config != 0) || (!info.isDOTween && sortedGroup is DOTweenPlayAttribute))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                    Debug.Log($"add key {groupBy}: {config}.{config==0} (origin: {info.eLayout}.{info.eLayout==0})");
#endif
                    layoutKeyToInfo[groupBy] = (config, info.isDOTween || sortedGroup is DOTweenPlayAttribute);
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
#if SAINTSFIELD_DOTWEEN
                        each.Value.isDOTween
                            ? (ISaintsRendererGroup)new DOTweenPlayGroup(target)
                            : new SaintsRendererGroup(each.Key, each.Value.eLayout)
#else
                        (ISaintsRendererGroup)new SaintsRendererGroup(each.Key, each.Value.eLayout)
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
                    if(rootGroupAdded.Add(rootGroup))
                    {
                        renderers.Add(layoutKeyToGroup[rootGroup]);
                    }

                    AbsRenderer itemResult = MakeRenderer(serializedObject, fieldWithInfo, tryFixUIToolkit);

                    ISaintsRendererGroup targetGroup = layoutKeyToGroup[longestGroup.GroupBy];

                    if(itemResult != null)
                    {
    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                        Debug.Log($"add renderer {itemResult} to {longestGroup.GroupBy}({targetGroup})");
    #endif
                        targetGroup.Add(longestGroup.GroupBy, itemResult);
                    }
                    continue;
                }

                // Debug.Log($"group {fieldWithInfo.MethodInfo.Name} {fieldWithInfo.groups.Count}: {string.Join(",", fieldWithInfo.groups.Select(each => each.GroupBy))}");
    // #if SAINTSFIELD_DOTWEEN
    //             if(fieldWithInfo.groups.Count > 0)
    //             {
    //                 ISaintsGroup group = fieldWithInfo.groups[0];
    //                 Debug.Assert(group.GroupBy == DOTweenPlayAttribute.DOTweenPlayGroupBy);
    //                 List<SaintsFieldWithInfo> groupFieldWithInfos = fieldWithInfos
    //                     .Where(each => each.groups.Any(eachGroup => eachGroup.GroupBy == group.GroupBy))
    //                     .ToList();
    //
    // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
    //                 Debug.Assert(_doTweenPlayGroup == null, "doTweenPlayGroup should only be created once");
    // #endif
    //                 // Debug.Log($"create doTween play: {groupFieldWithInfos.Count}, {fieldWithInfo.MethodInfo.Name}");
    //                 _doTweenPlayGroup = new DOTweenPlayGroup(groupFieldWithInfos.Select(each => (each.MethodInfo,
    //                     (DOTweenPlayAttribute)each.groups[0])), parent);
    //                 fieldWithInfos.RemoveAll(each => groupFieldWithInfos.Contains(each));
    //                 return _doTweenPlayGroup;
    //             }
    // #endif

                AbsRenderer result = MakeRenderer(serializedObject, fieldWithInfo, tryFixUIToolkit);
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

        protected static AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit)
        {
            // Debug.Log($"field {fieldWithInfo.fieldInfo?.Name}/{fieldWithInfo.fieldInfo?.GetCustomAttribute<ExtShowHideConditionBase>()}");
            switch (fieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                    return new SerializedFieldRenderer(serializedObject, fieldWithInfo, tryFixUIToolkit);
                case SaintsRenderType.NonSerializedField:
                    return new NonSerializedFieldRenderer(serializedObject, fieldWithInfo, tryFixUIToolkit);
                case SaintsRenderType.Method:
                    return new MethodRenderer(serializedObject, fieldWithInfo, tryFixUIToolkit);
                case SaintsRenderType.NativeProperty:
                    return new NativePropertyRenderer(serializedObject, fieldWithInfo, tryFixUIToolkit);
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.RenderType), fieldWithInfo.RenderType, null);
            }
        }

// #if SAINTSFIELD_DOTWEEN
//         // every inspector instance can only have ONE doTweenPlayGroup
//         private DOTweenPlayGroup _doTweenPlayGroup = null;
// #endif
    }
}
