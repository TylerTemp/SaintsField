#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
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
            VisualElement r = UIToolkitUtils.CreateOrUpdateFieldProperty(
                FieldWithInfo.SerializedProperty,
                ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo),
                FieldWithInfo.FieldInfo.FieldType,
                NoLabel? null: FieldWithInfo.SerializedProperty.displayName,
                FieldWithInfo.FieldInfo,
                InAnyHorizontalLayout,
                this,
                this,
                null,
                FieldWithInfo.Targets[0]
            );
            if (r != null)
            {
                r.style.width = new StyleLength(Length.Percent(100));
                void Search(string search)
                {
                    string labelName = FieldWithInfo.SerializedProperty.displayName;

                    DisplayStyle display = Util.UnityDefaultSimpleSearch(labelName, search)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    if (r.style.display != display)
                    {
                        r.style.display = display;
                    }
                }

                OnSearchFieldUIToolkit.AddListener(Search);
                r.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));
            }

            // Debug.Log($"{FieldWithInfo.SerializedProperty.propertyPath}/{r}");

            return (r, false);

            // // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // // {
            // //     style =
            // //     {
            // //         flexGrow = 1,
            // //     },
            // //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // // };
            // // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // // return (result, false);
            //
            //
            // // About letting SaintsPropertyDrawer fallback:
            // // SaintsPropertyDrawer relays on PropertyField to fallback. Directly hi-jacking the drawer with SaintsPropertyDrawer
            // // the workflow will still get into the PropertyField flow, then SaintsField will fail to decide when the
            // // fallback should stop.
            // PropertyAttribute[] allAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo);
            //
            // Type useDrawerType = null;
            // Attribute useAttribute = null;
            // bool isArray = FieldWithInfo.SerializedProperty.propertyType == SerializedPropertyType.Generic
            //     && FieldWithInfo.SerializedProperty.isArray;
            // if(!isArray)
            // {
            //     ISaintsAttribute saintsAttr = allAttributes
            //         .OfType<ISaintsAttribute>()
            //         .FirstOrDefault();
            //
            //     // Debug.Log(saintsAttr);
            //
            //     useAttribute = saintsAttr as Attribute;
            //     if (saintsAttr != null)
            //     {
            //         useDrawerType = SaintsPropertyDrawer.GetFirstSaintsDrawerType(saintsAttr.GetType());
            //     }
            //     else
            //     {
            //         (Attribute attrOrNull, Type drawerType) =
            //             SaintsPropertyDrawer.GetFallbackDrawerType(FieldWithInfo.FieldInfo,
            //                 FieldWithInfo.SerializedProperty);
            //         // Debug.Log($"{FieldWithInfo.SerializedProperty.propertyPath}: {drawerType}");
            //         useAttribute = attrOrNull;
            //         useDrawerType = drawerType;
            //
            //         if (useDrawerType == null &&
            //             FieldWithInfo.SerializedProperty.propertyType == SerializedPropertyType.Generic)
            //         {
            //             useDrawerType = typeof(SaintsRowAttributeDrawer);
            //         }
            //     }
            // }
            //
            // // List<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)> appendSaintsAttributeDrawer = null;
            // //
            // // if (!isArray && InHorizentalLayout)
            // // {
            // //     appendSaintsAttributeDrawer = new List<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)>();
            // //     NoLabelAttribute noLabelAttribute = new NoLabelAttribute();
            // //     RichLabelAttributeDrawer noLabelDrawer = (RichLabelAttributeDrawer)
            // //         SaintsPropertyDrawer.MakePropertyDrawer(typeof(RichLabelAttributeDrawer),
            // //             FieldWithInfo.FieldInfo, useAttribute, FieldWithInfo.SerializedProperty.displayName);
            // //
            // //     appendSaintsAttributeDrawer.Add((noLabelAttribute, noLabelDrawer));
            // //
            // //     // // ReSharper disable once RedundantArgumentDefaultValue
            // //     // AboveRichLabelAttribute aboveRichLabelAttribute = new AboveRichLabelAttribute("<label />");
            // //     // FullWidthRichLabelAttributeDrawer aboveRichLabelDrawer = (FullWidthRichLabelAttributeDrawer)
            // //     //     SaintsPropertyDrawer.MakePropertyDrawer(typeof(FullWidthRichLabelAttributeDrawer),
            // //     //         FieldWithInfo.FieldInfo, aboveRichLabelAttribute, FieldWithInfo.SerializedProperty.displayName);
            // //     //
            // //     // appendSaintsAttributeDrawer.Add((aboveRichLabelAttribute, aboveRichLabelDrawer));
            // // }
            //
            // if (!isArray && useDrawerType == null && InAnyHorizontalLayout)
            // {
            //     useDrawerType = typeof(SaintsPropertyDrawer);
            // }
            //
            // if (useDrawerType == null)
            // {
            //     VisualElement r = UIToolkitUtils.CreateOrUpdateFieldRawFallback(
            //         FieldWithInfo.SerializedProperty,
            //         FieldWithInfo.FieldInfo.FieldType,
            //         FieldWithInfo.SerializedProperty.displayName,
            //         FieldWithInfo.FieldInfo,
            //         InAnyHorizontalLayout,
            //         this,
            //         this,
            //         null
            //     );
            //     return (UIToolkitCache.MergeWithDec(r, allAttributes), false);
            // }
            //
            // // Nah... This didn't handle for mis-ordered case
            // // // Above situation will handle all including SaintsRow for general class/struct/interface.
            // // // At this point we only need to let Unity handle it
            // // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // // {
            // //     style =
            // //     {
            // //         flexGrow = 1,
            // //     },
            // //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // // };
            // // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // // return (result, false);
            //
            // PropertyDrawer propertyDrawer = SaintsPropertyDrawer.MakePropertyDrawer(useDrawerType, FieldWithInfo.FieldInfo, useAttribute, FieldWithInfo.SerializedProperty.displayName);
            // // Debug.Log(saintsPropertyDrawer);
            // if (propertyDrawer is SaintsPropertyDrawer saintsPropertyDrawer)
            // {
            //     // saintsPropertyDrawer.AppendSaintsAttributeDrawer = appendSaintsAttributeDrawer;
            //     saintsPropertyDrawer.InHorizontalLayout = InAnyHorizontalLayout;
            // }
            //
            // MethodInfo uiToolkitMethod = useDrawerType.GetMethod("CreatePropertyGUI");
            //
            // // bool isSaintsDrawer = useDrawerType.IsSubclassOf(typeof(SaintsPropertyDrawer)) || useDrawerType == typeof(SaintsPropertyDrawer);
            //
            // bool useImGui = uiToolkitMethod == null ||
            //                 uiToolkitMethod.DeclaringType == typeof(PropertyDrawer);  // null: old Unity || did not override
            //
            // // Debug.Log($"{useDrawerType}/{uiToolkitMethod.DeclaringType}/{FieldWithInfo.SerializedProperty.propertyPath}");
            //
            // if (!useImGui)
            // {
            //     VisualElement r = propertyDrawer.CreatePropertyGUI(FieldWithInfo.SerializedProperty);
            //     return (UIToolkitCache.MergeWithDec(r, allAttributes), false);
            // }
            //
            // // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
            // // We don't need to handle decorators either
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
            //
            // // // this is the SaintsPropertyDrawer way, but some IMGUI has height issue with IMGUIContainer (e.g. Wwise.EventDrawer)
            // // // so we just ignore anything and let unity handle it
            // // SerializedProperty property = FieldWithInfo.SerializedProperty;
            // // MethodInfo imGuiGetPropertyHeightMethod = useDrawerType.GetMethod("GetPropertyHeight");
            // // MethodInfo imGuiOnGUIMethodInfo = useDrawerType.GetMethod("OnGUI");
            // // Debug.Assert(imGuiGetPropertyHeightMethod != null);
            // // Debug.Assert(imGuiOnGUIMethodInfo != null);
            // //
            // // IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);
            // //
            // // IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
            // // {
            // //     property.serializedObject.Update();
            // //
            // //     GUIContent label = imguiLabelHelper.NoLabel
            // //         ? GUIContent.none
            // //         : new GUIContent(imguiLabelHelper.RichLabel);
            // //
            // //     using (new ImGuiFoldoutStyleRichTextScoop())
            // //     using (new ImGuiLabelStyleRichTextScoop())
            // //     {
            // //         float height =
            // //             (float)imGuiGetPropertyHeightMethod.Invoke(propertyDrawer, new object[] { property, label });
            // //         Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            // //         imGuiOnGUIMethodInfo.Invoke(propertyDrawer, new object[] { rect, property, label });
            // //     }
            // // })
            // // {
            // //     style =
            // //     {
            // //         flexGrow = 1,
            // //         flexShrink = 0,
            // //     },
            // //     userData = imguiLabelHelper,
            // // };
            // // imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);
            // //
            // // return (imGuiContainer, false);
        }

        public IEnumerable<AbsRenderer> MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
#endif
