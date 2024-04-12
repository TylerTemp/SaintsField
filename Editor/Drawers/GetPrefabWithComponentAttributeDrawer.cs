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
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    public class GetPrefabWithComponentAttributeDrawer: SaintsPropertyDrawer
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
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute, info);
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

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info)
        {
            if (property.objectReferenceValue != null)
            {
                return ("", null);
            }

            GetPrefabWithComponentAttribute getPrefabWithComponentAttribute = (GetPrefabWithComponentAttribute) saintsAttribute;
            Type fieldType = info.FieldType;

            if (getPrefabWithComponentAttribute.CompType == typeof(GameObject))
            {
                return ("You can not use GetPrefabWithComponentAttribute with GameObject type", null);
            }

            Type type = getPrefabWithComponentAttribute.CompType ?? fieldType;

            Component prefabWithComponent = null;

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject toCheck = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (toCheck == null)
                {
                    continue;
                }

                Component findSelfComponent = toCheck.GetComponent(type);
                if (findSelfComponent != null)
                {
                    prefabWithComponent = findSelfComponent;
                    break;
                }

                // Component findComponent = rootGameObject.GetComponentInChildren(type, includeInactive);
                // // ReSharper disable once InvertIf
                // if (findComponent != null)
                // {
                //     prefabWithComponent = findComponent;
                //     break;
                // }
            }

            if (prefabWithComponent == null)
            {
                return ($"No {type} found with prefab", null);
            }

            UnityEngine.Object result = prefabWithComponent;

            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    result = prefabWithComponent.gameObject;
                }
                else
                {
                    result = prefabWithComponent.GetComponent(fieldType);
                }
            }

            property.objectReferenceValue = result;
            return ("", result);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetPrefabWithComponent";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_PREFAB_WITH_COMPONENT
            Debug.Log($"GetPrefabWithComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute, info);
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
