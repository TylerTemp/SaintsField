using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AddComponentAttribute))]
    public class AddComponentAttributeDrawer: SaintsPropertyDrawer
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

            AddComponentAttribute getComponentAttribute = (AddComponentAttribute) saintsAttribute;
            Object target = property.serializedObject.targetObject;

            Component foundComponent = null;
            switch (target)
            {
                case GameObject gameObject:
                    foundComponent = gameObject.GetComponent(getComponentAttribute.CompType);
                    break;
                case Component component:
                    foundComponent = component.GetComponent(getComponentAttribute.CompType);
                    break;
            }

            if (foundComponent != null)
            {
                return false;
            }

            GameObject obj = target as GameObject ?? ((Component) target).gameObject;
            obj.AddComponent(getComponentAttribute.CompType);

            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);
    }
}
