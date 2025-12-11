#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {
        private class ResizableTextArea: BaseField<string>
        {
            public readonly TextField TextField;

            public ResizableTextArea(string label, TextField visualInput) : base(label, visualInput)
            {
                TextField = visualInput;
            }
        }

        // private static string NameLabelPlaceholder(SerializedProperty property) =>
        //     $"{property.propertyPath}__ResizableTextArea_LabelPlaceholder";

        private static string NameResizable(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";
        // private static string NameTextArea(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";

        private static float MinHeight => 47 / 3f * SaintsFieldConfigUtil.ResizableTextAreaMinRow();

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            ResizableTextArea r = MakeResizableTextArea(GetPreferredLabel(property));
            r.TextField.bindingPath = property.propertyPath;
            r.name = NameResizable(property);

            UIToolkitUtils.AddContextualMenuManipulator(r.labelElement, property, () => Util.PropertyChangedCallback(property, info, null));

            return r;
        }

        private static ResizableTextArea MakeResizableTextArea(string label)
        {
            TextField textField = new TextField
            {
                // bindingPath = property.propertyPath,
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    minHeight = MinHeight,
                    marginRight = 0,
                },
            };

            textField.AddToClassList(ClassAllowDisable);

            // TextInput inputField = textField.Q<TextInput>();
            VisualElement textInput = textField.Q(name: "unity-text-input");
            if (textInput != null)
            {
                textInput.style.minHeight = MinHeight;
            }

            // textField.BindProperty(property);

            ResizableTextArea r = new ResizableTextArea(label, textField)
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                },
                // name = NameResizable(property),
            };
            return r;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            ResizableTextArea resizableTextArea = container.Q<ResizableTextArea>(name: NameResizable(property));
            resizableTextArea.TrackPropertyValue(property, _ => onValueChangedCallback.Invoke(property.stringValue));
        }
    }
}
#endif
