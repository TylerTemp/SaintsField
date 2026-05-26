#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.RotationHandleDrawer
{
    public partial class RotationHandleAttributeDrawer
    {
        private static string NameRotationHandleHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_RotationHandle_HelpBox";

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
                name = NameRotationHandleHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameRotationHandleHelpBox(property, index));
            RotationHandleAttribute rotationHandleAttribute = (RotationHandleAttribute)saintsAttribute;
            RotationHandleInfo rotationHandleInfo = CreateRotationHandleInfo(rotationHandleAttribute, property, index, onValueChangedCallback, info, parent);
            helpBox.userData = rotationHandleInfo;
            SceneView.duringSceneGui += OnSceneGui;
            SceneView.RepaintAll();
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneView.duringSceneGui -= OnSceneGui;
                HandleVisibility.SetOutView(rotationHandleInfo.Id);
            });
            return;

            void OnSceneGui(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, rotationHandleInfo);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: NameRotationHandleHelpBox(property, index));
            RotationHandleInfo rotationHandleInfo = (RotationHandleInfo)helpBox.userData;
            UIToolkitUtils.SetHelpBox(helpBox, rotationHandleInfo.Error);
        }
    }
}

#endif
