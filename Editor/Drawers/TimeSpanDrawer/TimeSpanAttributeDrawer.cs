#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(TimeSpanAttribute), true)]
    public class TimeSpanAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement r = MakeElement(property, GetPreferredLabel(property), allAttributes.Any(each => each is DefaultExpandAttribute));
            r.AddToClassList(TimeSpanField.alignedFieldUssClassName);
            return r;
        }

        public static VisualElement RenderSerializedActual(ISaintsAttribute timeSpanAttribute, string label, SerializedProperty property, IReadOnlyList<Attribute> allAttributes, bool inHorizontal)
        {
            VisualElement r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue)), label, allAttributes.Any(each => each is DefaultExpandAttribute));
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(TimeSpanField.alignedFieldUssClassName);
            }

            return r;
        }

        private static VisualElement MakeElement(SerializedProperty property, string label, bool defaultExpanded)
        {
            TimeSpanElement timeSpanElement = new TimeSpanElement(defaultExpanded);
            timeSpanElement.BindPath(property.propertyPath);
            timeSpanElement.Bind(property.serializedObject);

            TimeSpanField element = new TimeSpanField(label, timeSpanElement);

            element.AddToClassList(ClassAllowDisable);

            return element;
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            bool isLong = valueType == typeof(long) || value is long;

            long ticks = isLong? (long) value: ((TimeSpan)value).Ticks;
            if (oldElement is TimeSpanField dtField)
            {
                // Debug.Log($"old element set ticks {ticks}");
                dtField.SetValueWithoutNotify(ticks);
                return null;
            }

            TimeSpanElement timeSpanElement = new TimeSpanElement
            {
                value = ticks,
            };

            TimeSpanField element = new TimeSpanField(label, timeSpanElement);

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
                element.AddToClassList(TimeSpanField.alignedFieldUssClassName);
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
                        invokeValue = new TimeSpan(evt.newValue);
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
