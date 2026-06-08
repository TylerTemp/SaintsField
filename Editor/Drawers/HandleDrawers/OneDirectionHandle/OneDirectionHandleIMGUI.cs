using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle
{
    public abstract partial class OneDirectionHandleBase
    {

        private static readonly Dictionary<string, OneDirectionInfo> IDToInfoImGui = new Dictionary<string, OneDirectionInfo>();
        // private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        // private string _cacheKey;

        private OneDirectionInfo EnsureKey(OneDirectionBaseAttribute oneDirectionBaseAttribute,
            SerializedProperty property, MemberInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!IDToInfoImGui.TryGetValue(key, out OneDirectionInfo oneDirectionInfo))
            {
                IDToInfoImGui[key] = oneDirectionInfo = new OneDirectionInfo
                {
                    SerializedProperty = property,
                    MemberInfo = info,
                    Parent = parent,

                    OneDirectionAttribute = oneDirectionBaseAttribute,
                    Color = oneDirectionBaseAttribute.Color,

                    Error = "",
                };

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    // ReSharper disable once InvertIf
                    if (IDToInfoImGui.TryGetValue(key, out OneDirectionInfo arrowInfo))
                    {
                        // ReSharper disable once InvertIf
                        if (!OnSceneGUIInternal(sceneView, arrowInfo))
                        {
                            Debug.LogWarning($"Target disposed, remove SceneGUI");
                            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                        }
                    }
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    IDToInfoImGui.Remove(key);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });

                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            return oneDirectionInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureKey((OneDirectionBaseAttribute) saintsAttribute, property, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey((OneDirectionBaseAttribute)saintsAttribute, property, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey((OneDirectionBaseAttribute)saintsAttribute, property, info, parent).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
