using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSceneDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableSceneAttribute), true)]
    public partial class AddressableSceneAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static (string error, AddressableAssetEntry sceneEntry) GetSceneEntry(string value, AddressableSceneAttribute addressableSceneAttribute)
        {
            if (!AddressableAssetSettingsDefaultObject.GetSettings(false))
            {
                return (AddressableUtil.ErrorNoSettings, null);
            }

            if (string.IsNullOrEmpty(value))
            {
                return ("", null);
            }

            (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
            if (error != "")
            {
                return (error, null);
            }

            AddressableAssetEntry assetEntry = assetGroups
                .Where(each => each.MainAsset is SceneAsset)
                .FirstOrDefault(each => each.address == value);

            return assetEntry == null
                ? ($"Scene `{value}` is not in target Addressable group", null)
                : ("", assetEntry);
        }

        private static (string error, AddressableAssetEntry sceneEntry) GetSceneEntryFromSceneAsset(UnityEngine.Object newObj, AddressableSceneAttribute addressableSceneAttribute)
        {
            if (!AddressableAssetSettingsDefaultObject.GetSettings(false))
            {
                return (AddressableUtil.ErrorNoSettings, null);
            }
            SceneAsset sceneAsset = (SceneAsset)newObj;
            if (sceneAsset == null)
            {
                return ("", null);
            }

            (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
            if (error != "")
            {
                return (error, null);
            }

            AddressableAssetEntry assetEntry = assetGroups.FirstOrDefault(each => ReferenceEquals(each.MainAsset, sceneAsset));
            return assetEntry == null ? ($"Scene `{sceneAsset.name}` is not in target Addressable group.", null) : ("", assetEntry);
        }

        private static bool ApplyAddressableSceneSelection(SerializedProperty property, FieldInfo info,
            object parent, string newValue, Action<string> onValueChanged)
        {
            property.stringValue = newValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
            property.serializedObject.ApplyModifiedProperties();
            onValueChanged?.Invoke(newValue);
            return true;
        }

        private enum DropdownType
        {
            Null,
            asset,
            CommandEdit,
        }

        private readonly struct DropdownItem
        {
            public readonly DropdownType Type;
            public readonly AddressableAssetEntry AddressableAssetEntry;


            public DropdownItem(DropdownType type, AddressableAssetEntry addressableAssetEntry)
            {
                Type = type;
                AddressableAssetEntry = addressableAssetEntry;
            }
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string key, IEnumerable<AddressableAssetEntry> assetEntries, bool sepAsSub, bool isImGui)
        {
            Dropdown<DropdownItem>
                dropdown = new Dropdown<DropdownItem>(isImGui? "Pick A Scene": "")
                {
                    {"[Null]", new DropdownItem(DropdownType.Null, null)},
                };

            AddressableAssetEntry curValue = null;

            foreach (AddressableAssetEntry assetEntry in assetEntries)
            {
                if (assetEntry.address == key)
                {
                    curValue = assetEntry;
                }

                if (sepAsSub)
                {
                    dropdown.Add(assetEntry.address, new DropdownItem(DropdownType.asset, assetEntry));
                }
                else
                {
                    dropdown.Add(new Dropdown<DropdownItem>(assetEntry.address,
                        new DropdownItem(DropdownType.asset, assetEntry)));
                }

            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Addresses...", new DropdownItem(DropdownType.CommandEdit, null), false, "d_editicon.sml");

            dropdown.SelfCompact();

            #region Get Cur Value

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curStack;
            if (curValue == null)
            {
                curStack = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stack, string _) = AdvancedDropdownUtil.GetSelected(curValue, Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdown);
                curStack = stack;
            }

            #endregion

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                // CurDisplay = display,
                CurValues = curValue == null? Array.Empty<AddressableAssetEntry>(): new []{curValue},
                DropdownListValue = dropdown,
                SelectStacks = curStack,
            };
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)propertyAttribute;

            (string error, AddressableAssetEntry _) = GetSceneEntry(property.stringValue, addressableSceneAttribute);
            if (error == "")
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                ExecError = "",
                Error = error,
            };
        }
    }
}
