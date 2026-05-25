#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.RadiusHandleDrawer
{
    public partial class RadiusHandleAttributeDrawer
    {
        // private static string NameRadiusHandleInfo(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_DrawWireDisc";
        private static string NameRadiusHandleInfoHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}_DrawWireDisc_HelpBox";

        // private RadiusHandleInfo _radiusHandleInfo;

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
                name = NameRadiusHandleInfoHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameRadiusHandleInfoHelpBox(property, index));
            RadiusHandleInfo radiusHandleInfo = CreateRadiusHandleInfo((RadiusHandleAttribute) saintsAttribute, property, index, info, parent);
            helpBox.userData = radiusHandleInfo;
            helpBox.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGui;
                SceneView.RepaintAll();
            });
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneView.duringSceneGui -= OnSceneGui;
                HandleVisibility.SetOutView(radiusHandleInfo.Id);
            });
            return;

            void OnSceneGui(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, radiusHandleInfo);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(name: NameRadiusHandleInfoHelpBox(property, index));
            RadiusHandleInfo radiusHandleInfo = (RadiusHandleInfo) helpBox.userData;
            UIToolkitUtils.SetHelpBox(helpBox, radiusHandleInfo.Error);
        }
    }
}
#endif
