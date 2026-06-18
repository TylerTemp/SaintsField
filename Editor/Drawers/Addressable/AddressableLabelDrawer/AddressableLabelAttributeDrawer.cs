using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableLabelAttribute), true)]
    public partial class AddressableLabelAttributeDrawer: SaintsPropertyDrawer
    {
        internal readonly struct AddressableLabelDropdownInfo
        {
            public readonly bool IsString;
            public readonly AddressableAssetSettings Settings;
            public readonly IReadOnlyList<string> Labels;
            public readonly AdvancedDropdownMetaInfo MetaInfo;

            public AddressableLabelDropdownInfo(bool isString, AddressableAssetSettings settings,
                IReadOnlyList<string> labels, AdvancedDropdownMetaInfo metaInfo)
            {
                IsString = isString;
                Settings = settings;
                Labels = labels;
                MetaInfo = metaInfo;
            }
        }

        internal const string ErrorAddressableSettingsNotCreated = "Addressable Settings not created.";
        private const string CreateSettingsLabel = "Create Addressable Settings...";
        private const string EditLabelsLabel = "Edit Labels...";

        internal static (string error, AddressableLabelDropdownInfo dropdownInfo) GetAddressableLabelDropdownInfo(
            SerializedProperty property, bool isImGui)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return ($"Type {property.propertyType} is not a string type",
                    new AddressableLabelDropdownInfo(false, null, Array.Empty<string>(), default));
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            Dropdown<string> dropdown = new Dropdown<string>(isImGui ? "Pick Addressable Label" : "");
            string selected = null;
            string error = "";
            IReadOnlyList<string> labels = Array.Empty<string>();

            if (settings == null)
            {
                error = ErrorAddressableSettingsNotCreated;
                dropdown.Add(CreateSettingsLabel, null);
            }
            else
            {
                labels = settings.GetLabels();

                foreach (string label in labels)
                {
                    dropdown.Add(new Dropdown<string>(label, label));
                    if (property.stringValue == label)
                    {
                        selected = label;
                    }
                }

                if (labels.Count > 0)
                {
                    dropdown.AddSeparator();
                }

                dropdown.Add(EditLabelsLabel, null, false, "d_editicon.sml");
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                Error = error,
                CurValues = selected is null
                    ? Array.Empty<object>()
                    : new object[] { selected },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            return (error, new AddressableLabelDropdownInfo(true, settings, labels, metaInfo));
        }

        internal static string GetAddressableLabelDisplay(string value, IReadOnlyCollection<string> labels,
            bool richText)
        {
            if (labels != null && labels.Contains(value))
            {
                return value;
            }

            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return richText
                ? $"<color=red>?</color> ({value})"
                : $"? ({value})";
        }

        internal static bool ApplyAddressableLabelSelection(SerializedProperty property, FieldInfo info,
            object parent, AddressableAssetSettings settings, string newValue, Action<string> onValueChanged)
        {
            if (newValue is null)
            {
                if (settings == null)
                {
                    AddressableAssetSettingsDefaultObject.GetSettings(true);
                }
                else
                {
                    AddressableUtil.OpenLabelEditor();
                }

                return false;
            }

            property.stringValue = newValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
            property.serializedObject.ApplyModifiedProperties();
            onValueChanged?.Invoke(newValue);
            return true;
        }
    }
}
