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
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    public class GetPrefabWithComponentAttributeDrawer: SaintsPropertyDrawer
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
                return ($"{targetProperty.propertyType} type is not supported by GetPrefabWithComponent", null);
            }

            // if (targetProperty.objectReferenceValue != null)
            // {
            //     return ("", null);
            // }

            GetPrefabWithComponentAttribute getPrefabWithComponentAttribute = (GetPrefabWithComponentAttribute) saintsAttribute;

            if (getPrefabWithComponentAttribute.CompType == typeof(GameObject))
            {
                return ("You can not use GetPrefabWithComponentAttribute with GameObject type", null);
            }

            Type type = getPrefabWithComponentAttribute.CompType ?? fieldType;

            // List<Component> prefabWithComponents = new List<Component>();
            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject toCheck = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (toCheck == null)
                {
                    continue;
                }

                Component findSelfComponent = interfaceType == null
                    ? toCheck.GetComponent(type)
                    : toCheck.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);

                if (findSelfComponent != null)
                {
                    UnityEngine.Object findResult = findSelfComponent;

                    if (fieldType != type)
                    {
                        if(fieldType == typeof(GameObject))
                        {
                            findResult = findSelfComponent.gameObject;
                            results.Add(findResult);
                        }
                        else
                        {
                            findResult = interfaceType == null
                                ? findSelfComponent.GetComponent(fieldType)
                                : findSelfComponent.GetComponents(fieldType).FirstOrDefault(interfaceType.IsInstanceOfType);
                            if (findResult != null)
                            {
                                results.Add(findResult);
                            }
                        }
                    }
                    else
                    {
                        results.Add(findResult);
                    }
                }
            }

            if (results.Count == 0)
            {
                return ($"No {type} found with prefab", null);
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
                return ($"No {type} found on any prefab {(indexInArray == -1 ? "": $"[{indexInArray}]")}", null);
            }

            UnityEngine.Object result = results[useIndexInArray];

            if (targetProperty.objectReferenceValue != result)
            {
                targetProperty.objectReferenceValue = result;
            }

            return ("", result);
        }

        public static int HelperGetArraySize(GetPrefabWithComponentAttribute getPrefabWithComponentAttribute, FieldInfo info)
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

            if (getPrefabWithComponentAttribute.CompType == typeof(GameObject))
            {
                return -1;
            }

            Type type = getPrefabWithComponentAttribute.CompType ?? fieldType;

            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject toCheck = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (toCheck == null)
                {
                    continue;
                }

                Component findSelfComponent = interfaceType == null
                    ? toCheck.GetComponent(type)
                    : toCheck.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);

                if (findSelfComponent != null)
                {
                    UnityEngine.Object findResult = findSelfComponent;

                    if (fieldType != type)
                    {
                        if(fieldType == typeof(GameObject))
                        {
                            return 1;
                        }
                        else
                        {
                            findResult = interfaceType == null
                                ? findSelfComponent.GetComponent(fieldType)
                                : findSelfComponent.GetComponents(fieldType).FirstOrDefault(interfaceType.IsInstanceOfType);
                            if (findResult != null)
                            {
                                return 1;
                            }
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            return 0;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetPrefabWithComponent";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_PREFAB_WITH_COMPONENT
            Debug.Log($"GetPrefabWithComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
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
