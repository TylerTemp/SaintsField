using System;
using System.Collections.Generic;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {
        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, ResizableTextAreaAttribute resizableTextAreaAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (oldElement is ResizableTextArea resizableTextArea)
            {
                resizableTextArea.TextField.SetValueWithoutNotify(value);
                return null;
            }

            ResizableTextArea field = MakeResizableTextArea(label);
            field.TextField.value = value;

            if (labelGrayColor)
            {
                field.labelElement.style.color = AbsRenderer.ReColor;
            }
            if (setterOrNull == null)
            {
                field.SetEnabled(false);
                field.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
            }
            else
            {
                field.TextField.RegisterValueChangedCallback(evt => setterOrNull(evt.newValue));
                field.AddToClassList(ClassAllowDisable);
            }

            return field;
        }
    }
}
