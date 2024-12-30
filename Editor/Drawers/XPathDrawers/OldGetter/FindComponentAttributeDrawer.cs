using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.OldGetter
{
    [CustomPropertyDrawer(typeof(FindComponentAttribute))]
    public class FindComponentAttributeDrawer: SaintsPropertyDrawer
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

            (string error, Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
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

        private static (string error, Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if (SerializedUtils.PropertyPathIndex(property.propertyPath) != -1)
            {
                return ("FindComponent can not be used on array/list", null);
            }

            SerializedProperty targetProperty = property;

            Type fieldType = info.FieldType;
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

            if (targetProperty.objectReferenceValue != null)
            {
                return ("", null);
            }

            FindComponentAttribute findComponentAttribute = (FindComponentAttribute) saintsAttribute;

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
                    return ("FindComponent can only be used on Component or GameObject", null);
            }

            Object componentInChildren = null;
            foreach (string findPath in findComponentAttribute.Paths)
            {
                Transform findTarget = transform.Find(findPath);
                if (!findTarget)
                {
                    continue;
                }

                if(fieldType == typeof(GameObject))
                {
                    componentInChildren = findTarget.gameObject;
                    break;
                }

                componentInChildren = interfaceType == null
                    ? findTarget.GetComponent(fieldType)
                    : findTarget.GetComponents(fieldType).FirstOrDefault(interfaceType.IsInstanceOfType);

                if (componentInChildren != null)
                {
                    break;
                }
            }

            if (componentInChildren == null)
            {
                return ($"No {fieldType} found in paths: {string.Join(", ", findComponentAttribute.Paths)}", null);
            }

            targetProperty.objectReferenceValue = componentInChildren;

            return ("", componentInChildren);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FindComponent";

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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_FIND_COMPONENT
            Debug.Log($"FindComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, Object result) = DoCheckComponent(property, saintsAttribute, info, parent);
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
        #endregion

#endif
    }
}
