#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Addressable;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;

namespace SaintsField.Editor.Drawers.Addressable
{
    public static class AddressableUtil
    {
        public const string ErrorNoSettings = "Addressable has no settings created yet.";

        public static void OpenGroupEditor() => EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
        public static void OpenLabelEditor()
        {
            LabelWindow window = EditorWindow.GetWindow<LabelWindow>();
            window.Intialize(AddressableAssetSettingsDefaultObject.GetSettings(false));
            window.Show();
        }

        public static (string error, IEnumerable<AddressableAssetEntry> assetGroups) GetAllEntries(string group, IReadOnlyList<IReadOnlyList<string>> labelFilters)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (!settings)
            {
                return (ErrorNoSettings, Array.Empty<AddressableAssetEntry>());
            }

            // AddressableAssetGroup[] targetGroups;
            IReadOnlyList<AddressableAssetGroup> assetGroups = string.IsNullOrEmpty(group)
                ? settings.groups.ToArray()
                : settings.groups.Where(each => each.name == group).ToArray();

            // string[][] labelFilters = addressableAddressAttribute.LabelFilters;

            IEnumerable<AddressableAssetEntry> entries = assetGroups.SelectMany(each => each.entries);

            // ReSharper disable once MergeIntoPattern
            if (labelFilters != null && labelFilters.Count > 0)
            {
                entries = entries.Where(eachName =>
                {
                    HashSet<string> labels = eachName.labels;
                    return labelFilters.Any(eachOr => eachOr.All(eachAnd => labels.Contains(eachAnd)));
                    // Debug.Log($"{eachName.address} {match}: {string.Join(",", labels)}");
                });
            }

            // IReadOnlyList<string> keys = entries
            //     .Select(each => each.address)
            //     .ToList();

            return ("", entries);
        }
    }
}
#endif
