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

namespace SaintsField.Editor.Drawers.XPathDrawers.OldGetter
{
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute))]
    public class GetComponentInChildrenAttributeDrawer: SaintsPropertyDrawer
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

            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            IEnumerable<Transform> searchTargets = getComponentInChildrenAttribute.ExcludeSelf
                ? transform.Cast<Transform>()
                : new[]{ transform };

            foreach (Transform directChildTrans in searchTargets)
            {
                IEnumerable<Component> components =
                    directChildTrans.GetComponentsInChildren(type, getComponentInChildrenAttribute.IncludeInactive);
                if (interfaceType != null)
                {
                    components = components.Where(interfaceType.IsInstanceOfType);
                }

                results.AddRange(components.SelectMany(each => FilterComponent(each, type, fieldType)));
            }

            if (results.Count == 0)
            {
                return ($"No {type} found in {(getComponentInChildrenAttribute.ExcludeSelf ? "children" : "self or children")}", null);
            }

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

        private static IEnumerable<UnityEngine.Object> FilterComponent(Component component, Type type, Type fieldType)
        {
            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    yield return component.gameObject;
                    yield break;
                }

                foreach (Component target in component.GetComponents(fieldType))
                {
                    yield return target;
                }
            }
            else
            {
                yield return component;
            }
        }

        public static int HelperGetArraySize(SerializedProperty property, GetComponentInChildrenAttribute getComponentInChildrenAttribute, FieldInfo info)
        {
            if (EditorApplication.isPlaying)
            {
                return -1;
            }

            Type fieldType = info.FieldType.IsGenericType? info.FieldType.GetGenericArguments()[0]: info.FieldType.GetElementType();
            // Debug.Log(fieldType);
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

            if (getComponentInChildrenAttribute.CompType == typeof(GameObject))
            {
                return -1;
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
                    return -1;
            }

            IEnumerable<Transform> searchTargets = getComponentInChildrenAttribute.ExcludeSelf
                ? transform.Cast<Transform>()
                : new[]{ transform };

            foreach (Transform directChildTrans in searchTargets)
            {
                IEnumerable<Component> components =
                    directChildTrans.GetComponentsInChildren(type, getComponentInChildrenAttribute.IncludeInactive);
                if (interfaceType != null)
                {
                    components = components.Where(interfaceType.IsInstanceOfType);
                }

                if (components.Any(each => FilterComponent(each, type, fieldType).Any()))
                {
                    return 1;
                }
            }


            return 0;
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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(NamePlaceholder(property, index));
            if (helpBox.text != "")
            {
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (parent == null)
                {
                    Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    return;
                }

                DoCheckComponentUIToolkit(property, saintsAttribute, index, container, onValueChanged, info, parent);
            }
        }

        private static void DoCheckComponentUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

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
