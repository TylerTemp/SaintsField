#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
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

        private static StyleSheet _uss;

        private static StyleSheet GetStyle()
        {
            if (!_uss)
            {
                _uss = Util.LoadResource<StyleSheet>("UIToolkit/ToggleButton.uss");
            }

            return _uss;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new Button
            {
                name = NameButton(property),
                tooltip = "Pick an object in the scene view",
                style =
                {
                    backgroundImage = EditorGUIUtility.IconContent("d_scenepicking_pickable").image as Texture2D,
                    width = SingleLineHeight,
                    height = SingleLineHeight,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(14, 14),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
        }

        private bool _curPicking;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));
            button.styleSheets.Add(GetStyle());

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
                    button.style.backgroundImage =
                        EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed_hover").image as Texture2D;
                    SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                    _curPicking = true;

                    button.AddToClassList("saintsfield-toggle-button-active");
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
                button.style.backgroundImage = EditorGUIUtility.IconContent("d_scenepicking_pickable").image as Texture2D;
                button.RemoveFromClassList("saintsfield-toggle-button-active");
            }
        }

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _pickingInfo);
        }
    }
}
#endif
