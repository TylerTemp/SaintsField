#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer: IMakeRenderer, IDOTweenPlayRecorder
    {
        private PropertyField _result;

        private VisualElement _fieldElement;
        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // };
            // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // return (result, false);


            // About letting SaintsPropertyDrawer fallback:
            // SaintsPropertyDrawer relys on PropertyField to fallback. Directly hi-jiacking the drawer with SaintsPropertyDrawer
            // the workflow will still get into the PropertyField flow, then SaintsField will fail to decide when the
            // fallback should stop.
            PropertyAttribute[] allAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo);

            Type useDrawerType = null;
            Attribute useAttribute = null;
            bool isArray = FieldWithInfo.SerializedProperty.propertyType == SerializedPropertyType.Generic
                && FieldWithInfo.SerializedProperty.isArray;
            if(!isArray)
            {
                ISaintsAttribute saintsAttr = allAttributes
                    .OfType<ISaintsAttribute>()
                    .FirstOrDefault();

                // Debug.Log(saintsAttr);

                useAttribute = saintsAttr as Attribute;
                if (saintsAttr != null)
                {
                    useDrawerType = SaintsPropertyDrawer.GetFirstSaintsDrawerType(saintsAttr.GetType());
                }
                else
                {
                    (Attribute attrOrNull, Type drawerType) =
                        SaintsPropertyDrawer.GetFallbackDrawerType(FieldWithInfo.FieldInfo,
                            FieldWithInfo.SerializedProperty);
                    // Debug.Log($"{FieldWithInfo.SerializedProperty.propertyPath}: {drawerType}");
                    useAttribute = attrOrNull;
                    useDrawerType = drawerType;

                    if (useDrawerType == null &&
                        FieldWithInfo.SerializedProperty.propertyType == SerializedPropertyType.Generic)
                    {
                        useDrawerType = typeof(SaintsRowAttributeDrawer);
                    }
                }
            }

            // List<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)> appendSaintsAttributeDrawer = null;
            //
            // if (!isArray && InHorizentalLayout)
            // {
            //     appendSaintsAttributeDrawer = new List<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)>();
            //     NoLabelAttribute noLabelAttribute = new NoLabelAttribute();
            //     RichLabelAttributeDrawer noLabelDrawer = (RichLabelAttributeDrawer)
            //         SaintsPropertyDrawer.MakePropertyDrawer(typeof(RichLabelAttributeDrawer),
            //             FieldWithInfo.FieldInfo, useAttribute, FieldWithInfo.SerializedProperty.displayName);
            //
            //     appendSaintsAttributeDrawer.Add((noLabelAttribute, noLabelDrawer));
            //
            //     // // ReSharper disable once RedundantArgumentDefaultValue
            //     // AboveRichLabelAttribute aboveRichLabelAttribute = new AboveRichLabelAttribute("<label />");
            //     // FullWidthRichLabelAttributeDrawer aboveRichLabelDrawer = (FullWidthRichLabelAttributeDrawer)
            //     //     SaintsPropertyDrawer.MakePropertyDrawer(typeof(FullWidthRichLabelAttributeDrawer),
            //     //         FieldWithInfo.FieldInfo, aboveRichLabelAttribute, FieldWithInfo.SerializedProperty.displayName);
            //     //
            //     // appendSaintsAttributeDrawer.Add((aboveRichLabelAttribute, aboveRichLabelDrawer));
            // }

            if (!isArray && useDrawerType == null && InAnyHorizontalLayout)
            {
                useDrawerType = typeof(SaintsPropertyDrawer);
            }

            if (useDrawerType == null)
            {
                VisualElement r = UIToolkitUtils.CreateOrUpdateFieldFromProperty(
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo.FieldType,
                    FieldWithInfo.SerializedProperty.displayName,
                    FieldWithInfo.FieldInfo,
                    InAnyHorizontalLayout,
                    this,
                    this,
                    null
                );
                return (MergeWithDec(r, allAttributes), false);
            }

            // Nah... This didn't handle for mis-ordered case
            // // Above situation will handle all including SaintsRow for general class/struct/interface.
            // // At this point we only need to let Unity handle it
            // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // };
            // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // return (result, false);

            PropertyDrawer propertyDrawer = SaintsPropertyDrawer.MakePropertyDrawer(useDrawerType, FieldWithInfo.FieldInfo, useAttribute, FieldWithInfo.SerializedProperty.displayName);
            // Debug.Log(saintsPropertyDrawer);
            if (propertyDrawer is SaintsPropertyDrawer saintsPropertyDrawer)
            {
                // saintsPropertyDrawer.AppendSaintsAttributeDrawer = appendSaintsAttributeDrawer;
                saintsPropertyDrawer.InHorizontalLayout = InAnyHorizontalLayout;
            }

            MethodInfo uiToolkitMethod = useDrawerType.GetMethod("CreatePropertyGUI");

            // bool isSaintsDrawer = useDrawerType.IsSubclassOf(typeof(SaintsPropertyDrawer)) || useDrawerType == typeof(SaintsPropertyDrawer);

            bool useImGui = uiToolkitMethod == null ||
                            uiToolkitMethod.DeclaringType == typeof(PropertyDrawer);  // null: old Unity || did not override

            // Debug.Log($"{useDrawerType}/{uiToolkitMethod.DeclaringType}/{FieldWithInfo.SerializedProperty.propertyPath}");

            if (!useImGui)
            {
                VisualElement r = propertyDrawer.CreatePropertyGUI(FieldWithInfo.SerializedProperty);
                return (MergeWithDec(r, allAttributes), false);
            }

            // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
            // We don't need to handle decorators either
            PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = FieldWithInfo.SerializedProperty.propertyPath,
            };
            result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            return (result, false);

            // // this is the SaintsPropertyDrawer way, but some IMGUI has height issue with IMGUIContainer (e.g. Wwise.EventDrawer)
            // // so we just ignore anything and let unity handle it
            // SerializedProperty property = FieldWithInfo.SerializedProperty;
            // MethodInfo imGuiGetPropertyHeightMethod = useDrawerType.GetMethod("GetPropertyHeight");
            // MethodInfo imGuiOnGUIMethodInfo = useDrawerType.GetMethod("OnGUI");
            // Debug.Assert(imGuiGetPropertyHeightMethod != null);
            // Debug.Assert(imGuiOnGUIMethodInfo != null);
            //
            // IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);
            //
            // IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
            // {
            //     property.serializedObject.Update();
            //
            //     GUIContent label = imguiLabelHelper.NoLabel
            //         ? GUIContent.none
            //         : new GUIContent(imguiLabelHelper.RichLabel);
            //
            //     using (new ImGuiFoldoutStyleRichTextScoop())
            //     using (new ImGuiLabelStyleRichTextScoop())
            //     {
            //         float height =
            //             (float)imGuiGetPropertyHeightMethod.Invoke(propertyDrawer, new object[] { property, label });
            //         Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            //         imGuiOnGUIMethodInfo.Invoke(propertyDrawer, new object[] { rect, property, label });
            //     }
            // })
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexShrink = 0,
            //     },
            //     userData = imguiLabelHelper,
            // };
            // imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);
            //
            // return (imGuiContainer, false);
        }

        private static VisualElement MergeWithDec(VisualElement result, PropertyAttribute[] allAttributes)
        {
            VisualElement dec = DrawDecorator(allAttributes);
            if(dec == null)
            {
                return result;
            }

            VisualElement container = new VisualElement();
            container.Add(dec);
            container.Add(result);
            return container;
        }


        private static VisualElement DrawDecorator(IEnumerable<PropertyAttribute> allPropAttributes)
        {
            IReadOnlyDictionary<Type, IReadOnlyList<SaintsPropertyDrawer.PropertyDrawerInfo>> propertyAttributeToDecoratorDrawers = SaintsPropertyDrawer.EnsureAndGetTypeToDrawers().attrToDecoratorDrawers;
            // this can be multiple, should not be a dictionary
            IEnumerable<(PropertyAttribute propAttribute, Type)> decDrawers =
                allPropAttributes
                    .Select(propAttribute => (propAttribute, SaintsPropertyDrawer.PropertyGetDecoratorDrawer(propAttribute.GetType())))
                    .Where(each => each.Item2 != null);
            // new List<(PropertyAttribute, Type drawerType)>();
            // if (decDrawers.Length == 0)
            // {
            //     return null;
            // }

            VisualElement result = new VisualElement();
            result.AddToClassList("unity-decorator-drawers-container");
            // this.m_DecoratorDrawersContainer = new VisualElement();
            // this.m_DecoratorDrawersContainer.AddToClassList(PropertyField.decoratorDrawersContainerClassName);

            bool hasAny = false;
            foreach ((PropertyAttribute propAttribute, Type drawerType) in decDrawers)
            {
                hasAny = true;
                DecoratorDrawer decorator = (DecoratorDrawer)Activator.CreateInstance(drawerType);
                FieldInfo mAttributeField = drawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mAttributeField != null)
                {
                    mAttributeField.SetValue(decorator, propAttribute);
                }

                VisualElement ve =
#if UNITY_2022_3_OR_NEWER
                        decorator.CreatePropertyGUI()
#else
                        null
#endif
                    ;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (ve == null)
                {
                    ve = new IMGUIContainer(() =>
                    {
                        Rect position = new Rect
                        {
                            height = decorator.GetHeight(),
                            width = result.resolvedStyle.width,
                        };
                        decorator.OnGUI(position);
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once AccessToModifiedClosure
                        ve.style.height = position.height;
                    });
                    ve.style.height = decorator.GetHeight();
                }
                result.Add(ve);
            }

            // foreach (DecoratorDrawer decoratorDrawer in decoratorDrawers)
            // {
            //     DecoratorDrawer decorator = decoratorDrawer;
            //     VisualElement ve = decorator.CreatePropertyGUI();
            //     if (ve == null)
            //     {
            //         ve = (VisualElement) new IMGUIContainer((Action) (() =>
            //         {
            //             Rect position = new Rect();
            //             position.height = decorator.GetHeight();
            //             position.width = this.resolvedStyle.width;
            //             decorator.OnGUI(position);
            //             ve.style.height = (StyleLength) position.height;
            //         }));
            //         ve.style.height = (StyleLength) decorator.GetHeight();
            //     }
            //     this.m_DecoratorDrawersContainer.Add(ve);
            // }

            return hasAny? result: null;

            // // PropertyHandler propertyHandle = UnityEditor.ScriptAttributeUtility.GetHandler(FieldWithInfo.SerializedProperty);
            // PropertyHandler propertyHandle = (PropertyHandler)typeof(UnityEditor.Editor).Assembly
            //     .GetType("UnityEditor.ScriptAttributeUtility")
            //     .GetMethod("GetHandler", BindingFlags.NonPublic | BindingFlags.Static)
            //     .Invoke(null, new object[] { FieldWithInfo.SerializedProperty });

        }

        public AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
#endif
