#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LeftToggleDrawer
{
    public partial class LeftToggleAttributeDrawer
    {
        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, LeftToggleAttribute leftToggleAttribute, string label, object value, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (value is not bool boolValue)
            {
                return oldElement ?? new HelpBox($"Value {value}({valueType}) is not a bool", HelpBoxMessageType.Error);
            }

            if (oldElement is LeftToggleField oldField)
            {
                oldField.SetValueWithoutNotify(boolValue);
                return null;
            }

            LeftToggleField field = new LeftToggleField(label)
            {
                value = boolValue,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }

            return field;
        }
    }
}
#endif
