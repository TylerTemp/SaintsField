using System;
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public partial class LayerAttributeDrawer
    {
        public static VisualElement UIToolkitValueEditLayerMask(VisualElement oldElement, string label, LayerMask value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is LayerMaskDropdownField lmdf)
            {
                lmdf.SetValueWithoutNotify(value.value);
                return null;
            }

            LayerMaskDropdownElement layerMaskDropdownElement = new LayerMaskDropdownElement
            {
                value = value.value,
            };
            LayerMaskDropdownField element =
                new LayerMaskDropdownField(label, layerMaskDropdownElement)
                {
                    value = value.value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                layerMaskDropdownElement.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull((LayerMask)evt.newValue);
                });
            }
            return element;
        }

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is IntDropdownField intDropdownField)
            {
                LayerIntDropdownElement layerIntDropdownElement = intDropdownField.Q<LayerIntDropdownElement>();
                if(layerIntDropdownElement != null)
                {
                    layerIntDropdownElement.SetValueWithoutNotify(value);
                    return null;
                }
            }

            LayerIntDropdownElement intDropdownElement = new LayerIntDropdownElement
            {
                value = value,
            };
            IntDropdownField element = new IntDropdownField(label, intDropdownElement);
            intDropdownElement.BindDrop(element);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                intDropdownElement.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return element;
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is StringDropdownField stringDropdownField)
            {
                LayerStringDropdownElement layerStringDropdownElement =
                    stringDropdownField.Q<LayerStringDropdownElement>();
                if(layerStringDropdownElement != null)
                {
                    // Debug.Log($"renderer update string {value}");
                    layerStringDropdownElement.SetValueWithoutNotify(value);
                    return null;
                }
            }

            LayerStringDropdownElement stringDropdownElement = new LayerStringDropdownElement
            {
                value = value,
            };
            StringDropdownField element = new StringDropdownField(label, stringDropdownElement);
            stringDropdownElement.BindDrop(element);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                stringDropdownElement.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    // Debug.Log($"renderer set string {evt.newValue}");
                    setterOrNull(evt.newValue);
                });
            }

            return element;
        }
    }
}
