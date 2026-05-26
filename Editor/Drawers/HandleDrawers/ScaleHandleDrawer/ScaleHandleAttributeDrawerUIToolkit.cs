#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.ScaleHandleDrawer
{
    public partial class ScaleHandleAttributeDrawer
    {
        private static string NameScaleHandleHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_ScaleHandle_HelpBox";

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
                name = NameScaleHandleHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameScaleHandleHelpBox(property, index));
            ScaleHandleAttribute scaleHandleAttribute = (ScaleHandleAttribute)saintsAttribute;
            ScaleHandleInfo scaleHandleInfo = CreateScaleHandleInfo(
                scaleHandleAttribute,
                property,
                index,
                onValueChangedCallback,
                info,
                parent);
            helpBox.userData = scaleHandleInfo;
            SceneView.duringSceneGui += OnSceneGui;
            SceneView.RepaintAll();
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneView.duringSceneGui -= OnSceneGui;
                HandleVisibility.SetOutView(scaleHandleInfo.Id);
            });
            return;

            void OnSceneGui(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, scaleHandleInfo);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: NameScaleHandleHelpBox(property, index));
            ScaleHandleInfo scaleHandleInfo = (ScaleHandleInfo)helpBox.userData;
            UIToolkitUtils.SetHelpBox(helpBox, scaleHandleInfo.Error);
        }
    }
}

#endif
