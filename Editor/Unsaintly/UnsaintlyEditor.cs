using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Unsaintly.Renderer;
using SaintsField.Editor.Utils;
using SaintsField.Unsaintly;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
#endif

namespace SaintsField.Editor.Unsaintly
{
    public class UnsaintlyEditor: UnityEditor.Editor
    {
        private MonoScript _monoScript;
        private IReadOnlyList<UnsaintlyFieldWithInfo> _fieldWithInfos = new List<UnsaintlyFieldWithInfo>();

        #region UI
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        #region UIToolkit
        public override VisualElement CreateInspectorGUI()
        {
            Setup();
            // return new Label("This is a Label in a Custom Editor");
            if (target == null)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            if(_monoScript)
            {
                ObjectField objectField = new ObjectField("Script")
                {
                    bindingPath = "m_Script",
                    value = _monoScript,
                    allowSceneObjects = false,
                    objectType = typeof(MonoScript),
                };
                objectField.Bind(serializedObject);
                objectField.SetEnabled(false);
                root.Add(objectField);
            }

            foreach (UnsaintlyFieldWithInfo fieldWithInfo in _fieldWithInfos)
            {
                AbsRenderer renderer = MakeRenderer(fieldWithInfo, TryFixUIToolkit);
                if(renderer != null)
                {
                    root.Add(renderer.CreateVisualElement());
                }
            }

            return root;
        }

        protected virtual bool TryFixUIToolkit => true;
        #endregion
#endif

        #region IMGUI
        public virtual void OnEnable()
        {
            Setup();
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                return;
            }

            if(_monoScript)
            {
                using(new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField("Script", _monoScript, typeof(MonoScript), false);
                }
            }

            serializedObject.Update();

            foreach (UnsaintlyFieldWithInfo fieldWithInfo in _fieldWithInfos)
            {
                // ReSharper disable once ConvertToUsingDeclaration
                AbsRenderer renderer = MakeRenderer(fieldWithInfo, false);
                // ReSharper disable once InvertIf
                if(renderer != null){
                    // Debug.Log($"gen renderer {renderer}");
                    renderer.Render();
                    renderer.AfterRender();
                    // renderer.AfterRender();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #endregion

        private void Setup()
        {
            if (serializedObject.targetObject)
            {
                try
                {
                    _monoScript = MonoScript.FromMonoBehaviour((MonoBehaviour) serializedObject.targetObject);
                }
                catch (Exception)
                {
                    try
                    {
                        _monoScript = MonoScript.FromScriptableObject((ScriptableObject)serializedObject.targetObject);
                    }
                    catch (Exception)
                    {
                        _monoScript = null;
                    }
                }
            }
            else
            {
                _monoScript = null;
            }

            // _fieldWithInfos.Clear();
            List<UnsaintlyFieldWithInfo> fieldWithInfos = new List<UnsaintlyFieldWithInfo>();
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            string[] serializableFields = GetSerializedProperties().ToArray();
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
                    int order = orderProp?.Order ?? int.MinValue;

                    fieldWithInfos.Add(new UnsaintlyFieldWithInfo
                    {
                        renderType = UnsaintlyRenderType.SerializedField,
                        fieldInfo = fieldInfo,
                        inherentDepth = inherentDepth,
                        order = order,
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
                    fieldWithInfos.Add(new UnsaintlyFieldWithInfo
                    {
                        renderType = UnsaintlyRenderType.NonSerializedField,
                        // memberType = nonSerFieldInfo.MemberType,
                        fieldInfo = nonSerFieldInfo,
                        inherentDepth = inherentDepth,
                        order = order,
                        // serializable = false,
                    });
                }
                #endregion

                #region Method

                MethodInfo[] methodInfos = systemType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly);

                IEnumerable<MethodInfo> buttonMethodInfos = methodInfos.Where(m =>
                        m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

                foreach (MethodInfo methodInfo in buttonMethodInfos)
                {
                    OrderedAttribute orderProp =
                        methodInfo.GetCustomAttribute<OrderedAttribute>();
                    int order = orderProp?.Order ?? int.MinValue;
                    fieldWithInfos.Add(new UnsaintlyFieldWithInfo
                    {
                        // memberType = MemberTypes.Method,
                        renderType = UnsaintlyRenderType.Method,
                        methodInfo = methodInfo,
                        inherentDepth = inherentDepth,
                        order = order,
                    });
                }

                // IEnumerable<MethodInfo> doTweenMethodInfos = methodInfos.Where(m =>
                //     m.GetCustomAttributes(typeof(DOTweenPreviewAttribute), true).Length > 0);
                // foreach (MethodInfo methodInfo in doTweenMethodInfos)
                // {
                //     OrderedAttribute orderProp =
                //         methodInfo.GetCustomAttribute<OrderedAttribute>();
                //     int order = orderProp?.Order ?? -2;
                //
                //     FieldWithInfo existsInfo = _fieldWithInfos.FirstOrDefault(each => each.groupName == DoTweenMethodsGroupName);
                //     if (existsInfo.renderType == FieldWithInfo.RenderType.None)
                //     {
                //         // Debug.Log($"new group {groupName}: {methodInfo.Name}");
                //         _fieldWithInfos.Add(new FieldWithInfo
                //         {
                //             renderType = FieldWithInfo.RenderType.Method,
                //             groupedType = FieldWithInfo.GroupedType.DOTween,
                //             groupName = DoTweenMethodsGroupName,
                //             inherentDepth = inherentDepth,
                //             order = order,
                //             methodInfos = new List<MethodInfo>{methodInfo},
                //         });
                //     }
                //     else
                //     {
                //         // Debug.Log($"add group {groupName}: {fieldInfo.Name}");
                //         Debug.Assert(existsInfo is { renderType: FieldWithInfo.RenderType.Method, groupedType: FieldWithInfo.GroupedType.DOTween });
                //         existsInfo.methodInfos.Add(methodInfo);
                //     }
                // }
                //
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
                    fieldWithInfos.Add(new UnsaintlyFieldWithInfo
                    {
                        // memberType = MemberTypes.Property,
                        renderType = UnsaintlyRenderType.NativeProperty,
                        propertyInfo = propertyInfo,
                        inherentDepth = inherentDepth,
                        order = order,
                    });
                }
                #endregion
            }

            _fieldWithInfos = fieldWithInfos
                .WithIndex()
                .OrderBy(each => each.value.inherentDepth)
                .ThenBy(each => each.value.order)
                .ThenBy(each => each.index)
                .Select(each => each.value)
                .ToArray();
        }

        private IEnumerable<string> GetSerializedProperties()
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

        protected virtual AbsRenderer MakeRenderer(UnsaintlyFieldWithInfo fieldWithInfo, bool tryFixUIToolkit)
        {
            // Debug.Log($"field {fieldWithInfo.fieldInfo?.Name}/{fieldWithInfo.fieldInfo?.GetCustomAttribute<ExtShowHideConditionBase>()}");
            switch (fieldWithInfo.renderType)
            {
                case UnsaintlyRenderType.SerializedField:
                {
                    return new SerializedFieldRenderer(this, fieldWithInfo, tryFixUIToolkit);
                }
                // case (FieldWithInfo.RenderType.GroupAttribute, FieldWithInfo.GroupedType.BoxGroup):
                // {
                //     return new BoxGroupRenderer(this, fieldWithInfo);
                // }
                // case (FieldWithInfo.RenderType.GroupAttribute, FieldWithInfo.GroupedType.Foldout):
                // {
                //     return new FoldoutRenderer(this, fieldWithInfo);
                // }

                case UnsaintlyRenderType.NonSerializedField:
                    // return IsVisible(fieldWithInfo.fieldInfo.GetCustomAttribute<ExtShowHideConditionBase>())
                    //     ? new NonSerializedFieldRenderer(this, fieldWithInfo)
                    //     : null;
                    return new NonSerializedFieldRenderer(this, fieldWithInfo, tryFixUIToolkit);

                case UnsaintlyRenderType.Method:
                    // return IsVisible(fieldWithInfo.methodInfos[0].GetCustomAttribute<ExtShowHideConditionBase>())
                    //     ? new DOTweenRenderer(this, fieldWithInfo)
                    //     : null;
                    return new MethodRenderer(this, fieldWithInfo, tryFixUIToolkit);

                // case (FieldWithInfo.RenderType.Method, _):
                //     return IsVisible(fieldWithInfo.methodInfo.GetCustomAttribute<ExtShowHideConditionBase>())
                //         ? new MethodRenderer(this, fieldWithInfo)
                //         : null;

                case UnsaintlyRenderType.NativeProperty:
                    // return IsVisible(fieldWithInfo.propertyInfo.GetCustomAttribute<ExtShowHideConditionBase>())
                    //     ? new NativeProperty(this, fieldWithInfo)
                    //     : null;
                    return new NativePropertyRenderer(this, fieldWithInfo, tryFixUIToolkit);
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.renderType), fieldWithInfo.renderType, null);
            }
        }
    }
}
