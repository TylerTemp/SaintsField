using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.PrimitiveBoundsHandleDrawer
{
    public partial class PrimitiveBoundsHandleAttributeDrawer
    {
        private static readonly Dictionary<string, PrimitiveBoundsHandleInfo> IdToInfoImGui = new Dictionary<string, PrimitiveBoundsHandleInfo>();

        private static PrimitiveBoundsHandleInfo EnsureKey(SerializedProperty property, PrimitiveBoundsHandleAttribute primitiveBoundsHandleAttribute,
            int index, MemberInfo info, object parent)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (!IdToInfoImGui.TryGetValue(key, out PrimitiveBoundsHandleInfo handleInfo))
            {
                IdToInfoImGui[key] = handleInfo = CreatePrimitiveBoundsHandleInfo(primitiveBoundsHandleAttribute, property, index, info, parent);

                void OnSceneGuiImGui(SceneView sceneView)
                {
                    if (!IdToInfoImGui.TryGetValue(key, out PrimitiveBoundsHandleInfo innerHandleInfo))
                    {
                        return;
                    }

                    OnSceneGUIInternal(sceneView, innerHandleInfo);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IdToInfoImGui.Remove(key);
                    HandleVisibility.SetOutView(handleInfo.Id);
                    SceneView.duringSceneGui -= OnSceneGuiImGui;
                });
                SceneView.duringSceneGui += OnSceneGuiImGui;
                SceneView.RepaintAll();
            }

            return handleInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return EnsureKey(property, (PrimitiveBoundsHandleAttribute)saintsAttribute, index, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (PrimitiveBoundsHandleAttribute)saintsAttribute, index, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (PrimitiveBoundsHandleAttribute)saintsAttribute, index, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
