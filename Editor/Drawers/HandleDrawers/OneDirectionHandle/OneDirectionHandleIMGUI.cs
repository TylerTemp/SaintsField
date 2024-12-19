using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle
{
    public abstract partial class OneDirectionHandleBase
    {
        #region IMGUI

        private readonly Dictionary<string, OneDirectionInfo> _idToInfoImGui = new Dictionary<string, OneDirectionInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        // private string _cacheKey;

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
            if (!_idToInfoImGui.ContainsKey(key))
            {
                OneDirectionBaseAttribute saintsArrowAttribute = (OneDirectionBaseAttribute)saintsAttribute;

                OneDirectionConstInfo oneDirectionConstInfo = new OneDirectionConstInfo
                {
                    OneDirectionAttribute = saintsArrowAttribute,
                    Property = property,
                    Info = info,
                    Parent = parent,
                };
                _idToInfoImGui[key] = GetArrowInfo(oneDirectionConstInfo);
                ImGuiEnsureDispose(property.serializedObject.targetObject);
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
                if (_idToInfoImGui.TryGetValue(key, out OneDirectionInfo arrowInfo))
                {
                    if (arrowInfo == null || arrowInfo.Error != "")
                    {
                        return;
                    }

                    // update here!
                    if (!arrowInfo.StartTargetWorldPosInfo.IsTransform || !arrowInfo.EndTargetWorldPosInfo.IsTransform)
                    {
                        _idToInfoImGui[key] = arrowInfo =  GetArrowInfo(arrowInfo.OneDirectionConstInfo);
                    }

                    if (!OnSceneGUIInternal(sceneView, arrowInfo))
                    {
                        Debug.LogWarning($"Target disposed, remove SceneGUI");
                        SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                    }
                }
            }

            void Unsub()
            {
                SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                _idToInfoImGui.Remove(key);
            }
        }


        #endregion
    }
}
