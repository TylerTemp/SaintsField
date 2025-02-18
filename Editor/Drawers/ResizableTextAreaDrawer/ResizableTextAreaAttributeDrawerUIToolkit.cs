#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {
        private static string NameLabelPlaceholder(SerializedProperty property) =>
            $"{property.propertyPath}__ResizableTextArea_LabelPlaceholder";

        private static string NameTextArea(SerializedProperty property) =>
            $"{property.propertyPath}__ResizableTextArea";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement();
            root.Add(new Label(property.displayName)
            {
                name = NameLabelPlaceholder(property),
                style =
                {
                    height = SingleLineHeight,
                    paddingLeft = 4,
                },
                pickingMode = PickingMode.Ignore,
            });

            const float singleLineHeight = 47 / 3f;

            root.Add(new TextField
            {
                value = property.stringValue,
                name = NameTextArea(property),
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    minHeight = singleLineHeight * SaintsFieldConfigUtil.ResizableTextAreaMinRow(),
                },
            });

            root.AddToClassList(ClassAllowDisable);

            return root;
            // return new TextField(property.displayName)
            // {
            //     value = property.stringValue,
            //     name = NameTextArea(property),
            //     multiline = true,
            //     style =
            //     {
            //         whiteSpace = WhiteSpace.Normal,
            //         minHeight = 47,
            //     },
            // };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            TextField textArea = container.Q<TextField>(name: NameTextArea(property));
            textArea.RegisterValueChangedCallback(changed =>
            {
                property.stringValue = changed.newValue;
                property.serializedObject.ApplyModifiedProperties();

                onValueChangedCallback?.Invoke(changed.newValue);
            });

            textArea.TrackPropertyValue(property, newProp =>
            {
                if (textArea.value != newProp.stringValue)
                {
                    textArea.SetValueWithoutNotify(newProp.stringValue);
                }
            });
        }

    }
}
#endif
