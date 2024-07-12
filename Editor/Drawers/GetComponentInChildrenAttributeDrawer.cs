using System;
using System.Collections.Generic;
using System.Linq;
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
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute))]
    public class GetComponentInChildrenAttributeDrawer: SaintsPropertyDrawer
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
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type interfaceType = null;
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                object propertyValue = SerializedUtils.GetValue(property, info, parent);

                if (propertyValue is IWrapProp wrapProp)
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

            if (targetProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                return ($"{targetProperty.propertyType} type is not supported by GetComponentInChildren", null);
            }

            // if (targetProperty.objectReferenceValue != null)
            // {
            //     return ("", null);
            // }

            GetComponentInChildrenAttribute getComponentInChildrenAttribute = (GetComponentInChildrenAttribute) saintsAttribute;

            if (getComponentInChildrenAttribute.CompType == typeof(GameObject))
            {
                return ("You can not use GetComponentInChildren with GameObject type", null);
            }

            Type type = getComponentInChildrenAttribute.CompType ?? fieldType;

            Transform transform;
            switch (property.serializedObject.targetObject)
            {
                case Component component:
                    transform = component.transform;
                    break;
                case GameObject gameObject:
                    transform = gameObject.transform;
                    break;
                default:
                    return ("GetComponentInChildrenAttribute can only be used on Component or GameObject", null);
            }

            // var directChildren = transform.Cast<Transform>();

            List<Component> componentInChildrenList = new List<Component>();
                // = transform.GetComponentInChildren(type, getComponentInChildrenAttribute.IncludeInactive);
            foreach (Transform directChildTrans in transform.Cast<Transform>())
            {
                Component[] componentInChildren = interfaceType == null
                    ? directChildTrans.GetComponentsInChildren(type, getComponentInChildrenAttribute.IncludeInactive)
                    : directChildTrans.GetComponentsInChildren(type, getComponentInChildrenAttribute.IncludeInactive).Where(interfaceType.IsInstanceOfType).ToArray();

                // if (componentInChildren != null)
                // {
                //     break;
                // }

                componentInChildrenList.AddRange(componentInChildren);
            }

            if (componentInChildrenList.Count == 0)
            {
                return ($"No {type} found in children", null);
            }

            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            foreach (Component componentInChildren in componentInChildrenList)
            {
                UnityEngine.Object target = componentInChildren;

                if (fieldType != type)
                {
                    if(fieldType == typeof(GameObject))
                    {
                        target = componentInChildren.gameObject;
                    }
                    else
                    {
                        target = componentInChildren.GetComponent(fieldType);
                    }
                }
                results.Add(target);
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (indexInArray == 0)
            {
                SerializedProperty arrayProp = SerializedUtils.GetArrayProperty(property).property;
                // Debug.Log($"arr size {arrayProp.arraySize} cur count {results.Count}");
                if (arrayProp.arraySize != results.Count)
                {
                    // Debug.Log($"update size {arrayProp.arraySize} to {results.Count}");
                    arrayProp.arraySize = results.Count;
                    arrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
            int useIndexInArray = indexInArray != -1 ? indexInArray: 0;

            if (useIndexInArray >= results.Count)
            {
                return ($"No {type} found on {transform.name}{(indexInArray == -1 ? "": $"[{indexInArray}]")}", null);
            }

            UnityEngine.Object result = results[useIndexInArray];

            // targetProperty.objectReferenceValue = result;
            if (targetProperty.objectReferenceValue != result)
            {
                targetProperty.objectReferenceValue = result;
                return ("", result);
            }
            return ("", null);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit


        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetComponentInChildren";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_CHILDREN
            Debug.Log($"GetComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            DoCheckComponentUIToolkit(property, saintsAttribute, index, container, onValueChangedCallback, info, parent);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NamePlaceholder(property, index));
            if (helpBox.text != "")
            {
                DoCheckComponentUIToolkit(property, saintsAttribute, index, container, onValueChanged, info, parent);
            }
        }

        private static void DoCheckComponentUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
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
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NamePlaceholder(property, index),
            };

            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }

        #endregion

#endif
    }
}
