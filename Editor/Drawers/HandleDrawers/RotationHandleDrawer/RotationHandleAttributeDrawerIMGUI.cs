using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.RotationHandleDrawer
{
    public partial class RotationHandleAttributeDrawer
    {
        private static readonly Dictionary<string, RotationHandleInfo> IDToInfoImGui = new Dictionary<string, RotationHandleInfo>();

        private static RotationHandleInfo EnsureKey(SerializedProperty property, RotationHandleAttribute rotationHandleAttribute,
            int index, MemberInfo info, object parent)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (!IDToInfoImGui.TryGetValue(key, out RotationHandleInfo rotationHandleInfo))
            {
                IDToInfoImGui[key] = rotationHandleInfo = CreateRotationHandleInfo(
                    rotationHandleAttribute,
                    property,
                    index,
                    _ => {},
                    info,
                    parent
                    );

                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!IDToInfoImGui.TryGetValue(key, out RotationHandleInfo innerRotationHandleInfo))
                    {
                        return;
                    }

                    OnSceneGUIInternal(sceneView, innerRotationHandleInfo);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IDToInfoImGui.Remove(key);
                    HandleVisibility.SetOutView(rotationHandleInfo.Id);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            return rotationHandleInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return EnsureKey(property, (RotationHandleAttribute) saintsAttribute, index, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (RotationHandleAttribute) saintsAttribute, index, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (RotationHandleAttribute) saintsAttribute, index, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
