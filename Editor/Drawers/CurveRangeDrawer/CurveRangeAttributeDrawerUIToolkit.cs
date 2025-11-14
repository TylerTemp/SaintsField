#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.CurveRangeDrawer
{
    public partial class CurveRangeAttributeDrawer
    {
        private static string NameCurveField(SerializedProperty property) => $"{property.propertyPath}__CurveRange";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            CurveRangeAttribute curveRangeAttribute = (CurveRangeAttribute)saintsAttribute;
            CustomCurveField createFieldElement = new CustomCurveField(GetPreferredLabel(property), curveRangeAttribute.Color.GetColor())
            {
                // value = property.animationCurveValue,
                bindingPath = property.propertyPath,
                ranges = GetRanges(curveRangeAttribute),
                name = NameCurveField(property),
            };

            createFieldElement.AddToClassList(ClassAllowDisable);
            createFieldElement.AddToClassList(CustomCurveField.alignedFieldUssClassName);

            return createFieldElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<CustomCurveField>(NameCurveField(property)).TrackPropertyValue(property, p => onValueChangedCallback.Invoke(p.animationCurveValue));
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, AnimationCurve value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            CurveRangeAttribute curveRangeAttribute = allAttributes.OfType<CurveRangeAttribute>().FirstOrDefault();

            if (oldElement is CustomCurveField oldF)
            {
                oldF.SetValueWithoutNotify(value);
                return null;
            }

            CustomCurveField field =
                curveRangeAttribute == null
                    ? new CustomCurveField(label)
                    : new CustomCurveField(label, curveRangeAttribute.Color.GetColor())
                    {
                        ranges = GetRanges(curveRangeAttribute),
                    };

            field.value = value;

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
#endif
