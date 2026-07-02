#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PlaceholderDrawerOfType
{
    public partial class PlaceholderDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement field = new VisualElement()
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            if (!string.IsNullOrEmpty(property.tooltip))
            {
                field.tooltip = property.tooltip;
            }
            return field;
        }
    }
}
#endif
