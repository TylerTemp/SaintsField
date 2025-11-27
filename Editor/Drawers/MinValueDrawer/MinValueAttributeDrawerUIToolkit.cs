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

namespace SaintsField.Editor.Drawers.MinValueDrawer
{
    public partial class MinValueAttributeDrawer
    {
        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__MinValue_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;

            TrackValue(property, minValueAttribute, helpBox, onValueChangedCallback, info, parent);
            helpBox.TrackPropertyValue(property, _ => TrackValue(property, minValueAttribute, helpBox, onValueChangedCallback, info, parent));
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ => UIToolkitUtils.Unbind(helpBox));
        }

        private static void TrackValue(SerializedProperty property, MinValueAttribute minValueAttribute,
            HelpBox helpBox, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (string error, float valueLimit) = GetLimitFloat(property, minValueAttribute, info, parent);

            UIToolkitUtils.SetHelpBox(helpBox, error);

            if (error != "")
            {
                return;
            }

            if (property.propertyType == SerializedPropertyType.Float && property.floatValue < valueLimit)
            {
                property.floatValue = valueLimit;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(valueLimit);
            }
            else if (property.propertyType == SerializedPropertyType.Integer && property.intValue < (int)valueLimit)
            {
                property.intValue = (int)valueLimit;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke((int)valueLimit);
            }
        }

    }
}
#endif
