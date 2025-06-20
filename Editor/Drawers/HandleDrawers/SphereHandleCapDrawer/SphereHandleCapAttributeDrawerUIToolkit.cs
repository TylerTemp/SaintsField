#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.SphereHandleCapDrawer
{
    public partial class SphereHandleCapAttributeDrawer
    {
        private static string NameDrawWireDisc(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_DrawWireDisc";
        private static string NameDrawWireDiscHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_DrawWireDisc_HelpBox";

        // private SphereInfo _sphereInfo;

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

            SphereInfo sphereInfo = CreateSphereInfo((SphereHandleCapAttribute) saintsAttribute, property, info, parent);
            HelpBox helpBox = container.Q<HelpBox>(name: NameDrawWireDiscHelpBox(property, index));

            UpdateErrorBox(helpBox, sphereInfo);

            child.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                SceneView.RepaintAll();
            });
            child.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
                HandleVisibility.SetOutView(sphereInfo.Id);
            });
            container.Add(child);

            return;

            // ReSharper disable once InconsistentNaming
            void OnSceneGUIUIToolkit(SceneView sceneView)
            {
                UpdateErrorBox(helpBox, sphereInfo);
                OnSceneGUIInternal(sceneView, sphereInfo);
            }
        }

        private static void UpdateErrorBox(HelpBox helpBox, SphereInfo sphereInfo)
        {
            if (helpBox.text == sphereInfo.Error)
            {
                return;
            }

            helpBox.text = sphereInfo.Error;
            helpBox.style.display = string.IsNullOrEmpty(sphereInfo.Error) ? DisplayStyle.None : DisplayStyle.Flex;
        }


    }
}
#endif
