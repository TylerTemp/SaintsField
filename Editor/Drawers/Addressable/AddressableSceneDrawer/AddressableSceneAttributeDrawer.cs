using System;
using System.Collections.Generic;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSceneDrawer
{
    [CustomPropertyDrawer(typeof(AddressableSceneAttribute))]
    public partial class AddressableSceneAttributeDrawer: SaintsPropertyDrawer
    {
        private static AdvancedDropdownMetaInfo GetMetaInfo(string key, IEnumerable<AddressableAssetEntry> assetEntries, bool sepAsSub)
        {
            AdvancedDropdownList<AddressableAssetEntry>
                advancedDropdownList = new AdvancedDropdownList<AddressableAssetEntry>
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
    }
}
