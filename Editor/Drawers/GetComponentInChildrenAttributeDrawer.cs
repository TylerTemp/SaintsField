using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentInChildrenAttribute))]
    public class GetComponentInChildrenAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            _error = "";

            if (property.objectReferenceValue != null)
            {
                return false;
            }

            GetComponentInChildrenAttribute getComponentInChildrenAttribute = (GetComponentInChildrenAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);

            if (getComponentInChildrenAttribute.CompType == typeof(GameObject))
            {
                _error = "You can not use GetComponentInChildren with GameObject type";
                return false;
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
                    _error = "GetComponentInChildrenAttribute can only be used on Component or GameObject";
                    return false;
            }

            // var directChildren = transform.Cast<Transform>();

            Component componentInChildren = null;
                // = transform.GetComponentInChildren(type, getComponentInChildrenAttribute.IncludeInactive);
            foreach (Transform directChildTrans in transform.Cast<Transform>())
            {
                componentInChildren = directChildTrans.GetComponentInChildren(type, getComponentInChildrenAttribute.IncludeInactive);
                if (componentInChildren != null)
                {
                    break;
                }
            }

            if (componentInChildren == null)
            {
                _error = $"No {type} found in children";
                return false;
            }

            UnityEngine.Object result = componentInChildren;

            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    result = componentInChildren.gameObject;
                }
                else
                {
                    result = componentInChildren.GetComponent(fieldType);
                }
            }

            property.objectReferenceValue = result;
            SetValueChanged(property);
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);
    }
}
