using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FindComponentAttribute))]
    public class FindComponentAttributeDrawer: SaintsPropertyDrawer
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

            FindComponentAttribute findComponentAttribute = (FindComponentAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);

            // Type type = findComponentAttribute.CompType ?? fieldType;

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

            UnityEngine.Object componentInChildren = null;
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

                if (fieldType == typeof(Transform))
                {
                    componentInChildren = findTarget;
                    break;
                }

                componentInChildren = findTarget.GetComponent(fieldType);
                if (componentInChildren != null)
                {
                    break;
                }
            }

            if (componentInChildren == null)
            {
                _error = $"No {fieldType} found in paths: {string.Join(", ", findComponentAttribute.Paths)}";
                return false;
            }

            property.objectReferenceValue = componentInChildren;
            SetValueChanged(property);
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);
    }
}
