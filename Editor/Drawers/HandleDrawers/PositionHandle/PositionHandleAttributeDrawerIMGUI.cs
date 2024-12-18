using System;
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

        // private string _cacheKey;

        // protected override void ImGuiOnDispose()
        // {
        //     base.ImGuiOnDispose();
        //     SceneView.duringSceneGui -= OnSceneGUIIMGUI;
        //     _idToInfoImGui.Remove(_cacheKey);
        // }

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
            string key = GetKey(property);
            // ReSharper disable once InvertIf
            if (!_idToInfoImGui.TryGetValue(key, out PositionHandleInfo positionHandleInfo))
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
                _idToInfoImGui[key] = positionHandleInfo;

                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();

                Selection.selectionChanged += OnSelectionChanged;

                return position;

                void OnSelectionChanged()
                {
                    bool remove = false;
                    UnityEngine.Object oriObject = null;
                    try
                    {
                        oriObject = property.serializedObject.targetObject;
                    }
                    catch (Exception)
                    {
                        remove = true;
                    }

                    if (!remove)
                    {
                        if (oriObject == null)
                        {
                            remove = true;
                        }
                        else
                        {
                            remove = Array.IndexOf(Selection.objects, oriObject) == -1;
                        }
                    }

                    if (remove)
                    {
                        Unsub();
                    }
                }
            }

            return position;

            // ReSharper disable once InconsistentNaming
            void OnSceneGUIIMGUI(SceneView sceneView)
            {
                if (_idToInfoImGui.TryGetValue(key, out PositionHandleInfo cachedInfo))
                {
                    positionHandleInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleInfo.Space, property, info, parent);
                    if (!OnSceneGUIInternal(sceneView, cachedInfo))
                    {
                        Unsub();
                    }
                }
            }

            void Unsub()
            {
                SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                _idToInfoImGui.Remove(key);
            }
        }

        // private void OnSceneGUIIMGUI(SceneView sceneView)
        // {
        //     if (_idToInfoImGui.TryGetValue(_cacheKey, out PositionHandleInfo positionHandleInfo))
        //     {
        //         if (!OnSceneGUIInternal(sceneView, positionHandleInfo))
        //         {
        //             Debug.LogWarning($"Target disposed, remove SceneGUI");
        //             SceneView.duringSceneGui -= OnSceneGUIIMGUI;
        //         }
        //     }
        // }

        #endregion
    }
}
