using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#region NaughtyAttributes Imports
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
using NaughtyAttributes.Editor;
#endif
#endregion

#region DOTween Imports
#if EXT_INSPECTOR_DOTWEEN
using DG.DOTweenEditor;
using DG.Tweening;
#endif
#endregion

namespace ExtInspector.Editor
{
#if !EXT_INSPECTOR_DISABLE
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
#endif
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ExtBaseInspector : UnityEditor.Editor
    {
        protected struct FieldWithInfo
        {
            public enum RenderType
            {
                None,
                SerializedField,
                NonSerializedField,
                Method,
                // DOTweenMethod,
                NativeProperty,
                GroupAttribute,
            }

            public enum GroupedType
            {
                // None,
                BoxGroup,
                Foldout,
#if EXT_INSPECTOR_DOTWEEN
                // ReSharper disable once InconsistentNaming
                DOTween,
#endif
            }

            public int order;

            public RenderType renderType;

            public GroupedType groupedType;
            public string groupName;
            // public MemberTypes memberType;

            public FieldInfo fieldInfo;
            public MethodInfo methodInfo;
            public PropertyInfo propertyInfo;
            public List<FieldInfo> fieldInfos;
            public List<MethodInfo> methodInfos;
        }

        private readonly Dictionary<string, SavedBool> _foldouts = new Dictionary<string, SavedBool>();
        private readonly List<FieldWithInfo> _fieldWithInfos = new List<FieldWithInfo>();

        #region DoTween
#if EXT_INSPECTOR_DOTWEEN
        private const string DoTweenMethodsGroupName = "__DOTWEEN_INSPECTOR_PREVIEW_EXT_GROUP_KEY__";

        private class DOTweenStatus
        {
            public Sequence sequence;
            public bool isPlaying;
            public DOTweenPreviewAttribute.StopAction stopAction;
        }

        private readonly Dictionary<int, DOTweenStatus> _funcToDOTweenStatus = new Dictionary<int, DOTweenStatus>();

        private bool _doTweenPreviewPlaying;
#endif
        #endregion

        private MonoScript _monoScript;

        #region Disposable Renders

        protected abstract class Renderer
        {
            protected readonly FieldWithInfo fieldWithInfo;
            protected readonly SerializedObject serializedObject;

            protected Renderer(UnityEditor.Editor editor, FieldWithInfo fieldWithInfo)
            {
                this.fieldWithInfo = fieldWithInfo;
                serializedObject = editor.serializedObject;
            }

            public abstract void Render();

            public virtual void AfterRender()
            {
            }
        }

        protected class SerializedFieldRenderer: Renderer
        {
            public SerializedFieldRenderer(UnityEditor.Editor editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
            }

            public override void Render()
            {
                // FieldWithInfo fieldWithInfo = this.fiend
                SerializedProperty property = serializedObject.FindProperty(fieldWithInfo.fieldInfo.Name);
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                // Check if visible
                bool visible = PropertyUtility.IsVisible(property);
                if (!visible)
                {
                    return;
                }
                // Validate
                ValidatorAttribute[] validatorAttributes = PropertyUtility.GetAttributes<ValidatorAttribute>(property);
                foreach (ValidatorAttribute validatorAttribute in validatorAttributes)
                {
                    validatorAttribute.GetValidator().ValidateProperty(property);
                }
#endif

#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                // Check if enabled and draw
                EditorGUI.BeginChangeCheck();
                bool enabled = PropertyUtility.IsEnabled(property);

                using (new EditorGUI.DisabledScope(disabled: !enabled))
                {
#endif
                    // propertyFieldFunction.Invoke(rect, property, PropertyUtility.GetLabel(property), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldWithInfo.fieldInfo.Name));
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                }

                // Call OnValueChanged callbacks
                if (EditorGUI.EndChangeCheck())
                {
                    PropertyUtility.CallOnValueChangedCallbacks(property);
                }
#endif
            }
        }

        protected class BoxGroupRenderer : Renderer
        {
            private readonly ExtBaseInspector _editor;

            public BoxGroupRenderer(ExtBaseInspector editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
                _editor = editor;
            }

            public override void Render()
            {
                Debug.Assert(fieldWithInfo.fieldInfos.Count >= 1);
                (string name, SerializedProperty prop)[] visibleProperties = fieldWithInfo.fieldInfos
                    .Select(fieldInfo => (name: fieldInfo.Name, prop: serializedObject.FindProperty(fieldInfo.Name)))
                    // .Where(each => PropertyUtility.IsVisible(each.prop))
                    .Where(each => _editor.IsVisible(each.prop))
                    .ToArray();

                if (visibleProperties.Length == 0)
                {
                    return;
                }

                Util.BeginBoxGroup_Layout(fieldWithInfo.groupName);
                foreach ((string _, SerializedProperty prop) in visibleProperties)
                {
                    // Util.PropertyField_Layout(prop, includeChildren: true);
                    EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName), true);
                }

                Util.EndBoxGroup_Layout();
            }
        }

        protected class FoldoutRenderer : Renderer
        {
            // private readonly Dictionary<string, SavedBool> _foldouts;
            // private readonly Func<Dictionary<string, SavedBool>> _getFoldoutsBool;
            private Renderer _rendererImplementation;
            private readonly ExtBaseInspector _extBaseInspector;

            // public FoldoutRenderer(Func<Dictionary<string, SavedBool>> getFoldoutsBool)
            // {
            //     _getFoldoutsBool = getFoldoutsBool;
            // }

            public FoldoutRenderer(ExtBaseInspector editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
                _extBaseInspector = editor;
            }

            public override void Render()
            {
                Debug.Assert(fieldWithInfo.fieldInfos.Count >= 1);
                (string name, SerializedProperty prop)[] visibleProperties = fieldWithInfo.fieldInfos
                    .Select(fieldInfo => (name: fieldInfo.Name, prop: serializedObject.FindProperty(fieldInfo.Name)))
                    // .Where(each => PropertyUtility.IsVisible(each.prop))
                    .Where(each => _extBaseInspector.IsVisible(each.prop))
                    .ToArray();

                if (visibleProperties.Length == 0)
                {
                    return;
                }

                string groupKey = visibleProperties[0].name;
                // Dictionary<string, SavedBool> foldouts = _extBaseInspector.foldouts;

                if (!_extBaseInspector._foldouts.ContainsKey(groupKey))
                {
                    _extBaseInspector._foldouts[groupKey] = new SavedBool($"{serializedObject.targetObject.GetInstanceID()}.{groupKey}", false);
                }

                _extBaseInspector._foldouts[groupKey].Value = EditorGUILayout.Foldout(_extBaseInspector._foldouts[groupKey].Value, groupKey, true);
                // ReSharper disable once InvertIf
                if (_extBaseInspector._foldouts[groupKey].Value)
                {
                    foreach ((string _, SerializedProperty prop) in visibleProperties)
                    {
                        // NaughtyEditorGUI.PropertyField_Layout(prop, true);
                        EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName), true);
                    }
                }
            }
        }

#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
        protected class NonSerializedFieldRenderer : Renderer
        {
            public NonSerializedFieldRenderer(UnityEditor.Editor editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
            }

            public override void Render() => NaughtyEditorGUI.NonSerializedField_Layout(serializedObject.targetObject,
                fieldWithInfo.fieldInfo);
        }

        protected class MethodRenderer : Renderer
        {
            public MethodRenderer(UnityEditor.Editor editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
            }

            public override void Render() => NaughtyEditorGUI.Button(serializedObject.targetObject, fieldWithInfo.methodInfo);
        }

        protected class NativeProperty : Renderer
        {
            public NativeProperty(UnityEditor.Editor editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
            }

            public override void Render() => NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
        }
#endif

#if EXT_INSPECTOR_DOTWEEN
        protected class DOTweenRenderer : Renderer
        {
            private readonly ExtBaseInspector _extBaseInspector;

            public DOTweenRenderer(ExtBaseInspector editor, FieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
            {
                _extBaseInspector = editor;
            }


            public override void Render()
            {
                _extBaseInspector.DrawDoTweenPreviews(fieldWithInfo.methodInfos);
            }
        }
#endif

        #endregion

        public virtual void OnEnable()
        {
            if (serializedObject.targetObject)
            {
                try
                {
                    try
                    {
                        _monoScript = MonoScript.FromMonoBehaviour((MonoBehaviour)serializedObject.targetObject);
                    }
                    catch (Exception)
                    {
                        _monoScript = MonoScript.FromScriptableObject((ScriptableObject)serializedObject.targetObject);
                    }
                }
                catch (InvalidCastException)
                {
                    _monoScript = null;
                }
            }

            List<Type> types = Util.GetSelfAndBaseTypes(target);
            // List<FieldWithInfo> fieldWithInfos = new List<FieldWithInfo>();
            string[] serializableFields = GetSerializedProperties().ToArray();
            foreach (Type systemType in types)
            {
                FieldInfo[] allFields = systemType
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                               BindingFlags.Public | BindingFlags.DeclaredOnly);

                // Debug.Log(allFields.FirstOrDefault(each => each.Name == "m_Script"));

                // foreach (FieldInfo fieldInfo in allFields)
                // {
                //     Debug.Log($"[type={systemType}]");
                //     Debug.Log($"Name            : {fieldInfo.Name}");
                //     Debug.Log($"Declaring Type  : {fieldInfo.DeclaringType}");
                //     Debug.Log($"IsPublic        : {fieldInfo.IsPublic}");
                //     Debug.Log($"MemberType      : {fieldInfo.MemberType}");
                //     Debug.Log($"FieldType       : {fieldInfo.FieldType}");
                //     Debug.Log($"IsFamily        : {fieldInfo.IsFamily}");
                // }

                #region SerializedField

                // IEnumerable<FieldInfo> serializableFieldInfos =
                //     allFields.Where(fieldInfo => fieldInfo.IsSerializable()
                //                                  && !fieldInfo.IsLiteral // const
                //                                  && !fieldInfo.IsStatic // static
                //                                  && !fieldInfo.IsInitOnly // readonly
                //     );
                IEnumerable<FieldInfo> serializableFieldInfos =
                    allFields.Where(fieldInfo =>
                        {
                            if (serializableFields.Contains(fieldInfo.Name))
                            {
                                return true;
                            }

                            // Name            : <GetHitPoint>k__BackingField
                            if (fieldInfo.Name.StartsWith("<") && fieldInfo.Name.EndsWith(">k__BackingField"))
                            {
                                return serializedObject.FindProperty(fieldInfo.Name) != null;
                            }

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
                    int order = orderProp?.Order ?? -4;

#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                    BoxGroupAttribute boxGroupAttribute = fieldInfo.GetCustomAttribute<BoxGroupAttribute>();
                    FoldoutAttribute foldoutAttribute = fieldInfo.GetCustomAttribute<FoldoutAttribute>();

                    #region BoxGrouped
                    if (boxGroupAttribute != null)
                    {
                        string groupName = boxGroupAttribute.Name;
                        FieldWithInfo existsInfo = _fieldWithInfos.FirstOrDefault(each => each.groupName == groupName);
                        if (existsInfo.renderType == FieldWithInfo.RenderType.None)
                        {
                            // Debug.Log($"new group {groupName}: {fieldInfo.Name}");
                            _fieldWithInfos.Add(new FieldWithInfo
                            {
                                renderType = FieldWithInfo.RenderType.GroupAttribute,
                                groupedType = FieldWithInfo.GroupedType.BoxGroup,
                                groupName = groupName,
                                order = order,
                                fieldInfos = new List<FieldInfo>{fieldInfo},
                            });
                        }
                        else
                        {
                            // Debug.Log($"add group {groupName}: {fieldInfo.Name}");
                            Debug.Assert(existsInfo.renderType == FieldWithInfo.RenderType.GroupAttribute);
                            existsInfo.fieldInfos.Add(fieldInfo);
                        }
                        continue;
                    }
                    #endregion

                    #region Foldout

                    if (foldoutAttribute != null)
                    {
                        string groupName = foldoutAttribute.Name;
                        FieldWithInfo existsInfo = _fieldWithInfos.FirstOrDefault(each => each.groupName == groupName);
                        if (existsInfo.renderType == FieldWithInfo.RenderType.None)
                        {
                            _fieldWithInfos.Add(new FieldWithInfo
                            {
                                renderType = FieldWithInfo.RenderType.GroupAttribute,
                                groupedType = FieldWithInfo.GroupedType.Foldout,
                                groupName = groupName,
                                order = order,
                                fieldInfos = new List<FieldInfo>{fieldInfo},
                            });
                        }
                        else
                        {
                            Debug.Assert(existsInfo.renderType == FieldWithInfo.RenderType.GroupAttribute);
                            existsInfo.fieldInfos.Add(fieldInfo);
                        }
                        continue;
                    }
                    #endregion
#endif

                    _fieldWithInfos.Add(new FieldWithInfo
                    {
                        // memberType = fieldInfo.MemberType,
                        renderType = FieldWithInfo.RenderType.SerializedField,
                        fieldInfo = fieldInfo,
                        order = order,
                        // serializable = true,
                    });
                }
                #endregion

                #region nonSerFieldInfo
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                IEnumerable<FieldInfo> nonSerFieldInfos = allFields
                    .Where(f => f.GetCustomAttributes(typeof(ShowNonSerializedFieldAttribute), true).Length > 0);
                foreach (FieldInfo nonSerFieldInfo in nonSerFieldInfos)
                {
                    OrderedAttribute orderProp = nonSerFieldInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? -3;
                    _fieldWithInfos.Add(new FieldWithInfo
                    {
                        renderType = FieldWithInfo.RenderType.NonSerializedField,
                        // memberType = nonSerFieldInfo.MemberType,
                        fieldInfo = nonSerFieldInfo,
                        order = order,
                        // serializable = false,
                    });
                }
#endif
                #endregion

                #region Method

                MethodInfo[] methodInfos = systemType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly);

#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                IEnumerable<MethodInfo> buttonMethodInfos = methodInfos.Where(m =>
                        m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

                foreach (MethodInfo methodInfo in buttonMethodInfos)
                {
                    OrderedAttribute orderProp =
                        methodInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? -2;
                    _fieldWithInfos.Add(new FieldWithInfo
                    {
                        // memberType = MemberTypes.Method,
                        renderType = FieldWithInfo.RenderType.Method,
                        methodInfo = methodInfo,
                        order = order,
                    });
                }
#endif

#if EXT_INSPECTOR_DOTWEEN
                IEnumerable<MethodInfo> doTweenMethodInfos = methodInfos.Where(m =>
                    m.GetCustomAttributes(typeof(DOTweenPreviewAttribute), true).Length > 0);
                foreach (MethodInfo methodInfo in doTweenMethodInfos)
                {
                    OrderedAttribute orderProp =
                        methodInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? -2;

                    FieldWithInfo existsInfo = _fieldWithInfos.FirstOrDefault(each => each.groupName == DoTweenMethodsGroupName);
                    if (existsInfo.renderType == FieldWithInfo.RenderType.None)
                    {
                        // Debug.Log($"new group {groupName}: {methodInfo.Name}");
                        _fieldWithInfos.Add(new FieldWithInfo
                        {
                            renderType = FieldWithInfo.RenderType.Method,
                            groupedType = FieldWithInfo.GroupedType.DOTween,
                            groupName = DoTweenMethodsGroupName,
                            order = order,
                            methodInfos = new List<MethodInfo>{methodInfo},
                        });
                    }
                    else
                    {
                        // Debug.Log($"add group {groupName}: {fieldInfo.Name}");
                        Debug.Assert(existsInfo is { renderType: FieldWithInfo.RenderType.Method, groupedType: FieldWithInfo.GroupedType.DOTween });
                        existsInfo.methodInfos.Add(methodInfo);
                    }
                }
#endif

                #endregion

                #region NativeProperty
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                IEnumerable<PropertyInfo> propertyInfos = systemType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(p => p.GetCustomAttributes(typeof(ShowNativePropertyAttribute), true).Length > 0);

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    OrderedAttribute orderProp =
                        propertyInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? -1;
                    _fieldWithInfos.Add(new FieldWithInfo
                    {
                        // memberType = MemberTypes.Property,
                        renderType = FieldWithInfo.RenderType.NativeProperty,
                        propertyInfo = propertyInfo,
                        order = order,
                    });
                }
#endif
                #endregion
            }

            _fieldWithInfos.Sort((a, b) => a.order.CompareTo(b.order));
        }

        private bool IsVisible(ShowHideConditionBase showIfAttribute)
        {
            if (showIfAttribute == null)
            {
                return true;
            }

            string checkProp = showIfAttribute.propOrMethodName;
            bool inverted = showIfAttribute.inverted;

            // Debug.Log($"IsVisible {checkProp}, {inverted}");

            List<Type> types = Util.GetSelfAndBaseTypes(target);
            // List<FieldWithInfo> fieldWithInfos = new List<FieldWithInfo>();
            foreach (Type systemType in types)
            {
                FieldInfo matchedField = systemType
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                               BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(each => each.Name == checkProp);

                if (matchedField != null)
                {
                    // Debug.Log($"matchedField={matchedField}, target={target}");
                    bool result;
                    try
                    {
                        object targetResult = matchedField.GetValue(target);
                        // Debug.Log($"targetResult={targetResult}");
                        result = targetResult != null;
                    }
                    catch (NullReferenceException)
                    {
                        result = false;
                    }
                    return inverted ? !result : result;
                }
                #region Method

                MethodInfo matchedMethod = systemType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(each => each.Name == checkProp);
                if (matchedMethod != null)
                {
                    ParameterInfo[] methodParams = matchedMethod.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    Debug.Assert(matchedMethod.ReturnType == typeof(bool));
                    bool result = (bool)matchedMethod.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    return inverted ? !result : result;
                }
                #endregion

                #region NativeProperty
                PropertyInfo matchedProperty = systemType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(each => each.Name == checkProp);

                if (matchedProperty != null)
                {
                    bool result = (bool)matchedProperty.GetValue(null);
                    return inverted ? !result : result;
                }

                #endregion
            }

            // throw new ArgumentNotFoundException($"{checkProp} not found in target {target}");
            throw new ArgumentOutOfRangeException(nameof(checkProp), checkProp,
                $"{checkProp} not found in target {target}");
        }

        private bool IsVisible(SerializedProperty serializedProp) =>
            IsVisible(Util.GetAttribute<ShowHideConditionBase>(serializedProp));

        public virtual void OnDisable()
        {
            // ReorderableListPropertyDrawer.Instance.ClearCache();
            // DOTweenEditorPreview.Stop();
            // _doTweenPreviewPlaying = false;
#if EXT_INSPECTOR_DOTWEEN
            DoTweenPreviewStop();
#endif
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                return;
            }

            // UnityEngine.Object scriptObject = GetScriptObject();
            if(_monoScript)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Script", _monoScript, typeof(MonoScript), false);
                EditorGUILayout.Space();
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.Update();
            foreach (FieldWithInfo fieldWithInfo in _fieldWithInfos)
            {
                Renderer renderer = MakeRenderer(fieldWithInfo);
                if(renderer != null)
                {
                    renderer.Render();
                    renderer.AfterRender();
                }
            }

            serializedObject.ApplyModifiedProperties();
            // base.OnInspectorGUI();
        }


        private IEnumerable<string> GetSerializedProperties()
        {
            // outSerializedProperties.Clear();
            using SerializedProperty iterator = serializedObject.GetIterator();

            // ReSharper disable once InvertIf
            if (iterator.NextVisible(true))
            {
                do
                {
                    // outSerializedProperties.Add(serializedObject.FindProperty(iterator.name));
                    yield return iterator.name;
                }
                while (iterator.NextVisible(false));
            }
        }

        // private static List<Type> GetSelfAndBaseTypes(object target)
        // {
        //     List<Type> types = new()
        //     {
        //         target.GetType(),
        //     };
        //
        //     while (types.Last().BaseType != null)
        //     {
        //         types.Add(types.Last().BaseType);
        //     }
        //
        //     return types;
        // }

        #region DOTween
#if EXT_INSPECTOR_DOTWEEN
        private void DrawDoTweenPreviews(IReadOnlyList<MethodInfo> methodInfos)
        {
            // if ( )
            // {
            //     return;
            // }
            Debug.Assert(methodInfos.Count > 0);

            Rect labelTitleRect = EditorGUILayout.GetControlRect(false);

            const string title = "DOTween Preview";

            const float titleBtnWidth = 30f;

            // float titleWidth = EditorStyles.label.CalcSize(new GUIContent(title)).x + 20f;
            Rect titleRect = new Rect(labelTitleRect)
            {
                width = labelTitleRect.width - titleBtnWidth * 2,
            };

            EditorGUI.LabelField(titleRect, title, new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
            });

            Rect titleBtn1Rect = new Rect(labelTitleRect)
            {
                x = titleRect.width,
                width = titleBtnWidth,
            };
            Rect titleBtn2Rect = new Rect(labelTitleRect)
            {
                x = titleRect.width + titleBtnWidth,
                width = titleBtnWidth,
            };

            EditorGUI.BeginDisabledGroup(_doTweenPreviewPlaying);
            if (GUI.Button(titleBtn1Rect, "▶"))
            {
                DoTweenPreviewPlay();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!_doTweenPreviewPlaying);
            if (GUI.Button(titleBtn2Rect, "■"))
            {
                DoTweenPreviewStop();
            }
            EditorGUI.EndDisabledGroup();

            foreach ((MethodInfo methodInfo, bool isLast) in methodInfos.Select((each, index) => (each, index == methodInfos.Count - 1)))
            {
                Rect lineRect = EditorGUILayout.GetControlRect(false);

                int methodHash = methodInfo.GetHashCode();

                if (!_funcToDOTweenStatus.TryGetValue(methodHash, out DOTweenStatus status))
                {
                    status = new DOTweenStatus();
                }

                DOTweenPreviewAttribute previewAttribute = (DOTweenPreviewAttribute)methodInfo.GetCustomAttributes(typeof(DOTweenPreviewAttribute), true)[0];
                status.stopAction = previewAttribute.onManualStop;
                string previewText = string.IsNullOrEmpty(previewAttribute.text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : previewAttribute.text;

                GUI.Label(lineRect, new GUIContent($"{(isLast ? "└" : "├")}{previewText}"));

                float totalWidth = lineRect.width;
                const float btnWidth = 30f;
                Rect playBtnRect = new Rect(lineRect)
                {
                    x = totalWidth - btnWidth * 3,
                    width = btnWidth,
                };
                Rect stopBtnRect = new Rect(lineRect)
                {
                    x = totalWidth - btnWidth * 2,
                    width = btnWidth,
                };
                Rect restartBtnRect = new Rect(lineRect)
                {
                    x = totalWidth - btnWidth,
                    width = btnWidth,
                };

                EditorGUI.BeginChangeCheck();
                status.isPlaying = GUI.Toggle(playBtnRect, status.isPlaying, status.isPlaying? "‖ ‖": "▶", "Button");
                if (EditorGUI.EndChangeCheck())
                {
                    if (status.isPlaying)
                    {
                        DoTweenPreviewPlay();
                        if (status.sequence == null)
                        {
                            object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();

                            Sequence methodResult = (Sequence)methodInfo.Invoke(target, defaultParams);
                            status.sequence = methodResult;

                            DOTweenEditorPreview.PrepareTweenForPreview(methodResult);
                        }
                        else
                        {
                            status.sequence.Play();
                        }
                    }
                    else
                    {
                        status.sequence.Pause();
                    }
                }

                EditorGUI.BeginDisabledGroup(!status.isPlaying);
                if (GUI.Button(stopBtnRect, "■"))
                {
                    StopDOTweenStatus(status);
                    status = new DOTweenStatus();
                }
                EditorGUI.EndDisabledGroup();

                if (GUI.Button(restartBtnRect, "↻"))
                {
                    DoTweenPreviewPlay();

                    if (status.sequence == null)
                    {
                        object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();

                        Sequence methodResult = (Sequence)methodInfo.Invoke(target, defaultParams);
                        status.sequence = methodResult;

                        DOTweenEditorPreview.PrepareTweenForPreview(methodResult);
                    }
                    else
                    {
                        status.sequence.Restart();
                    }

                    status.isPlaying = true;
                }

                _funcToDOTweenStatus[methodHash] = status;
            }
        }

        private void DoTweenPreviewPlay()
        {
            if (_doTweenPreviewPlaying)
            {
                return;
            }
            _doTweenPreviewPlaying = true;
            DOTweenEditorPreview.Start();
        }

        private void DoTweenPreviewStop()
        {
            if (!_doTweenPreviewPlaying)
            {
                return;
            }

            _doTweenPreviewPlaying = false;

            foreach (DOTweenStatus status in _funcToDOTweenStatus.Values)
            {
                StopDOTweenStatus(status);
            }

            _funcToDOTweenStatus.Clear();

            DOTweenEditorPreview.Stop();
        }

        private static void StopDOTweenStatus(DOTweenStatus status)
        {
            if (status.sequence == null)
            {
                return;
            }

            switch (status.stopAction)
            {
                case DOTweenPreviewAttribute.StopAction.None:
                    status.sequence.Pause();
                    break;
                case DOTweenPreviewAttribute.StopAction.Complete:
                    status.sequence.Complete();
                    break;
                case DOTweenPreviewAttribute.StopAction.Rewind:
                    status.sequence.Rewind();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif
        #endregion

        #region 对外接口

        protected virtual Renderer MakeRenderer(FieldWithInfo fieldWithInfo)
        {
            // Debug.Log($"field {fieldWithInfo.fieldInfo?.Name}/{fieldWithInfo.fieldInfo?.GetCustomAttribute<ExtShowHideConditionBase>()}");
            switch (fieldWithInfo.renderType, fieldWithInfo.groupedType)
            {
                case (FieldWithInfo.RenderType.SerializedField, _):
                {
                    // Debug.Log($"field {fieldWithInfo.fieldInfo.Name}/{fieldWithInfo.fieldInfo.GetCustomAttribute<ExtShowHideConditionBase>()}");
                    return IsVisible(fieldWithInfo.fieldInfo.GetCustomAttribute<ShowHideConditionBase>())
                        ? new SerializedFieldRenderer(this, fieldWithInfo)
                        : null;
                }
                case (FieldWithInfo.RenderType.GroupAttribute, FieldWithInfo.GroupedType.BoxGroup):
                {
                    return new BoxGroupRenderer(this, fieldWithInfo);
                }
                case (FieldWithInfo.RenderType.GroupAttribute, FieldWithInfo.GroupedType.Foldout):
                {
                    return new FoldoutRenderer(this, fieldWithInfo);
                }
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
                case (FieldWithInfo.RenderType.NonSerializedField, _):
                    return IsVisible(fieldWithInfo.fieldInfo.GetCustomAttribute<ShowHideConditionBase>())
                        ? new NonSerializedFieldRenderer(this, fieldWithInfo)
                        : null;
#if EXT_INSPECTOR_DOTWEEN
                case (FieldWithInfo.RenderType.Method, FieldWithInfo.GroupedType.DOTween):
                    return IsVisible(fieldWithInfo.methodInfo.GetCustomAttribute<ShowHideConditionBase>())
                        ? new DOTweenRenderer(this, fieldWithInfo)
                        : null;
#endif

                case (FieldWithInfo.RenderType.Method, _):
                    return IsVisible(fieldWithInfo.methodInfo.GetCustomAttribute<ShowHideConditionBase>())
                        ? new MethodRenderer(this, fieldWithInfo)
                        : null;

                case (FieldWithInfo.RenderType.NativeProperty, _):
                    return IsVisible(fieldWithInfo.propertyInfo.GetCustomAttribute<ShowHideConditionBase>())
                        ? new NativeProperty(this, fieldWithInfo)
                        : null;
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
