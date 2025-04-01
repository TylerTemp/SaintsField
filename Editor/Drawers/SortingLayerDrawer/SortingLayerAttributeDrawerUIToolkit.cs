#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public partial class SortingLayerAttributeDrawer
    {

        private static string NameButtonField(SerializedProperty property) =>
            $"{property.propertyPath}__SortingLayer_Button";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__SortingLayer_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton =
                UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButtonField(property);

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
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
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField buttonLabel =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));

            UIToolkitUtils.AddContextualMenuManipulator(buttonLabel.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            (string[] _, int _, string displayName) = GetSelected(property);
            buttonLabel.ButtonLabelElement.text = displayName;

            container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property)).ButtonElement.clicked += () =>
                ShowDropdown(property, container, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property,
            VisualElement container, Action<object> onChange)
        {
            (string[] layers, int selectedIndex, string _) = GetSelected(property);

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            // if (layers.Length == 0)
            // {
            //     genericDropdownMenu.AddDisabledItem("No layers", false);
            //     genericDropdownMenu.DropDown(button.worldBound, button, true);
            //     return;
            // }

            UIToolkitUtils.DropdownButtonField buttonLabel =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));

            for (int index = 0; index < layers.Length; index++)
            {
                int curIndex = index;
                string curItem = layers[index];

                string curName = $"{index}: {curItem}";

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {
                    if (property.propertyType == SerializedPropertyType.String)
                    {
                        property.stringValue = curItem;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curItem);
                    }
                    else
                    {
                        property.intValue = curIndex;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curIndex);
                    }

                    buttonLabel.ButtonLabelElement.text = curName;
                });
            }

            if (layers.Length > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }

            genericDropdownMenu.AddItem("Edit Sorting Layers...", false, OpenSortingLayerInspector);

            genericDropdownMenu.DropDown(buttonLabel.ButtonElement.worldBound, buttonLabel, true);
        }

        private static (string[] layers, int index, string display) GetSelected(SerializedProperty property)
        {
            string[] layers = GetLayers();
            if (property.propertyType == SerializedPropertyType.String)
            {
                string value = property.stringValue;
                int index = Array.IndexOf(layers, value);
                return (layers, index, index == -1 ? $"?: {value}" : $"{index}: {value}");
            }
            else
            {
                int index = property.intValue;
                if (index < 0 || index >= layers.Length)
                {
                    return (layers, -1, $"{index}: ?");
                }

                return (layers, index, $"{index}: {layers[index]}");
            }
        }

    }
}
#endif
