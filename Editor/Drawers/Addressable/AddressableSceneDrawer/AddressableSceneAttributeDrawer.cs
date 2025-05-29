using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
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

            (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
            if (error != "")
            {
                return (error, null);
            }

            AddressableAssetEntry assetEntry = assetGroups.FirstOrDefault(each => ReferenceEquals(each.MainAsset, sceneAsset));
            return assetEntry == null ? ($"Scene `{sceneAsset.name}` is not in target Addressable group.", null) : ("", assetEntry);
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string key, IEnumerable<AddressableAssetEntry> assetEntries, bool sepAsSub, bool isImGui)
        {
            AdvancedDropdownList<AddressableAssetEntry>
                advancedDropdownList = new AdvancedDropdownList<AddressableAssetEntry>(isImGui? "Pick A Scene": "")
                {
                    {"[Null]", null},
                };

            advancedDropdownList.AddSeparator();

            AddressableAssetEntry curValue = null;

            foreach (AddressableAssetEntry assetEntry in assetEntries)
            {
                if (assetEntry.address == key)
                {
                    curValue = assetEntry;
                }

                if (sepAsSub)
                {
                    advancedDropdownList.Add(assetEntry.address, assetEntry);
                }
                else
                {
                    advancedDropdownList.Add(new AdvancedDropdownList<AddressableAssetEntry>(assetEntry.address, assetEntry));
                }
            }

            #region Get Cur Value

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curStack;
            if (curValue == null)
            {
                curStack = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stack, string _) = AdvancedDropdownUtil.GetSelected(curValue, Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), advancedDropdownList);
                curStack = stack;
            }

            #endregion

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                // CurDisplay = display,
                CurValues = curValue == null? Array.Empty<AddressableAssetEntry>(): new []{curValue},
                DropdownListValue = advancedDropdownList,
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
