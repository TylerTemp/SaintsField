using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AddComponentAttribute), true)]
    public class AddComponentAttributeDrawer: SaintsPropertyDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            return DoCheckComponent(property, saintsAttribute, info);
        }

        private static bool DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info)
        {
            if (property.objectReferenceValue != null)
            {
                return false;
            }

            AddComponentAttribute getComponentAttribute = (AddComponentAttribute) saintsAttribute;
            Object target = property.serializedObject.targetObject;
            // Type fieldType = SerializedUtils.GetType(property);
            Type type = getComponentAttribute.CompType ?? info.FieldType;

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

#if UNITY_2021_3_OR_NEWER

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            DoCheckComponent(property, saintsAttribute, info);
            return new VisualElement();
        }

#endif
    }
}
