using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BuiltIn
{
    public static class GradientUsageAttributeDrawer
    {
        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, GradientUsageAttribute gradientUsageAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            // null is a valid Gradient value, so only a non-null mismatch is an error
            if (rawValue != null && rawValue is not Gradient)
            {
                string message = $"{label}: {rawValue}({rawValue.GetType()}) is not a Gradient";
                if (oldElement is HelpBox helpBox)
                {
                    UIToolkitUtils.SetHelpBox(helpBox, message);
                    return null;
                }

                return new HelpBox(message, HelpBoxMessageType.Error);
            }

            Gradient value = (Gradient)rawValue;

            if (oldElement is GradientField oldGradientField)
            {
                oldGradientField.SetValueWithoutNotify(value);
                return null;
            }

            GradientField field = new GradientField(label)
            {
                value = value,
                colorSpace = gradientUsageAttribute.colorSpace,
#if UNITY_2022_1_OR_NEWER
                hdr = gradientUsageAttribute.hdr,
#endif
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(evt.newValue);
                });
            }

            return field;
        }
    }
}
