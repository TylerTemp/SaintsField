#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DateTimeAttribute), true)]
    public class DateTimeAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement r = MakeElement(property, GetPreferredLabel(property));
            r.AddToClassList(DateTimeField.alignedFieldUssClassName);
            return r;
        }

        public static VisualElement RenderSerializedActual(ISaintsAttribute dateTimeAttribute, string label, SerializedProperty property, bool inHorizontal)
        {
            VisualElement r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue)), label);
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(DateTimeField.alignedFieldUssClassName);
            }

            return r;
        }

        private static VisualElement MakeElement(SerializedProperty property, string label)
        {
            DateTimeElement dateTimeElement = new DateTimeElement();
            dateTimeElement.BindPath(property.propertyPath);
            dateTimeElement.Bind(property.serializedObject);

            DateTimeField element = new DateTimeField(label, dateTimeElement);

            element.AddToClassList(ClassAllowDisable);

            return element;
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            bool isLong = valueType == typeof(long) || value is long;

            long ticks = isLong? (long) value: ((DateTime)value).Ticks;
            if (oldElement is DateTimeField dtField)
            {
                // Debug.Log($"old element set ticks {ticks}");
                dtField.SetValueWithoutNotify(ticks);
                return null;
            }

            DateTimeElement dateTimeElement = new DateTimeElement
            {
                value = ticks,
            };

            DateTimeField element = new DateTimeField(label, dateTimeElement);

            element.AddToClassList(ClassAllowDisable);

            if (labelGrayColor)
            {
                element.labelElement.style.color = AbsRenderer.ReColor;
            }
            if (inHorizontalLayout)
            {
                element.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                element.AddToClassList(DateTimeField.alignedFieldUssClassName);
            }
            if (setterOrNull == null)
            {
                element.SetEnabled(false);
                element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
            }
            else
            {
                element.AddToClassList(ClassAllowDisable);
                element.RegisterValueChangedCallback(evt =>
                {
                    object invokeValue;
                    if (isLong)
                    {
                        invokeValue = evt.newValue;
                    }
                    else
                    {
                        invokeValue = new DateTime(evt.newValue);
                    }
                    beforeSet?.Invoke(invokeValue);
                    setterOrNull.Invoke(invokeValue);
                });
            }

            return element;
        }
    }
}
#endif
