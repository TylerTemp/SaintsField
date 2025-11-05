using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Interfaces;
using UnityEditor;
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
            GuidStringElement element = new GuidStringElement
            {
                bindingPath = property.propertyPath,
            };
            element.BindProp(property);
            GuidStringField field = new GuidStringField(GetPreferredLabel(property), element);
            field.AddToClassList(ClassAllowDisable);
            field.AddToClassList(GuidStringField.alignedFieldUssClassName);

            return field;
        }
    }
}
