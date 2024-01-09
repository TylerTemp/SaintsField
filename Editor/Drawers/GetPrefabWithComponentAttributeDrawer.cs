using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    public class GetPrefabWithComponentAttributeDrawer: SaintsPropertyDrawer
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

            GetPrefabWithComponentAttribute getPrefabWithComponentAttribute = (GetPrefabWithComponentAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);

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
                if (toCheck is null)
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

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetPrefabWithComponent";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent,
            Action<object> onChange)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_PREFAB_WITH_COMPONENT
            Debug.Log($"GetPrefabWithComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, UnityEngine.Object result) = DoCheckComponent(property, saintsAttribute);
            if (error != "")
            {
                return new VisualElement
                {
                    style =
                    {
                        width = 0,
                    },
                    name = NamePlaceholder(property, index),
                    userData = error,
                };
            }

            property.serializedObject.ApplyModifiedProperties();

            onChange?.Invoke(result);

            return new VisualElement
            {
                style =
                {
                    width = 0,
                },
                name = NamePlaceholder(property, index),
                userData = "",
            };
        }

        // NOTE: ensure the post field is added to the container!
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            string error = (string)(container.Q<VisualElement>(NamePlaceholder(property, index))!.userData ?? "");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_PREFAB_WITH_COMPONENT
            Debug.Log($"GetPrefabWithComponent error {error}");
#endif
            return string.IsNullOrEmpty(error)
                ? null
                : new HelpBox(_error, HelpBoxMessageType.Error);
        }
        #endregion
    }
}
