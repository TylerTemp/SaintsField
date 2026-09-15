#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {
        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, ResizableTextAreaAttribute resizableTextAreaAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (oldElement is ResizableTextField resizableTextArea)
            {
                resizableTextArea.SetValueWithoutNotify(value);
                return null;
            }

            ResizableTextField field = new ResizableTextField(label, GetMinHeight(resizableTextAreaAttribute))
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout || !resizableTextAreaAttribute.Inline);

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
