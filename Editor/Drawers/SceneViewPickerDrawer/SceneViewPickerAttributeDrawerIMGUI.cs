using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
    public partial class SceneViewPickerAttributeDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            Scene currentScene = GetScene(property.serializedObject.targetObject);
            if (!currentScene.IsValid())
            {
                return 0;
            }
            return SingleLineHeight;
        }

        private class ImGuiConfig
        {
            public PickingInfo PickingInfo;
            public bool IsPicking;
        }

        private static readonly Dictionary<string, ImGuiConfig> ImGuiCache = new Dictionary<string, ImGuiConfig>();


        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            Scene currentScene = GetScene(property.serializedObject.targetObject);
            if (!currentScene.IsValid())
            {
                return false;
            }

            string key = SerializedUtils.GetUniqueId(property);

            if (!ImGuiCache.TryGetValue(key, out ImGuiConfig config))
            {
                config = new ImGuiConfig
                {
                    PickingInfo = new PickingInfo(),
                    IsPicking = false,
                };
                ImGuiCache[key] = config;

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    ImGuiCache.Remove(key);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                    StopAllPicking.RemoveListener(StopPick);
                });

                StopAllPicking.AddListener(StopPick);
            }

            Texture2D icon = config.IsPicking
                ? EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed_hover").image as Texture2D
                : EditorGUIUtility.IconContent("d_scenepicking_pickable").image as Texture2D;

            // ReSharper disable once InvertIf
            if (GUI.Button(position, new GUIContent(icon), new GUIStyle("iconButton")))
            {
                StopAllPicking.Invoke();

                if (!config.IsPicking)
                {
                    config.IsPicking = true;
                    config.PickingInfo = InitPickingInfo(property, info, StopPick);
                    SceneView.duringSceneGui += OnSceneGUIIMGUI;
                }
                else
                {
                    config.IsPicking = false;
                    StopPick();
                }
            }

            return true;

            void StopPick()
            {
                SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                config.IsPicking = false;
                _showSelectingPanel = false;
                _selectingPanelSearching = "";
            }

            // ReSharper disable once InconsistentNaming
            void OnSceneGUIIMGUI(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, config.PickingInfo);
            }
        }
    }
}
