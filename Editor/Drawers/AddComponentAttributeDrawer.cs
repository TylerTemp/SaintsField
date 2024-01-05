using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AddComponentAttribute))]
    public class AddComponentAttributeDrawer: SaintsPropertyDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            return DoCheckComponent(property, saintsAttribute);
        }

        private static bool DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            if (property.objectReferenceValue != null)
            {
                return false;
            }

            AddComponentAttribute getComponentAttribute = (AddComponentAttribute) saintsAttribute;
            Object target = property.serializedObject.targetObject;
            // Type fieldType = SerializedUtils.GetType(property);
            Type type = getComponentAttribute.CompType ?? SerializedUtils.GetType(property);

            Component foundComponent = null;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (target)
            {
                case GameObject gameObject:
                    foundComponent = gameObject.GetComponent(type);
                    break;
                case Component component:
                    foundComponent = component.GetComponent(type);
                    break;
            }

            if (foundComponent != null)
            {
                return false;
            }

            GameObject obj = target as GameObject ?? ((Component) target).gameObject;
            obj.AddComponent(type);

            return true;
        }

        protected override VisualElement CreateAboveUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            DoCheckComponent(property, saintsAttribute);
            return new VisualElement();
        }
    }
}
