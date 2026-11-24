using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BuiltIn
{
    public static class MultilineAttributeDrawer
    {
        private const float RowHeight = 15f;

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, MultilineAttribute multilineAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not string value)
            {
                string message = $"{label}: {rawValue}({rawValue?.GetType().ToString() ?? "null"}) is not a string";
                if (oldElement is HelpBox helpBox)
                {
                    UIToolkitUtils.SetHelpBox(helpBox, message);
                    return null;
                }

                return new HelpBox(message, HelpBoxMessageType.Error);
            }

            if (oldElement is TextField tf)
            {
                tf.SetValueWithoutNotify(value);
                return null;
            }

            TextField field = new TextField(label)
            {
                value = value,
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                },
            };

            float minHeight = Mathf.Max(multilineAttribute.lines, 1) * RowHeight;
            field.style.minHeight = minHeight;
            VisualElement textInput = field.Q(name: "unity-text-input");
            if (textInput != null)
            {
                textInput.style.minHeight = minHeight;
            }

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
