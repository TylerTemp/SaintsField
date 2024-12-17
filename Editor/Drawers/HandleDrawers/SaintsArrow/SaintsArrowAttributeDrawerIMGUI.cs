#if SAINTSFIELD_SAINTSDRAW || SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE

using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.SaintsArrow
{
    public partial class SaintsArrowAttributeDrawer
    {
        #region IMGUI

        private readonly Dictionary<string, ArrowInfo> _idToInfoImGui = new Dictionary<string, ArrowInfo>();
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
            if (!_idToInfoImGui.ContainsKey(_cacheKey))
            {
                SaintsArrowAttribute saintsArrowAttribute = (SaintsArrowAttribute)saintsAttribute;

                ArrowConstInfo arrowConstInfo = new ArrowConstInfo
                {
                    SaintsArrowAttribute = saintsArrowAttribute,
                    Property = property,
                    Info = info,
                    Parent = parent,
                };
                _idToInfoImGui[_cacheKey] = GetArrowInfo(arrowConstInfo);
                ImGuiEnsureDispose(property.serializedObject.targetObject);
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
                return position;
            }
            return position;
        }

        private void OnSceneGUIIMGUI(SceneView sceneView)
        {
            if (_idToInfoImGui.TryGetValue(_cacheKey, out ArrowInfo arrowInfo))
            {
                if (arrowInfo == null || arrowInfo.Error != "")
                {
                    return;
                }

                // update here!
                if (!arrowInfo.StartTargetWorldPosInfo.IsTransform || !arrowInfo.EndTargetWorldPosInfo.IsTransform)
                {
                    _idToInfoImGui[_cacheKey] = arrowInfo =  GetArrowInfo(arrowInfo.ArrowConstInfo);
                }

                if (!OnSceneGUIInternal(sceneView, arrowInfo))
                {
                    Debug.LogWarning($"Target disposed, remove SceneGUI");
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                }
            }
        }

        #endregion
    }
}
#endif
