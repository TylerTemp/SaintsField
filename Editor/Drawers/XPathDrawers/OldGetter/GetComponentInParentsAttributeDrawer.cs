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
    [CustomPropertyDrawer(typeof(GetComponentInParentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentsAttribute))]
    public class GetComponentInParentsAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
           GUIContent label,
           ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
           OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }

            (string error, UnityEngine.Object result) = DoCheckComponent(property, (GetComponentInParentsAttribute)saintsAttribute, info, parent);
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
        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);
        #endregion

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, GetComponentInParentsAttribute getComponentInParentsAttribute, FieldInfo info, object parent)
        {
            SerializedProperty targetProperty = property;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);  // ? why this can be array/list type?
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

            bool multiple = getComponentInParentsAttribute.Limit >= 1;

            if (targetProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                return ($"{targetProperty.propertyType} type is not supported by GetComponentInParent{(multiple? "s": "")}", null);
            }

            // if (targetProperty.objectReferenceValue != null)
            // {
            //     return ("", null);
            // }

            Type type = getComponentInParentsAttribute.CompType ?? fieldType;

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
                    return ($"GetComponentInParent{(multiple? "s": "")} can only be used on Component or GameObject", null);
            }

            List<Component> componentsInParents = new List<Component>();

            Transform curCheckingTrans = getComponentInParentsAttribute.ExcludeSelf
                ? transform.parent
                : transform;

            int levelLimit = getComponentInParentsAttribute.Limit > 0
                ? getComponentInParentsAttribute.Limit
                : int.MaxValue;

            bool isGameObject = type == typeof(GameObject);
            List<string> checkingNames = new List<string>();
            while (curCheckingTrans != null && levelLimit > 0)
            {
                if(!getComponentInParentsAttribute.IncludeInactive && !curCheckingTrans.gameObject.activeSelf)
                {
                    levelLimit--;
                    curCheckingTrans = curCheckingTrans.parent;
                    continue;
                }

                checkingNames.Add(curCheckingTrans.name);

                if (isGameObject)
                {
                    // componentInParent = curCheckingTrans;
                    componentsInParents.Add(curCheckingTrans);
                }
                else
                {
                    // componentInParent = interfaceType == null
                    //     ? curCheckingTrans.GetComponent(type)
                    //     : curCheckingTrans.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);
                    componentsInParents.AddRange(interfaceType == null
                        ? curCheckingTrans.GetComponents(type)
                        : curCheckingTrans.GetComponents(type).Where(interfaceType.IsInstanceOfType).ToArray()
                    );
                }
                // componentInParent = isGameObject
                //     ? curCheckingTrans
                //     : curCheckingTrans.GetComponent(type);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_PARENTS
                Debug.Log($"Search parent {levelLimit}, curCheckingTrans={curCheckingTrans}, componentInParent={componentInParent}");
#endif

                // if (componentInParent != null)
                // {
                //     break;
                // }
                levelLimit--;
                curCheckingTrans = curCheckingTrans.parent;
            }

            if (componentsInParents.Count == 0)
            {
                return ($"No {type} found in parent{(multiple? "s": "")}: {string.Join(", ", checkingNames)}", null);
            }

            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            // UnityEngine.Object result = componentInParent;
            // Debug.Log($"fieldType={fieldType}, type={type}, propPath={targetProperty.propertyPath}");
            foreach (Component componentInParent in componentsInParents)
            {
                if (fieldType != type)
                {
                    if(fieldType == typeof(GameObject))
                    {
                        results.Add(componentInParent.gameObject);
                    }
                    else
                    {
                        results.Add(interfaceType == null
                            ? componentInParent.GetComponent(fieldType)
                            : componentInParent.GetComponents(fieldType).FirstOrDefault(interfaceType.IsInstanceOfType));
                    }
                }
                else
                {
                    results.Add(componentInParent);
                }
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            // bool insideArray = ;
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

            if (targetProperty.objectReferenceValue != result)
            {
                targetProperty.objectReferenceValue = result;
                return ("", result);
            }

            return ("", null);
        }

        public static int HelperGetArraySize(SerializedProperty property, GetComponentInParentsAttribute getComponentInParentsAttribute, FieldInfo info)
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

            Type type = getComponentInParentsAttribute.CompType ?? fieldType;

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

            Transform curCheckingTrans = getComponentInParentsAttribute.ExcludeSelf
                ? transform.parent
                : transform;

            int levelLimit = getComponentInParentsAttribute.Limit > 0
                ? getComponentInParentsAttribute.Limit
                : int.MaxValue;

            bool isGameObject = type == typeof(GameObject);
            while (curCheckingTrans != null && levelLimit > 0)
            {
                if(!getComponentInParentsAttribute.IncludeInactive && !curCheckingTrans.gameObject.activeSelf)
                {
                    levelLimit--;
                    curCheckingTrans = curCheckingTrans.parent;
                    continue;
                }

                if (isGameObject)
                {
                    if (FilterComponent(curCheckingTrans, fieldType, interfaceType, type).Any())
                    {
                        return 1;
                    }
                }
                else
                {
                    // componentInParent = interfaceType == null
                    //     ? curCheckingTrans.GetComponent(type)
                    //     : curCheckingTrans.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);
                    IEnumerable<Component> components = curCheckingTrans.GetComponents(type);
                    if (interfaceType != null)
                    {
                        components = components.Where(interfaceType.IsInstanceOfType);
                    }

                    if (components.FirstOrDefault(each => FilterComponent(each, fieldType, interfaceType, type).Any()))
                    {
                        return 1;
                    }
                }
                // componentInParent = isGameObject
                //     ? curCheckingTrans
                //     : curCheckingTrans.GetComponent(type);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_PARENTS
                Debug.Log($"Search parent {levelLimit}, curCheckingTrans={curCheckingTrans}, componentInParent={componentInParent}");
#endif

                // if (componentInParent != null)
                // {
                //     break;
                // }
                levelLimit--;
                curCheckingTrans = curCheckingTrans.parent;
            }

            return 0;
        }

        private static IEnumerable<UnityEngine.Object> FilterComponent(Component componentInParent, Type fieldType, Type interfaceType, Type type)
        {
            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    yield return componentInParent.gameObject;
                    yield break;
                }

                IEnumerable<Component> getComponents = componentInParent.GetComponents(fieldType);
                if (interfaceType != null)
                {
                    getComponents = getComponents.Where(interfaceType.IsInstanceOfType);
                }

                foreach (Component component in getComponents)
                {
                    yield return component;
                }
            }
            else
            {
                yield return componentInParent;
            }
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit


        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetComponentInParents";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_PARENTS
            Debug.Log($"GetComponentInParents DrawPostFieldUIToolkit for {property.propertyPath}");
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

            (string error, UnityEngine.Object result) = DoCheckComponent(property, (GetComponentInParentsAttribute)saintsAttribute, info, parent);
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
