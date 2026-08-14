#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.RateDrawer
{
    public partial class RateAttributeDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            RateField r = new RateField(GetPreferredLabel(property), (RateAttribute)saintsAttribute)
                {
                    bindingPath = property.propertyPath,
                };

            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(RateField.alignedFieldUssClassName);
            if (!string.IsNullOrEmpty(property.tooltip) && r.labelElement != null)
            {
                r.labelElement.tooltip = property.tooltip;
            }
            return r;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            RateField field = container.Q<RateField>();
            UIToolkitUtils.AddContextualMenuManipulator(
                container.Q<RateField>(),
                property,
                () => onValueChangedCallback(property.intValue));
            field.TrackPropertyValue(property, p => onValueChangedCallback.Invoke(p.intValue));
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, RateAttribute rateAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (oldElement is RateField rateField)
            {
                rateField.SetValueWithoutNotify(value);
                return null;
            }

            RateField element =
                new RateField(label, rateAttribute);
            element.SetValueWithoutNotify(value);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return element;
        }
    }
}
#endif
