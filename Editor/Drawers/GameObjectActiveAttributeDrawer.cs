using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

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
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (_width >= 0)
            {
                return _width;
            }
            // object target = property.serializedObject.targetObject;
            // string labelXml = GetButtonLabelXml((DecButtonAttribute)saintsAttribute, target, target.GetType());
            float xmlWidth = RichTextDrawer.GetWidth(label, position.height, RichTextDrawer.ParseRichXml(UnSeeXml, "", info, parent));
            if (xmlWidth > 0)
            {
                return _width = xmlWidth;
            }

            return position.height;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // error = "";

            (string myError, GameObject go) = GetGo(property, info, parent);
            error = myError;
            // if (property.objectReferenceValue is GameObject isGO)
            // {
            //     go = isGO;
            // }
            // else
            // {
            //     go = ((Component) property.objectReferenceValue)?.gameObject;
            // }

            bool goIsNull = go == null;
            bool goActive = !goIsNull && go.activeSelf;

            Draw(position, property, label, goActive? SeeXml: UnSeeXml, goActive, (newIsActive) =>
            {
                if (go != null)
                {
                    Undo.RecordObject(go, $"GameObjectActive: {property.propertyPath}");
                    go.SetActive(newIsActive);
                }
            }, info, parent);

            if (goIsNull)
            {
                error = $"Unable to get GameObject from {property.propertyPath}";
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);

        #endregion

        private static (string error, GameObject go) GetGo(SerializedProperty property, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

                if (error == "" && propertyValue is IWrapProp wrapProp)
                {
                    object propWrapValue = Util.GetWrapValue(wrapProp);
                    switch (propWrapValue)
                    {
                        case null:
                            return ("", null);
                        case GameObject wrapGo:
                            return ("", wrapGo);
                        case Component wrapComp:
                            return ("", wrapComp.gameObject);
                        default:
                            return ($"{propWrapValue} is not GameObject or Component", null);
                    }
                }

                return ($"{property.propertyType} is not supported", null);
            }
            if (property.objectReferenceValue is GameObject isGo)
            {
                return ("", isGo);
            }
            if(property.objectReferenceValue is Component comp)
            {
                return ("", comp.gameObject);
                // go = ((Component) property.objectReferenceValue)?.gameObject;
            }

            return ($"{property.propertyType} is not GameObject or Component", null);
        }

#if UNITY_2021_3_OR_NEWER

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
            VisualElement container, FieldInfo info, object parent)
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

            Texture2D active = Util.LoadResource<Texture2D>("eye.png");
            Texture2D inactive = Util.LoadResource<Texture2D>("eye-slash.png");

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

            // button itself can toggle enable/disable. We need a wrapper for ReadOnly
            VisualElement root = new VisualElement();
            root.AddToClassList(ClassAllowDisable);

            root.Add(button);

            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButton(property, index));
            button.clicked += () =>
            {
                (string goError, GameObject go) = GetGo(property, info, parent);
                if(goError != "")
                {
                    Debug.LogError(goError);
                    return;
                }

                if (go == null)
                {
                    return;
                }

                Undo.RecordObject(go, $"GameObjectActive: {property.propertyPath}");
                go.SetActive(!go.activeSelf);
                OnUpdateUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info);
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            (string goError, GameObject go) = GetGo(property, info, parent);
            if (goError != "")
            {
                // Debug.LogError(goError);
                return;
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

#endif
    }
}
