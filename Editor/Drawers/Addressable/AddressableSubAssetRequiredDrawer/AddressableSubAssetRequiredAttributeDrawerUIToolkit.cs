using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSubAssetRequiredDrawer
{
    public partial class AddressableSubAssetRequiredAttributeDrawer
    {
        private static string NameRequiredBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__AddressableSubAssetRequired";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            (string error, string _) = ValidateProperty(property);

            HelpBoxMessageType helpBoxMessageType = ((AddressableSubAssetRequiredAttribute) saintsAttribute).MessageType.GetUIToolkitMessageType();

            // Debug.Log(typeError);
            HelpBox helpBox = new HelpBox(error, string.IsNullOrEmpty(error)? helpBoxMessageType: HelpBoxMessageType.Error)
            {
                style =
                {
                    display = error == ""? DisplayStyle.None : DisplayStyle.Flex,
                },
                name = NameRequiredBox(property, index),
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(MSubObjectName);
            if (prop == null)
            {
                Debug.LogWarning($"Property {MSubObjectName} not found in {property.propertyPath}");
                return;
            }

            AddressableSubAssetRequiredAttribute assetRequiredAttribute = (AddressableSubAssetRequiredAttribute)saintsAttribute;

            HelpBoxMessageType helpBoxMessageType = assetRequiredAttribute.MessageType.GetUIToolkitMessageType();
            string customMessage = assetRequiredAttribute.ErrorMessage;

            HelpBox helpBox = container.Q<HelpBox>(NameRequiredBox(property, index));

            helpBox.TrackPropertyValue(prop, _ => ValidateDisplay(property, customMessage, helpBox, helpBoxMessageType));
            ValidateDisplay(property, customMessage, helpBox, helpBoxMessageType);
        }

        private static void ValidateDisplay(SerializedProperty property, string customMessage, HelpBox helpBox, HelpBoxMessageType helpBoxMessageType)
        {
            string message;
            HelpBoxMessageType messageType = helpBoxMessageType;

            (string error, string validateMessage) = ValidateProperty(property);
            if (string.IsNullOrEmpty(error))
            {
                if (string.IsNullOrEmpty(validateMessage))
                {
                    message = "";
                }
                else
                {
                    message = string.IsNullOrEmpty(customMessage)
                        ? $"{property.displayName} is required"
                        : customMessage;
                }
            }
            else
            {
                message = error;
                messageType = HelpBoxMessageType.Error;
            }

            if (helpBox.text != message)
            {
                helpBox.text = message;
            }

            if (helpBox.messageType != messageType)
            {
                helpBox.messageType = messageType;
            }

            DisplayStyle display = message == "" ? DisplayStyle.None : DisplayStyle.Flex;
            if (helpBox.style.display != display)
            {
                helpBox.style.display = display;
            }
        }

    }
}
