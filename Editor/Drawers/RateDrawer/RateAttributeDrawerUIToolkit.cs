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
            RateElement rateElement = new RateElement((RateAttribute)saintsAttribute)
            {
                bindingPath = property.propertyPath,
            };

            RateField r = new RateField(GetPreferredLabel(property), rateElement);
            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(RateField.alignedFieldUssClassName);
            return r;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(
                container.Q<RateField>(),
                property,
                () => onValueChangedCallback(property.intValue));
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, RateAttribute rateAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (oldElement is RateField rateField)
            {
                rateField.value = value;
                return null;
            }

            RateElement visualInput = new RateElement(rateAttribute)
            {
                value = value,
            };
            RateField element =
                new RateField(label, visualInput)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                visualInput.RegisterValueChangedCallback(evt =>
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
