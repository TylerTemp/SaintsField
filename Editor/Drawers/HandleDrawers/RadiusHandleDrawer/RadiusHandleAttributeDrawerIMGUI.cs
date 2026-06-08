using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.RadiusHandleDrawer
{
    public partial class RadiusHandleAttributeDrawer
    {
        private static readonly Dictionary<string, RadiusHandleInfo> IDToInfoImGui = new Dictionary<string, RadiusHandleInfo>();

        private static RadiusHandleInfo EnsureKey(SerializedProperty property, RadiusHandleAttribute radiusHandleAttribute,
            int index, MemberInfo info, object parent)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (!IDToInfoImGui.TryGetValue(key, out RadiusHandleInfo radiusHandleInfo))
            {
                IDToInfoImGui[key] = radiusHandleInfo = CreateRadiusHandleInfo(radiusHandleAttribute, property, index, info, parent);

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!IDToInfoImGui.TryGetValue(key, out RadiusHandleInfo innerRadiusHandleInfo))
                    {
                        return;
                    }
                    OnSceneGUIInternal(sceneView, innerRadiusHandleInfo);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IDToInfoImGui.Remove(key);
                    HandleVisibility.SetOutView(radiusHandleInfo.Id);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            return radiusHandleInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return EnsureKey(property, (RadiusHandleAttribute)saintsAttribute, index, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (RadiusHandleAttribute)saintsAttribute, index, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (RadiusHandleAttribute)saintsAttribute, index, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
