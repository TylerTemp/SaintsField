using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    public class GetComponentInSceneAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute);
            if (error != "")
            {
                _error = error;
                return false;
            }
            if(result != null)
            {
                SetValueChanged(property);
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);
        #endregion

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            if (property.objectReferenceValue != null)
            {
                return ("", null);
            }

            GetComponentInSceneAttribute getComponentInSceneAttribute = (GetComponentInSceneAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);

            if (getComponentInSceneAttribute.CompType == typeof(GameObject))
            {
                return ("You can not use GetComponentInChildrenAttribute with GameObject type", null);
            }

            Type type = getComponentInSceneAttribute.CompType ?? fieldType;

            Component componentInScene = null;

            Scene scene = SceneManager.GetActiveScene();
            bool includeInactive = getComponentInSceneAttribute.IncludeInactive;
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (!includeInactive && !rootGameObject.activeSelf)
                {
                    continue;
                }

                Component findSelfComponent = rootGameObject.GetComponent(type);
                if (findSelfComponent != null)
                {
                    componentInScene = findSelfComponent;
                    break;
                }

                Component findComponent = rootGameObject.GetComponentInChildren(type, includeInactive);
                // ReSharper disable once InvertIf
                if (findComponent != null)
                {
                    componentInScene = findComponent;
                    break;
                }
            }

            if (componentInScene == null)
            {
                return ($"No {type} found in scene", null);
            }

            UnityEngine.Object result = componentInScene;

            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    result = componentInScene.gameObject;
                }
                else
                {
                    result = componentInScene.GetComponent(fieldType);
                }
            }

            property.objectReferenceValue = result;
            return ("", result);
        }

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetComponentInScene";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_SCENE
            Debug.Log($"GetComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute);
            HelpBox helpBox = container.Q<HelpBox>(NamePlaceholder(property, index));
            if (error != helpBox.text)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

            // ReSharper disable once InvertIf
            if (result)
            {
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(result);
            }
        }

        // NOTE: ensure the post field is added to the container!
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NamePlaceholder(property, index),
            };
        }
        #endregion
    }
}
