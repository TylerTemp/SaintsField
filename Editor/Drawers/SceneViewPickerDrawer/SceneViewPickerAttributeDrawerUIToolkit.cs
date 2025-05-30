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
                style =
                {
                    width = SingleLineHeight,
                    height = SingleLineHeight,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                },
            };
        }

        private bool _curPicking;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));

            Scene currentScene = GetScene(property.serializedObject.targetObject);
            if (!currentScene.IsValid())
            {
                button.RemoveFromHierarchy();
                return;
            }

            button.clicked += () =>
            {
                StopAllPicking.Invoke();

                if(!_curPicking)
                {
                    _pickingInfo = InitPickingInfo(property, info, StopPick);
                    button.iconImage =
                        EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed_hover").image as Texture2D;
                    SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                    _curPicking = true;
                }
                else
                {
                    SceneView.RepaintAll();
                    _curPicking = false;
                }
            };

            StopAllPicking.AddListener(StopPick);

            button.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                StopAllPicking.RemoveListener(StopPick);
            });
            return;

            void StopPick()
            {
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
                _curPicking = false;
                _showSelectingPanel = false;
                _selectingPanelSearching = "";
                button.iconImage = EditorGUIUtility.IconContent("d_scenepicking_pickable").image as Texture2D;
            }
        }

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _pickingInfo);
        }
    }
}
