#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.PrimitiveBoundsHandleDrawer
{
    public partial class PrimitiveBoundsHandleAttributeDrawer
    {
        private static string NamePrimitiveBoundsHandleHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_PrimitiveBoundsHandle_HelpBox";

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
                name = NamePrimitiveBoundsHandleHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NamePrimitiveBoundsHandleHelpBox(property, index));
            PrimitiveBoundsHandleInfo handleInfo = CreatePrimitiveBoundsHandleInfo((PrimitiveBoundsHandleAttribute)saintsAttribute, property, index, info, parent);
            helpBox.userData = handleInfo;
            helpBox.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGui;
                SceneView.RepaintAll();
            });
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneView.duringSceneGui -= OnSceneGui;
                HandleVisibility.SetOutView(handleInfo.Id);
            });
            return;

            void OnSceneGui(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, handleInfo);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: NamePrimitiveBoundsHandleHelpBox(property, index));
            PrimitiveBoundsHandleInfo handleInfo = (PrimitiveBoundsHandleInfo)helpBox.userData;
            UIToolkitUtils.SetHelpBox(helpBox, handleInfo.Error);
        }
    }
}
#endif
