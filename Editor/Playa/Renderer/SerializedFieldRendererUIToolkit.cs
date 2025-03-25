#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
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


            Type useDrawerType = null;
            Attribute useAttribute = null;
            if(!FieldWithInfo.SerializedProperty.isArray)
            {
                PropertyAttribute[] allAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo);

                ISaintsAttribute saintsAttr = allAttributes
                    .OfType<ISaintsAttribute>()
                    .FirstOrDefault();

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
                    useAttribute = attrOrNull;
                    useDrawerType = drawerType;
                }
            }

            // Debug.Log(useDrawerType);

            if (useDrawerType == null)
            {
                VisualElement r = UIToolkitUtils.CreateOrUpdateFieldFromProperty(
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo.FieldType,
                    FieldWithInfo.SerializedProperty.displayName,
                    FieldWithInfo.FieldInfo,
                    this,
                    this,
                    null
                );
                return (r, false);
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

            MethodInfo uiToolkitMethod = useDrawerType.GetMethod("CreatePropertyGUI");

            // bool isSaintsDrawer = useDrawerType.IsSubclassOf(typeof(SaintsPropertyDrawer)) || useDrawerType == typeof(SaintsPropertyDrawer);

            bool useImGui = uiToolkitMethod == null ||
                            uiToolkitMethod.DeclaringType == typeof(PropertyDrawer);  // null: old Unity || did not override

            // Debug.Log($"{useDrawerType}/{uiToolkitMethod.DeclaringType}/{FieldWithInfo.SerializedProperty.propertyPath}");

            if (!useImGui)
            {
                return (propertyDrawer.CreatePropertyGUI(FieldWithInfo.SerializedProperty), false);
            }

            // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
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

        public AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
#endif
