using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.SphereHandleCapDrawer
{
    public partial class SphereHandleCapAttributeDrawer
    {
        private readonly Dictionary<string, SphereInfo> _idToSphereInfo = new Dictionary<string, SphereInfo>();
        private static string GetKey(SerializedProperty property) => SerializedUtils.GetUniqueId(property);

        private SphereInfo EnsureWireDiscInfo(SphereHandleCapAttribute sphereHandleCapAttribute,
            SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            string key = GetKey(serializedProperty);
            if (!_idToSphereInfo.TryGetValue(key, out SphereInfo sphereInfo))
            {
                _idToSphereInfo[key] = sphereInfo =
                    CreateSphereInfo(sphereHandleCapAttribute, serializedProperty, memberInfo, parent);

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!_idToSphereInfo.TryGetValue(key, out SphereInfo innerSphereInfo))
                    {
                        return;
                    }
                    OnSceneGUIInternal(sceneView, innerSphereInfo);
                }

                NoLongerInspectingWatch(serializedProperty.serializedObject.targetObject, key, () =>
                {
                    _idToSphereInfo.Remove(key);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                    HandleVisibility.SetOutView(sphereInfo.Id);
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            sphereInfo.SerializedProperty = serializedProperty;
            sphereInfo.MemberInfo = memberInfo;
            sphereInfo.Parent = parent;

            return sphereInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureWireDiscInfo((SphereHandleCapAttribute)saintsAttribute, property, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureWireDiscInfo((SphereHandleCapAttribute)saintsAttribute, property, info, parent).Error;
            return error == ""
                ? 0
                : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureWireDiscInfo((SphereHandleCapAttribute)saintsAttribute, property, info, parent).Error;

            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }

    }
}
