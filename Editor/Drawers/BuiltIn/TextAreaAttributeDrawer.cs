using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BuiltIn
{
    public static class TextAreaAttributeDrawer
    {
        // Unity's `TextAreaDrawer` uses this for the text box, and a full line height for the label above it
        private const float RowHeight = 13f;

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, TextAreaAttribute textAreaAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
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

            int minLines = Mathf.Max(textAreaAttribute.minLines, 1);
            int maxLines = Mathf.Max(textAreaAttribute.maxLines, minLines);

            TextField field = new TextField(label)
            {
                value = value,
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                },
            };
#if UNITY_2023_1_OR_NEWER
            field.verticalScrollerVisibility = ScrollerVisibility.Auto;
#elif UNITY_2022_3_OR_NEWER
            field.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif

            VisualElement textInput = field.Q(name: "unity-text-input");
            if (textInput != null)
            {
                textInput.style.minHeight = minLines * RowHeight;
                textInput.style.maxHeight = maxLines * RowHeight;
            }

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, true);

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
