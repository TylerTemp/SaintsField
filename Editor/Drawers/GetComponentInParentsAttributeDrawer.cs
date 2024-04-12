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
    [CustomPropertyDrawer(typeof(GetComponentInParentAttribute))]
    [CustomPropertyDrawer(typeof(GetComponentInParentsAttribute))]
    public class GetComponentInParentsAttributeDrawer: SaintsPropertyDrawer
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
            (string error, UnityEngine.Object result) = DoCheckComponent(property, (GetComponentInParentsAttribute)saintsAttribute, info);
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

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, GetComponentInParentsAttribute getComponentInParentsAttribute, FieldInfo info)
        {
            if (property.objectReferenceValue != null)
            {
                return ("", null);
            }

            // GetComponentInChildrenAttribute getComponentInChildrenAttribute = (GetComponentInChildrenAttribute) saintsAttribute;
            Type fieldType = info.FieldType;

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
                    return ("GetComponentInParent(s)Attribute can only be used on Component or GameObject", null);
            }

            Component componentInParent = null;

            Transform curCheckingTrans = transform;
            int levelLimit = getComponentInParentsAttribute.Limit > 0
                ? getComponentInParentsAttribute.Limit
                : int.MaxValue;

            bool isGameObject = type == typeof(GameObject);
            while (componentInParent == null && curCheckingTrans != null && levelLimit > 0)
            {
                curCheckingTrans = curCheckingTrans.parent;
                if (curCheckingTrans == null)
                {
                    break;
                }

                componentInParent = isGameObject
                    ? curCheckingTrans
                    : curCheckingTrans.GetComponent(type);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_PARENTS
                Debug.Log($"Search parent {levelLimit}, curCheckingTrans={curCheckingTrans}, componentInParent={componentInParent}");
#endif

                if (componentInParent != null)
                {
                    break;
                }
                levelLimit--;
            }

            if (componentInParent == null)
            {
                return ($"No {type} found in parent(s)", null);
            }

            UnityEngine.Object result = componentInParent;

            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    result = componentInParent.gameObject;
                }
                else
                {
                    result = componentInParent.GetComponent(fieldType);
                }
            }

            property.objectReferenceValue = result;
            return ("", result);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit


        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetComponentInParents";

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_PARENTS
            Debug.Log($"GetComponentInParents DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, UnityEngine.Object result) = DoCheckComponent(property, (GetComponentInParentsAttribute)saintsAttribute, info);
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
