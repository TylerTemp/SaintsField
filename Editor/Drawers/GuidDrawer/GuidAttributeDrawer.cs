using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.GuidDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(GuidAttribute), true)]
    public class GuidAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement field = MakeElement(property, GetPreferredLabel(property));
            field.AddToClassList(GuidStringField.alignedFieldUssClassName);
            return field;
        }

        private static VisualElement MakeElement(SerializedProperty property, string label)
        {
            GuidStringElement timeSpanElement = new GuidStringElement
            {
                bindingPath = property.propertyPath,
            };
            timeSpanElement.BindProp(property);
            timeSpanElement.Bind(property.serializedObject);

            GuidStringField element = new GuidStringField(label, timeSpanElement);

            element.AddToClassList(ClassAllowDisable);

            return element;
        }

        public static VisualElement RenderSerializedActual(string label, SerializedProperty property, bool inHorizontal)
        {
            VisualElement r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.stringValue)), label);
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(TimeSpanField.alignedFieldUssClassName);
            }

            return r;
        }
    }
}
