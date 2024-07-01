using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    public class GetComponentInSceneAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
            if (error != "")
            {
                _error = error;
                return false;
            }
            if(result != null)
            {
                onGUIPayload.SetValue(result);
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);
        #endregion

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            SerializedProperty targetProperty = property;
            Type fieldType = info.FieldType;
            Type interfaceType = null;
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                object propertyValue = SerializedUtils.GetValue(property, info, parent);

                if (propertyValue is IWrapProp wrapProp)
                {
                    Type mostBaseType = SaintsInterfaceDrawer.GetMostBaseType(wrapProp.GetType());
                    if (mostBaseType.IsGenericType && mostBaseType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
                    {
                        IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
                        if (genericArguments.Count == 2)
                        {
                            interfaceType = genericArguments[1];
                        }
                    }
                    targetProperty = property.FindPropertyRelative(wrapProp.EditorPropertyName) ??
                                     SerializedUtils.FindPropertyByAutoPropertyName(property,
                                         wrapProp.EditorPropertyName);

                    if(targetProperty == null)
                    {
                        return ($"{wrapProp.EditorPropertyName} not found in {property.propertyPath}", null);
                    }

                    SerializedUtils.FieldOrProp wrapFieldOrProp = Util.GetWrapProp(wrapProp);
                    fieldType = wrapFieldOrProp.IsField
                        ? wrapFieldOrProp.FieldInfo.FieldType
                        : wrapFieldOrProp.PropertyInfo.PropertyType;

                    if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
                    {
                        fieldType = typeof(Component);
                    }
                }
            }

            if (targetProperty.objectReferenceValue != null)
            {
                return ("", null);
            }

            GetComponentInSceneAttribute getComponentInSceneAttribute = (GetComponentInSceneAttribute) saintsAttribute;

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

                Component findSelfComponent = interfaceType == null
                    ? rootGameObject.GetComponent(type)
                    : rootGameObject.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);
                if (findSelfComponent != null)
                {
                    componentInScene = findSelfComponent;
                    break;
                }

                Component findComponent  = interfaceType == null
                    ? rootGameObject.GetComponentInChildren(type, includeInactive)
                    : rootGameObject.GetComponentsInChildren(type, includeInactive).FirstOrDefault(interfaceType.IsInstanceOfType);

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

            if(!ReferenceEquals(targetProperty.objectReferenceValue, result))
            {
                targetProperty.objectReferenceValue = result;
            }
            return ("", result);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetComponentInScene";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_SCENE
            Debug.Log($"GetComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
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
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };

            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }
        #endregion

#endif
    }
}
