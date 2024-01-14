using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GameObjectActiveAttribute))]
    public class GameObjectActiveAttributeDrawer: DecToggleAttributeDrawer
    {
        #region IMGUI
        private const string SeeXml = "<color=white><icon='eye.png' /></color>";
        private const string UnSeeXml = "<color=orange><icon='eye-slash.png' /></color>";

        private float _width = -1;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            if (_width >= 0)
            {
                return _width;
            }
            // object target = property.serializedObject.targetObject;
            // string labelXml = GetButtonLabelXml((DecButtonAttribute)saintsAttribute, target, target.GetType());
            float xmlWidth = RichTextDrawer.GetWidth(label, position.height, RichTextDrawer.ParseRichXml(UnSeeXml, ""));
            if (xmlWidth > 0)
            {
                return _width = xmlWidth;
            }

            return position.height;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            _error = "";

            GameObject go;
            if (property.objectReferenceValue is GameObject isGO)
            {
                go = isGO;
            }
            else
            {
                go = ((Component) property.objectReferenceValue)?.gameObject;
            }

            bool goIsNull = go == null;
            bool goActive = !goIsNull && go.activeSelf;

            Draw(position, property, label, goActive? SeeXml: UnSeeXml, goActive, (newIsActive) =>
            {
                if (go != null)
                {
                    Undo.RecordObject(go, $"GameObjectActive: {property.propertyPath}");
                    go.SetActive(newIsActive);
                }
            });

            if (goIsNull)
            {
                _error = $"Unable to get GameObject from {property.name}";
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

        #region UIToolkit

        private static string NameButton(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GameObjectActive_Button";

        private static string NameButtonLabelActive(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GameObjectActive_ButtonLabelActive";
        private static string NameButtonLabelInactive(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GameObjectActive_ButtonLabelInactive";

        private struct Payload
        {
            public bool isActive;
            public bool isNull;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    width = EditorGUIUtility.singleLineHeight + 2,
                    paddingLeft = 1,
                    paddingRight = 1,
                    marginLeft = 1,
                    marginRight = 1,
                },
                name = NameButton(property, index),
                userData = new Payload
                {
                    isActive = true,
                    isNull = false,
                },
            };

            Texture2D active = RichTextDrawer.LoadTexture("eye.png");
            Texture2D inactive = RichTextDrawer.LoadTexture("eye-slash.png");

            button.Add(new Image
            {
                image = active,
                name = NameButtonLabelActive(property, index),

                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                },
            });
            button.Add(new Image
            {
                image = inactive,
                name = NameButtonLabelInactive(property, index),
                tintColor = EColor.Orange.GetColor(),

                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    display = DisplayStyle.None,
                },
            });


            return button;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            Button button = container.Q<Button>(NameButton(property, index));
            button.clicked += () =>
            {
                GameObject go;
                if (property.objectReferenceValue is GameObject isGo)
                {
                    go = isGo;
                }
                else
                {
                    go = ((Component) property.objectReferenceValue)?.gameObject;
                }

                if (go == null)
                {
                    return;
                }

                Undo.RecordObject(go, $"GameObjectActive: {property.propertyPath}");
                go.SetActive(!go.activeSelf);
                OnUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, parent);
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            GameObject go;
            if (property.objectReferenceValue is GameObject isGo)
            {
                go = isGo;
            }
            else
            {
                go = ((Component) property.objectReferenceValue)?.gameObject;
            }

            bool goIsNull = go == null;
            bool goActive = !goIsNull && go.activeSelf;

            Button button = container.Q<Button>(NameButton(property, index));
            Payload payload = (Payload) button.userData;

            bool hasChange = false;

            if (payload.isNull != goIsNull)
            {
                hasChange = true;
                button.SetEnabled(!goIsNull);
            }

            if (payload.isActive != goActive)
            {
                hasChange = true;
                container.Q<Image>(NameButtonLabelActive(property, index)).style.display = goActive ? DisplayStyle.Flex : DisplayStyle.None;
                container.Q<Image>(NameButtonLabelInactive(property, index)).style.display = goActive ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (hasChange)
            {
                button.userData = new Payload
                {
                    isActive = goActive,
                    isNull = goIsNull,
                };
            }
        }

        #endregion
    }
}
