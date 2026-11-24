using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BuiltIn
{
    public static class ColorUsageAttributeDrawer
    {
        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, ColorUsageAttribute colorUsageAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not Color value)
            {
                string message = $"{label}: {rawValue}({rawValue?.GetType().ToString() ?? "null"}) is not a Color";
                if (oldElement is HelpBox helpBox)
                {
                    UIToolkitUtils.SetHelpBox(helpBox, message);
                    return null;
                }

                return new HelpBox(message, HelpBoxMessageType.Error);
            }

            if (oldElement is ColorField cf)
            {
                cf.SetValueWithoutNotify(value);
                return null;
            }

            ColorField field = new ColorField(label)
            {
                value = value,
                showAlpha = colorUsageAttribute.showAlpha,
                hdr = colorUsageAttribute.hdr,
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
