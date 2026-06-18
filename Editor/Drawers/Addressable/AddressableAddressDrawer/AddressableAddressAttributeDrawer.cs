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

namespace SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableAddressAttribute), true)]
    public partial class AddressableAddressAttributeDrawer: SaintsPropertyDrawer
    {
        internal readonly struct AddressableAddressDropdownInfo
        {
            public readonly bool IsString;
            public readonly AddressableAssetSettings Settings;
            public readonly IReadOnlyList<string> Addresses;
            public readonly AdvancedDropdownMetaInfo MetaInfo;

            public AddressableAddressDropdownInfo(bool isString, AddressableAssetSettings settings,
                IReadOnlyList<string> addresses, AdvancedDropdownMetaInfo metaInfo)
            {
                IsString = isString;
                Settings = settings;
                Addresses = addresses;
                MetaInfo = metaInfo;
            }
        }

        internal const string ErrorAddressableSettingsNotCreated = "Addressable Settings not created.";
        private const string CreateSettingsLabel = "Create Addressable Settings...";
        private const string EditAddressesLabel = "Edit Addresses...";

        internal static (string error, AddressableAddressDropdownInfo dropdownInfo) GetAddressableAddressDropdownInfo(
            SerializedProperty property, AddressableAddressAttribute addressableAddressAttribute, bool isImGui)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return ($"Type {property.propertyType} is not a string type",
                    new AddressableAddressDropdownInfo(false, null, Array.Empty<string>(), default));
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            Dropdown<string> dropdown = new Dropdown<string>(isImGui ? "Pick Addressable Address" : "");
            string selected = null;
            string error = "";
            string[] addresses = Array.Empty<string>();

            if (settings == null)
            {
                error = ErrorAddressableSettingsNotCreated;
                dropdown.Add(CreateSettingsLabel, null);
            }
            else
            {
                (string entriesError, IEnumerable<AddressableAssetEntry> entries) =
                    AddressableUtil.GetAllEntries(addressableAddressAttribute.Group,
                        addressableAddressAttribute.LabelFilters);
                error = entriesError;

                if (entriesError == "")
                {
                    addresses = entries.Select(each => each.address).ToArray();

                    foreach (string address in addresses)
                    {
                        dropdown.Add(new Dropdown<string>(address, address));
                        if (property.stringValue == address)
                        {
                            selected = address;
                        }
                    }

                    if (addresses.Length > 0)
                    {
                        dropdown.AddSeparator();
                    }

                    dropdown.Add(EditAddressesLabel, null, false, "d_editicon.sml");
                }
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

            return (error, new AddressableAddressDropdownInfo(true, settings, addresses, metaInfo));
        }

        internal static string GetAddressableAddressDisplay(string value, IReadOnlyCollection<string> addresses,
            bool richText)
        {
            if (addresses != null && addresses.Contains(value))
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

        internal static bool ApplyAddressableAddressSelection(SerializedProperty property, FieldInfo info,
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
                    AddressableUtil.OpenGroupEditor();
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
