#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
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
    public partial class TimeSpanAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            TimeSpanField r = MakeElement(property, GetPreferredLabel(property), allAttributes.Any(each => each is DefaultExpandAttribute));
            r.AddToClassList(TimeSpanField.alignedFieldUssClassName);
            return r;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<TimeSpanField>().TrackPropertyValue(property, p => onValueChangedCallback(p.longValue));
        }

        private static TimeSpanField MakeElement(SerializedProperty property, string label, bool defaultExpanded)
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
                dtField.SetValueWithoutNotify(ticks);
                return null;
            }

            TimeSpanElement timeSpanElement = new TimeSpanElement
            {
                value = ticks,
            };

            TimeSpanField element = new TimeSpanField(label, timeSpanElement);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
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
