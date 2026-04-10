using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public partial class SaintsDecimalDrawer
    {
        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, decimal value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is DecimalTextField dtf)
            {
                dtf.SetValueWithoutNotify(value);
                return null;
            }

            DecimalTextField field = new DecimalTextField(label)
            {
                value = value,
            };
            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
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
