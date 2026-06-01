#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.SliderHandleDrawer
{
    public partial class SliderHandleAttributeDrawer
    {
        private static string NameSliderHandleHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}_SliderHandle_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameSliderHandleHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameSliderHandleHelpBox(property, index));
            SliderHandleInfo sliderHandleInfo =
                CreateSliderHandleInfo((SliderHandleAttribute)saintsAttribute, property, index, info, parent);
            helpBox.userData = sliderHandleInfo;
            SceneView.duringSceneGui += OnSceneGui;
            SceneView.RepaintAll();
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneView.duringSceneGui -= OnSceneGui;
                HandleVisibility.SetOutView(sliderHandleInfo.Id);
            });
            return;

            void OnSceneGui(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, sliderHandleInfo);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: NameSliderHandleHelpBox(property, index));
            SliderHandleInfo sliderHandleInfo = (SliderHandleInfo)helpBox.userData;
            UIToolkitUtils.SetHelpBox(helpBox, sliderHandleInfo.Error);
        }
    }
}
#endif
