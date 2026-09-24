using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer.EditUnityEvent
{
    public static class UIToolkitEditUnityEvent
    {
        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, UnityEventBase value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is UnityEventRuntimeElement target)
            {
                target.SetValueWithoutNotify(value);
                return null;
            }

            UnityEventRuntimeElement field = new UnityEventRuntimeElement(label)
            {
                // value = value,
            };
            field.SetValueWithoutNotify(value);

            return field;
        }
    }
}
