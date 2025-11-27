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

namespace SaintsField.Editor.Drawers.MaxValueDrawer
{
    public partial class MaxValueAttributeDrawer
    {

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__MaxValue_HelpBox";

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
            MaxValueAttribute maxValueAttribute = (MaxValueAttribute)saintsAttribute;

            TrackValue(property, maxValueAttribute, helpBox, onValueChangedCallback, info, parent);
            helpBox.TrackPropertyValue(property, _ => TrackValue(property, maxValueAttribute, helpBox, onValueChangedCallback, info, parent));
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ => UIToolkitUtils.Unbind(helpBox));
        }

        private static void TrackValue(SerializedProperty property, MaxValueAttribute maxValueAttribute,
            HelpBox helpBox, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (string error, float valueLimit) = GetLimitFloat(property, maxValueAttribute, info, parent);

            if (helpBox.text != error)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

            if (error != "")
            {
                return;
            }

            if (property.propertyType == SerializedPropertyType.Float && property.floatValue > valueLimit)
            {
                property.floatValue = valueLimit;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(valueLimit);
            }
            else if (property.propertyType == SerializedPropertyType.Integer && property.intValue > (int)valueLimit)
            {
                property.intValue = (int)valueLimit;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke((int)valueLimit);
            }
        }

    }
}
#endif
