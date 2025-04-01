#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public partial class InputAxisAttributeDrawer
    {

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__InputAxis";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            IReadOnlyList<string> axisNames = GetAxisNames();
            int selectedIndex = IndexOf(axisNames, property.stringValue);
            string buttonLabel = selectedIndex == -1 ? "-" : axisNames[selectedIndex];

            UIToolkitUtils.DropdownButtonField dropdownButton =
                UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));

            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButtonField(property);
            dropdownButton.ButtonLabelElement.text = buttonLabel;

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButtonField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));

            UIToolkitUtils.AddContextualMenuManipulator(dropdownButtonField.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            dropdownButtonField.ButtonElement.clicked += () =>
                ShowDropdown(property, container, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property,
            VisualElement container, Action<object> onChange)
        {
            IReadOnlyList<string> axisNames = GetAxisNames();

            // Button button = container.Q<Button>(NameButtonField(property));
            UIToolkitUtils.DropdownButtonField buttonLabel =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));
            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

            int selectedIndex = IndexOf(axisNames, property.stringValue);

            // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
            for (int index = 0; index < axisNames.Count; index++)
            {
                int curIndex = index;

                genericDropdownMenu.AddItem(axisNames[index], index == selectedIndex, () =>
                {
                    property.stringValue = axisNames[curIndex];
                    onChange(axisNames[curIndex]);
                    buttonLabel.ButtonLabelElement.text = axisNames[curIndex];
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            if (axisNames.Count > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }

            genericDropdownMenu.AddItem("Open Input Manager...", false, OpenInputManager);

            genericDropdownMenu.DropDown(buttonLabel.ButtonElement.worldBound, buttonLabel, true);
        }

    }
}
#endif
