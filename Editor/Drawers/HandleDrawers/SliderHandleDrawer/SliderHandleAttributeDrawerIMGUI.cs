using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.SliderHandleDrawer
{
    public partial class SliderHandleAttributeDrawer
    {
        private static readonly Dictionary<string, SliderHandleInfo> IDToInfoImGui =
            new Dictionary<string, SliderHandleInfo>();

        private static SliderHandleInfo EnsureKey(SerializedProperty property,
            SliderHandleAttribute sliderHandleAttribute, int index, MemberInfo info, object parent)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (!IDToInfoImGui.TryGetValue(key, out SliderHandleInfo sliderHandleInfo))
            {
                IDToInfoImGui[key] = sliderHandleInfo =
                    CreateSliderHandleInfo(sliderHandleAttribute, property, index, info, parent);

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!IDToInfoImGui.TryGetValue(key, out SliderHandleInfo innerSliderHandleInfo))
                    {
                        return;
                    }

                    OnSceneGUIInternal(sceneView, innerSliderHandleInfo);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IDToInfoImGui.Remove(key);
                    HandleVisibility.SetOutView(sliderHandleInfo.Id);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            return sliderHandleInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return EnsureKey(property, (SliderHandleAttribute)saintsAttribute, index, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (SliderHandleAttribute)saintsAttribute, index, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (SliderHandleAttribute)saintsAttribute, index, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
