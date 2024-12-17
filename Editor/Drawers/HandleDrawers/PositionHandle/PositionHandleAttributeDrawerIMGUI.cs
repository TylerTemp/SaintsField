using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.PositionHandle
{
    public partial class PositionHandleAttributeDrawer
    {
        #region IMGUI

        private readonly Dictionary<string, PositionHandleInfo> _idToInfoImGui = new Dictionary<string, PositionHandleInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private string _cacheKey;

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
            _idToInfoImGui.Remove(_cacheKey);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            _cacheKey = GetKey(property);
            // ReSharper disable once InvertIf
            if (!_idToInfoImGui.TryGetValue(_cacheKey, out PositionHandleInfo positionHandleInfo))
            {
                PositionHandleAttribute positionHandleAttribute = (PositionHandleAttribute)saintsAttribute;

                Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleAttribute.Space, property, info, parent);
                if (targetWorldPosInfo.Error != "")
                {
                    Debug.LogError(targetWorldPosInfo.Error);
                    return position;
                }

                positionHandleInfo = new PositionHandleInfo
                {
                    Property = property,
                    Info = info,
                    Parent = parent,
                    Space = positionHandleAttribute.Space,
                    TargetWorldPosInfo = targetWorldPosInfo,
                };
                _idToInfoImGui[_cacheKey] = positionHandleInfo;
                ImGuiEnsureDispose(property.serializedObject.targetObject);
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            positionHandleInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleInfo.Space, property, info, parent);
            return position;
        }

        private void OnSceneGUIIMGUI(SceneView sceneView)
        {
            if (_idToInfoImGui.TryGetValue(_cacheKey, out PositionHandleInfo positionHandleInfo))
            {
                if (!OnSceneGUIInternal(sceneView, positionHandleInfo))
                {
                    Debug.LogWarning($"Target disposed, remove SceneGUI");
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                }
            }
        }

        #endregion
    }
}
