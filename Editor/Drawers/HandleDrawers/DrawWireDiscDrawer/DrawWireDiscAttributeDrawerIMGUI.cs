using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawWireDiscDrawer
{
    public partial class DrawWireDiscAttributeDrawer
    {
        private readonly Dictionary<string, WireDiscInfo> _idToWireDiscInfo = new Dictionary<string, WireDiscInfo>();
        private static string GetKey(SerializedProperty property) => SerializedUtils.GetUniqueId(property);

        private WireDiscInfo EnsureWireDiscInfo(DrawWireDiscAttribute drawWireDiscAttribute,
            SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            string key = GetKey(serializedProperty);
            if (!_idToWireDiscInfo.TryGetValue(key, out WireDiscInfo wireDiscInfo))
            {
                _idToWireDiscInfo[key] = wireDiscInfo =
                    CreateWireDiscInfo(drawWireDiscAttribute, serializedProperty, memberInfo, parent);

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!_idToWireDiscInfo.TryGetValue(key, out WireDiscInfo innerWireDiscInfo))
                    {
                        return;
                    }
                    OnSceneGUIInternal(sceneView, innerWireDiscInfo);
                }

                NoLongerInspectingWatch(serializedProperty.serializedObject.targetObject, key, () =>
                {
                    _idToWireDiscInfo.Remove(key);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                    HandleVisibility.SetOutView(wireDiscInfo.Id);
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            wireDiscInfo.SerializedProperty = serializedProperty;
            wireDiscInfo.MemberInfo = memberInfo;
            wireDiscInfo.Parent = parent;

            return wireDiscInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureWireDiscInfo((DrawWireDiscAttribute)saintsAttribute, property, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureWireDiscInfo((DrawWireDiscAttribute)saintsAttribute, property, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureWireDiscInfo((DrawWireDiscAttribute)saintsAttribute, property, info, parent).Error;

            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }

    }
}
