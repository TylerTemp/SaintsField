#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    public partial class AddressableLabelAttributeDrawer
    {

        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container1,
            FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButtonField = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButtonField.name = NameDropdownField(property);
            dropdownButtonField.userData = Array.Empty<string>();
            // ReSharper disable once MergeConditionalExpression
            dropdownButtonField.ButtonLabelElement.text = property.stringValue == null ? "-" : property.stringValue;

            dropdownButtonField.AddToClassList(ClassAllowDisable);
            return dropdownButtonField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameHelpBox(property),
            };
            helpBoxElement.AddToClassList(ClassAllowDisable);
            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            dropdownField.ButtonElement.clicked += () => ShowDropdown(property, dropdownField, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property, UIToolkitUtils.DropdownButtonField dropdownField, Action<object> onValueChangedCallback)
        {

            // ReSharper disable once Unity.NoNullPropagation
            List<string> labels = AddressableAssetSettingsDefaultObject.Settings?.GetLabels() ?? new List<string>();

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            foreach (string addressableLabel in labels)
            {
                string thisAddressableLabel = addressableLabel;
                genericDropdownMenu.AddItem(addressableLabel, property.stringValue == thisAddressableLabel, () =>
                {
                    dropdownField.ButtonLabelElement.text = thisAddressableLabel;
                    property.stringValue = thisAddressableLabel;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(thisAddressableLabel);
                });
            }

            if (labels.Count > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }

            genericDropdownMenu.AddItem("Edit Labels...", false, AddressableUtil.OpenLabelEditor);

            genericDropdownMenu.DropDown(dropdownField.ButtonElement.worldBound, dropdownField, true);
        }

        private static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
            {
                UpdateHelpBox(container.Q<HelpBox>(NameHelpBox(property)), AddressableUtil.ErrorNoSettings);
                return;
            }

            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            if (dropdownField.ButtonLabelElement.text != property.stringValue)
            {
                // ReSharper disable once MergeConditionalExpression
                dropdownField.ButtonLabelElement.text = property.stringValue == null ? "-" : property.stringValue;
            }
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            UIToolkitUtils.SetLabel(dropdownField.labelElement, richTextChunks, richTextDrawer);
        }
    }
}
#endif
