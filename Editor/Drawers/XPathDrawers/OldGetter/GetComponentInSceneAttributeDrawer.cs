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

namespace SaintsField.Editor.Drawers.XPathDrawers.OldGetter
{
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    public class GetComponentInSceneAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }

            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
            if (error != "")
            {
                _error = error;
                return false;
            }
            if(result != null)
            {
                onGUIPayload.SetValue(result);
                if(ExpandableIMGUIScoop.IsInScoop)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);
        #endregion

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            SerializedProperty targetProperty = property;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type interfaceType = null;
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

                if (error == "" && propertyValue is IWrapProp wrapProp)
                {
                    Util.SaintsInterfaceInfo saintsInterfaceInfo = Util.GetSaintsInterfaceInfo(property, wrapProp);
                    if(saintsInterfaceInfo.Error != "")
                    {
                        return (saintsInterfaceInfo.Error, null);
                    }

                    fieldType = saintsInterfaceInfo.FieldType;
                    targetProperty = saintsInterfaceInfo.TargetProperty;
                    interfaceType = saintsInterfaceInfo.InterfaceType;

                    if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
                    {
                        fieldType = typeof(Component);
                    }
                }
            }

            // if (targetProperty.objectReferenceValue != null)
            // {
            //     return ("", null);
            // }

            GetComponentInSceneAttribute getComponentInSceneAttribute = (GetComponentInSceneAttribute) saintsAttribute;

            if (getComponentInSceneAttribute.CompType == typeof(GameObject))
            {
                return ("You can not use GetComponentInScene with GameObject type", null);
            }

            Type type = getComponentInSceneAttribute.CompType ?? fieldType;

            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            Scene scene = SceneManager.GetActiveScene();
            bool includeInactive = getComponentInSceneAttribute.IncludeInactive;
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (!includeInactive && !rootGameObject.activeSelf)
                {
                    continue;
                }

                IEnumerable<Component> components = rootGameObject.GetComponentsInChildren(type, includeInactive);
                if (interfaceType != null)
                {
                    components = components.Where(interfaceType.IsInstanceOfType);
                }

                results.AddRange(components.SelectMany(each => FilterComponent(each, type, fieldType)));
            }

            if (results.Count == 0)
            {
                return ($"No {type} found in scene", null);
            }

            // List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (indexInArray == 0)
            {
                SerializedProperty arrayProp = SerializedUtils.GetArrayProperty(property).property;
                if (arrayProp.arraySize != results.Count)
                {
                    arrayProp.arraySize = results.Count;
                    arrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
            int useIndexInArray = indexInArray != -1 ? indexInArray: 0;

            if (useIndexInArray >= results.Count)
            {
                return ($"No {type} found in scene{(indexInArray == -1 ? "": $"[{indexInArray}]")}", null);
            }

            UnityEngine.Object result = results[useIndexInArray];

            if(!ReferenceEquals(targetProperty.objectReferenceValue, result))
            {
                targetProperty.objectReferenceValue = result;
            }
            return ("", result);
        }

        private static IEnumerable<UnityEngine.Object> FilterComponent(Component component, Type type, Type fieldType)
        {
            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    yield return component.gameObject;
                    yield break;
                }

                foreach (Component foundComp in component.GetComponents(fieldType))
                {
                    yield return foundComp;
                }
            }
            else
            {
                yield return component;
            }
        }

        public static int HelperGetArraySize(GetComponentInSceneAttribute getComponentInSceneAttribute, FieldInfo info)
        {
            if (EditorApplication.isPlaying)
            {
                return -1;
            }

            Type fieldType = info.FieldType.IsGenericType? info.FieldType.GetGenericArguments()[0]: info.FieldType.GetElementType();
            if (fieldType == null)
            {
                return -1;
            }

            Type interfaceType = null;

            if (typeof(IWrapProp).IsAssignableFrom(fieldType))
            {
                Type mostBaseType = ReflectUtils.GetMostBaseType(fieldType);
                if (mostBaseType.IsGenericType && mostBaseType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
                {
                    IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
                    if (genericArguments.Count == 2)
                    {
                        fieldType = genericArguments[0];
                        interfaceType = genericArguments[1];
                    }
                }

                if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
                {
                    fieldType = typeof(Component);
                }
            }

            Type type = getComponentInSceneAttribute.CompType ?? fieldType;

            Scene scene = SceneManager.GetActiveScene();
            bool includeInactive = getComponentInSceneAttribute.IncludeInactive;
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (!includeInactive && !rootGameObject.activeSelf)
                {
                    continue;
                }

                bool found = (interfaceType == null
                        ? rootGameObject.GetComponentsInChildren(type, includeInactive)
                        : rootGameObject.GetComponentsInChildren(type, includeInactive)
                            .Where(interfaceType.IsInstanceOfType))
                    .Any();

                if (found)
                {
                    return 1;
                }
            }

            return 0;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetComponentInScene";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

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
