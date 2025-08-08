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

        private static string NameResiable(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";
        // private static string NameTextArea(SerializedProperty property) => $"{property.propertyPath}__ResizableTextArea";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            const float singleLineHeight = 47 / 3f;

            float minHeight = singleLineHeight * SaintsFieldConfigUtil.ResizableTextAreaMinRow();

            TextField textField = new TextField
            {
                value = property.stringValue,
                // name = NameTextArea(property),
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    minHeight = minHeight,
                },
            };

            textField.AddToClassList(ClassAllowDisable);

            // TextInput inputField = textField.Q<TextInput>();
            VisualElement textInput = textField.Q(name: "unity-text-input");
            if (textInput != null)
            {
                textInput.style.minHeight = minHeight;
            }

            textField.BindProperty(property);

            ResizableTextArea r = new ResizableTextArea(GetPreferredLabel(property), textField)
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                },
                name = NameResiable(property),
            };

            UIToolkitUtils.AddContextualMenuManipulator(r.labelElement, property, () => Util.PropertyChangedCallback(property, info, null));
            r.BindProperty(property);

            return r;

            // textField.style.minHeight = singleLineHeight * SaintsFieldConfigUtil.ResizableTextAreaMinRow();

            // return r;
        }

        // protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
        //     Action<object> onValueChangedCallback, FieldInfo info, object parent)
        // {
        //     ResizableTextArea resizableTextArea = container.Q<ResizableTextArea>(name: NameResiable(property));
        //     resizableTextArea.TextField.RegisterValueChangedCallback(changed =>
        //     {
        //         property.stringValue = changed.newValue;
        //         property.serializedObject.ApplyModifiedProperties();
        //
        //         onValueChangedCallback?.Invoke(changed.newValue);
        //     });
        //
        //     resizableTextArea.TextField.TrackPropertyValue(property, newProp =>
        //     {
        //         if (resizableTextArea.value != newProp.stringValue)
        //         {
        //             resizableTextArea.TextField.SetValueWithoutNotify(newProp.stringValue);
        //         }
        //     });
        //
        //     UIToolkitUtils.AddContextualMenuManipulator(resizableTextArea.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
        //
        //     resizableTextArea.labelElement.AddManipulator(new ContextualMenuManipulator(evt =>
        //     {
        //         evt.menu.AppendAction("Clear", _ =>
        //         {
        //             property.stringValue = string.Empty;
        //             property.serializedObject.ApplyModifiedProperties();
        //             onValueChangedCallback.Invoke(property.stringValue);
        //         });
        //     }));
        // }

    }
}
#endif
