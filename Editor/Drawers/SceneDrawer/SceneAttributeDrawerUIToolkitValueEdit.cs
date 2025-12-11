using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public partial class SceneAttributeDrawer
    {
        private class UIToolkitValueEditStringElement: VisualElement
        {
            public readonly ScenePickerStringField ScenePickerStringField;
            public UIToolkitValueEditStringElement(ScenePickerStringField scenePickerStringField)
            {
                ScenePickerStringField = scenePickerStringField;
                Add(scenePickerStringField);
            }
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, SceneAttribute sceneAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is UIToolkitValueEditStringElement old)
            {
                // Debug.Log($"set value = {value}");
                old.ScenePickerStringField.SetValueWithoutNotify(value);
                return null;
            }

            ScenePickerStringField r = new ScenePickerStringField(label,
                new ScenePickerStringElement(sceneAttribute))
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(r, setterOrNull, labelGrayColor, inHorizontalLayout);
            if(setterOrNull != null)
            {
                r.RegisterValueChangedCallback(v => setterOrNull.Invoke(v.newValue));
            }

            UIToolkitValueEditStringElement container = new UIToolkitValueEditStringElement(r);

            return container;
        }

        private class UIToolkitValueEditIntElement: VisualElement
        {
            public readonly ScenePickerIntField ScenePickerIntField;
            public UIToolkitValueEditIntElement(ScenePickerIntField scenePickerIntField)
            {
                ScenePickerIntField = scenePickerIntField;
                Add(scenePickerIntField);
            }
        }

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, SceneAttribute sceneAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is UIToolkitValueEditIntElement old)
            {
                old.ScenePickerIntField.SetValueWithoutNotify(value);
                return null;
            }

            ScenePickerIntField r = new ScenePickerIntField(label,
                new ScenePickerIntElement(sceneAttribute))
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(r, setterOrNull, labelGrayColor, inHorizontalLayout);
            if(setterOrNull != null)
            {
                r.RegisterValueChangedCallback(v => setterOrNull.Invoke(v.newValue));
            }

            UIToolkitValueEditIntElement container = new UIToolkitValueEditIntElement(r);

            return container;
        }
    }
}
