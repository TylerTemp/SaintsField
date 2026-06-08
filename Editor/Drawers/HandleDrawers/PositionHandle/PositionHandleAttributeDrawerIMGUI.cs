using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.PositionHandle
{
    public partial class PositionHandleAttributeDrawer
    {
        private static readonly Dictionary<string, PositionHandleInfo> IDToInfoImGui = new Dictionary<string, PositionHandleInfo>();

        private static PositionHandleInfo EnsureKey(SerializedProperty property, PositionHandleAttribute positionHandleAttribute,
            MemberInfo info,
            object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!IDToInfoImGui.TryGetValue(key, out PositionHandleInfo positionHandleInfo))
            {
                IDToInfoImGui[key] = positionHandleInfo = new PositionHandleInfo
                {
                    SerializedProperty = property,
                    MemberInfo = info,
                    Parent = parent,
                    Space = positionHandleAttribute.Space,

                    Id = key,
                };

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!IDToInfoImGui.TryGetValue(key, out PositionHandleInfo innerPositionHandle))
                    {
                        return;
                    }
                    OnSceneGUIInternal(sceneView, innerPositionHandle);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IDToInfoImGui.Remove(key);
                    HandleVisibility.SetOutView(positionHandleInfo.Id);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            return positionHandleInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureKey(property, (PositionHandleAttribute) saintsAttribute, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (PositionHandleAttribute)saintsAttribute, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (PositionHandleAttribute)saintsAttribute, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
