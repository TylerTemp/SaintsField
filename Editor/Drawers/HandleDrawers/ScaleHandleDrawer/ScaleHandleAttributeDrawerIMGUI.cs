using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.ScaleHandleDrawer
{
    public partial class ScaleHandleAttributeDrawer
    {
        private static readonly Dictionary<string, ScaleHandleInfo> IDToInfoImGui = new Dictionary<string, ScaleHandleInfo>();

        private static ScaleHandleInfo EnsureKey(SerializedProperty property, ScaleHandleAttribute scaleHandleAttribute,
            int index, MemberInfo info, object parent)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (!IDToInfoImGui.TryGetValue(key, out ScaleHandleInfo scaleHandleInfo))
            {
                IDToInfoImGui[key] = scaleHandleInfo = CreateScaleHandleInfo(
                    scaleHandleAttribute,
                    property,
                    index,
                    _ => { },
                    info,
                    parent);

                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!IDToInfoImGui.TryGetValue(key, out ScaleHandleInfo innerScaleHandleInfo))
                    {
                        return;
                    }

                    OnSceneGUIInternal(sceneView, innerScaleHandleInfo);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IDToInfoImGui.Remove(key);
                    HandleVisibility.SetOutView(scaleHandleInfo.Id);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            return scaleHandleInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return EnsureKey(property, (ScaleHandleAttribute) saintsAttribute, index, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (ScaleHandleAttribute) saintsAttribute, index, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (ScaleHandleAttribute) saintsAttribute, index, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
