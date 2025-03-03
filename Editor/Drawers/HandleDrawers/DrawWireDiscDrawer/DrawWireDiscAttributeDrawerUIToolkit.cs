#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawWireDiscDrawer
{
    public partial class DrawWireDiscAttributeDrawer
    {
        private static string NameDrawWireDisc(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_DrawWireDisc";
        private static string NameDrawWireDiscHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_DrawWireDisc_HelpBox";

        private WireDiscInfo _wireDiscInfo;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameDrawWireDiscHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement child = new VisualElement
            {
                name = NameDrawWireDisc(property, index),
            };
            _wireDiscInfo = CreateWireDiscInfo((DrawWireDiscAttribute) saintsAttribute, property, info, parent);
            child.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                SceneView.RepaintAll();
            });
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: NameDrawWireDiscHelpBox(property, index));
            if (helpBox.text == _wireDiscInfo.Error)
            {
                return;
            }

            helpBox.text = _wireDiscInfo.Error;
            helpBox.style.display = string.IsNullOrEmpty(_wireDiscInfo.Error) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _wireDiscInfo);
        }
    }
}
#endif
