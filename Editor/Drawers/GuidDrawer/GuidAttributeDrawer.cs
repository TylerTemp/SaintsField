using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.GuidDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(GuidAttribute), true)]
    public class GuidAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement field = MakeElement(property, GetPreferredLabel(property));
            field.AddToClassList(GuidStringField.alignedFieldUssClassName);
            return field;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(container.Q<GuidStringField>(), property, () => onValueChangedCallback(property.stringValue));
        }

        private static VisualElement MakeElement(SerializedProperty property, string label)
        {
            GuidStringElement timeSpanElement = new GuidStringElement
            {
                bindingPath = property.propertyPath,
            };
            timeSpanElement.BindProp(property);
            timeSpanElement.Bind(property.serializedObject);

            GuidStringField element = new GuidStringField(label, timeSpanElement);

            element.AddToClassList(ClassAllowDisable);

            return element;
        }

        public static VisualElement RenderSerializedActual(string label, SerializedProperty property, bool inHorizontal)
        {
            VisualElement r = MakeElement(property.FindPropertyRelative(nameof(SaintsSerializedProperty.stringValue)), label);
            if (inHorizontal)
            {
                r.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                r.AddToClassList(GuidStringField.alignedFieldUssClassName);
            }

            UIToolkitUtils.AddContextualMenuManipulator(r, property, () => {});

            return r;
        }

        public static VisualElement UIToolkitValueEditGuid(VisualElement oldElement, string label, Guid value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is GuidStringField gsf)
            {
                gsf.SetValueWithoutNotify(value.ToString());
                return null;
            }

            GuidStringElement guidStringElement = new GuidStringElement
            {
                value = value.ToString(),
            };
            GuidStringField element =
                new GuidStringField(label, guidStringElement)
                {
                    value = value.ToString(),
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                guidStringElement.RegisterValueChangedCallback(evt =>
                {
                    // ReSharper disable once InvertIf
                    if (Guid.TryParse(evt.newValue, out Guid guid))
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(guid);
                    }
                });
            }
            return element;
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, GuidAttribute guidAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is GuidStringField gsf)
            {
                gsf.SetValueWithoutNotify(value);
                return null;
            }

            GuidStringElement guidStringElement = new GuidStringElement
            {
                value = value,
            };
            GuidStringField element =
                new GuidStringField(label, guidStringElement)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                guidStringElement.RegisterValueChangedCallback(evt =>
                {
                    // ReSharper disable once InvertIf
                    if (Guid.TryParse(evt.newValue, out Guid guid))
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(guid);
                    }
                });
            }
            return element;
        }
    }
}
