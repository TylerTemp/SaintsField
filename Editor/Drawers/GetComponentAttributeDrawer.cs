using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using HelpBox = SaintsField.Editor.Utils.HelpBox;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentAttribute))]
    public class GetComponentAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            if (!DoCheckComponent(property, saintsAttribute))
            {
                return false;
            }
            SetValueChanged(property);
            return true;
        }

        protected override VisualElement DrawPostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, Action<object> onChange)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"GetComponent DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            Object added = DoCheckComponent(property, saintsAttribute);
            if (!added)
            {
                return null;
            }

            property.serializedObject.ApplyModifiedProperties();

            onChange?.Invoke(added);

            return new VisualElement
            {
                style =
                {
                    width = 0,
                },
            };
        }

        private Object DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            _error = "";

            if (property.objectReferenceValue != null)
            {
                return null;
            }

            GetComponentAttribute getComponentAttribute = (GetComponentAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);
            Type type = getComponentAttribute.CompType ?? fieldType;

            if (type == typeof(GameObject))
            {
                if (fieldType != typeof(GameObject))
                {
                    _error = $"You can not use GetComponent with field of {fieldType} type while looking for {type} type";
                }

                GameObject resultGo;
                switch (property.serializedObject.targetObject)
                {
                    case Component component:
                        resultGo = component.gameObject;
                        break;
                    case GameObject gameObject:
                        resultGo = gameObject;
                        break;
                    default:
                        _error = "GetComponent can only be used on Component or GameObject";
                        return null;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
                Debug.Log($"GetComponent Add {resultGo} for {property.propertyPath}");
#endif

                property.objectReferenceValue = resultGo;
                return resultGo;
            }

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
                    _error = "GetComponent can only be used on Component or GameObject";
                    return null;
            }

            Component componentOnSelf = transform.GetComponent(type);
            if (componentOnSelf == null)
            {
                _error = $"No {type} found on {transform.name}";
                return null;
            }

            Object result = componentOnSelf;

            // if (fieldType != type)
            // {
            //     if(fieldType == typeof(GameObject))
            //     {
            //         result = componentOnSelf.gameObject;
            //     }
            //     else
            //     {
            //         result = componentOnSelf.GetComponent(fieldType);
            //     }
            // }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"GetComponent Add {result} for {property.propertyPath}");
#endif

            property.objectReferenceValue = result;
            return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);

        protected override VisualElement DrawBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"GetComponent error {_error}");
#endif
            return _error != ""
                ? new UnityEngine.UIElements.HelpBox(_error, HelpBoxMessageType.Error)
                : null;
        }
    }
}
