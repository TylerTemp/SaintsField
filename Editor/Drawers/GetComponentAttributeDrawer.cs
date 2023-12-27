using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentAttribute))]
    public class GetComponentAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            _error = "";

            if (property.objectReferenceValue != null)
            {
                return false;
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
                        return false;
                }

                property.objectReferenceValue = resultGo;
                SetValueChanged(property);
                return true;
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
                    return false;
            }

            Component componentOnSelf = transform.GetComponent(type);
            if (componentOnSelf == null)
            {
                _error = $"No {type} found on {transform.name}";
                return false;
            }

            UnityEngine.Object result = componentOnSelf;

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

            property.objectReferenceValue = result;
            SetValueChanged(property);
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);
    }
}
