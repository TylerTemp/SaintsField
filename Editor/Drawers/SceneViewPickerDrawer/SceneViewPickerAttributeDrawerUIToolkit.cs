using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
    public partial class SceneViewPickerAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__SceneViewPicker";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new Button(EditorGUIUtility.IconContent("d_scenepicking_pickable").image as Texture2D)
            {
                name = NameButton(property),
                tooltip = "Pick an object in the scene view",
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // _pickingInfo = new PickingInfo
            // {
            //     SerializedProperty = property,
            //     MemberInfo = info,
            //     Parent = parent,
            //     Error = "",
            // };

            Button button = container.Q<Button>(NameButton(property));

            Scene currentScene = GetScene(property.serializedObject.targetObject);
            if (!currentScene.IsValid())
            {
                button.RemoveFromHierarchy();
                return;
            }

            button.clicked += () =>
            {
                _pickingInfo = InitPickingInfo(property, info, parent);
                SceneView.duringSceneGui += OnSceneGUIUIToolkit;
            };

            button.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
        }

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _pickingInfo);
        }
    }
}
