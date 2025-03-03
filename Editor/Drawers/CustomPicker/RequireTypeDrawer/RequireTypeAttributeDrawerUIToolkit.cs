#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer
{
    public partial class RequireTypeAttributeDrawer
    {
        #region UIToolkit
        protected static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__RequireType_HelpBox";
        protected static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__RequireType_SelectorButton";

        protected class Payload
        {
            public bool HasCorrectValue;
            public UnityEngine.Object CorrectValue;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            bool customPicker = requireTypeAttribute.CustomPicker;

            if (!customPicker)
            {
                return null;
            }

            Button button = new Button
            {
                text = "●",
                style =
                {
                    // position = Position.Absolute,
                    // right = 0,
                    width = 18,
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NameSelectorButton(property),
            };

            button.AddToClassList(ClassAllowDisable);
            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                userData = new Payload
                {
                    HasCorrectValue = false,
                    CorrectValue = null,
                },
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            if(requireTypeAttribute.CustomPicker)
            {
                container.Q<Button>(NameSelectorButton(property)).clicked += () =>
                {
                    OpenSelectorWindow(property, requireTypeAttribute, info, onValueChangedCallback, parent);
                };
            }

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;
            UnityEngine.Object curValue = property.objectReferenceValue;
            IReadOnlyList<string> missingTypeNames = curValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(curValue, requiredTypes);
            if (missingTypeNames.Count > 0)
            {
                helpBox.text = $"{curValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}";
                helpBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                payload.HasCorrectValue = true;
                payload.CorrectValue = curValue;
            }


        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UnityEngine.Object newObjectValue = (UnityEngine.Object)newValue;
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;

            IReadOnlyList<string> missingTypeNames = newObjectValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(newObjectValue, requiredTypes);

            if (missingTypeNames.Count == 0)
            {
                helpBox.style.display = DisplayStyle.None;
                payload.HasCorrectValue = true;
                payload.CorrectValue = newObjectValue;
            }
            else
            {
                string errorMessage = $"{newObjectValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}.";
                if(requireTypeAttribute.FreeSign || !payload.HasCorrectValue)
                {
                    helpBox.text = errorMessage;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                else
                {
                    Debug.Assert(!requireTypeAttribute.FreeSign && payload.HasCorrectValue,
                          "Code should not be here. This is a BUG.");
                    property.objectReferenceValue = payload.CorrectValue;
                    property.serializedObject.ApplyModifiedProperties();
                    Debug.LogWarning($"{errorMessage} Change reverted to {(payload.CorrectValue == null ? "null" : payload.CorrectValue.ToString())}.");
                    // careful for infinite loop!
                    onValueChangedCallback(payload.CorrectValue);
                }
            }

        }

        #endregion
    }
}
#endif
