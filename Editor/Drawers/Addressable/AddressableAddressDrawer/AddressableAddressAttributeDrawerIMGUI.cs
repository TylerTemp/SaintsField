using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer
{
    public partial class AddressableAddressAttributeDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;

            (string error, IReadOnlyList<string> keys) = SetupAssetGroup(addressableAddressAttribute);

            _error = error;
            // _targetKeys = keys;
            if (_error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            int index = Util.ListIndexOfAction(keys, each => each == property.stringValue);

            GUIContent[] contents = keys
                .Select(each => new GUIContent(each.Replace('/', '\u2215').Replace('&', '＆')))
                .Concat(new[]
                {
                    GUIContent.none,
                    new GUIContent("Edit Addressable Group..."),
                })
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newIndex < keys.Count)
                    {
                        property.stringValue = keys[newIndex];
                    }
                    else
                    {
                        EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
                    }
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, IReadOnlyList<string> _) = SetupAssetGroup(addressableAddressAttribute);
            _error = error;
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, IReadOnlyList<string> _) = SetupAssetGroup(addressableAddressAttribute);
            _error = error;

            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        private static (string error, IReadOnlyList<string> assetGroups) SetupAssetGroup(
            AddressableAddressAttribute addressableAddressAttribute)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                return (AddressableUtil.ErrorNoSettings, Array.Empty<string>());
            }

            // AddressableAssetGroup[] targetGroups;
            IReadOnlyList<AddressableAssetGroup> assetGroups = string.IsNullOrEmpty(addressableAddressAttribute.Group)
                ? settings.groups.ToArray()
                : settings.groups.Where(each => each.name == addressableAddressAttribute.Group).ToArray();

            string[][] labelFilters = addressableAddressAttribute.LabelFilters;

            IEnumerable<AddressableAssetEntry> entries = assetGroups.SelectMany(each => each.entries);

            // ReSharper disable once MergeIntoPattern
            if (labelFilters != null && labelFilters.Length > 0)
            {
                entries = entries.Where(eachName =>
                {
                    HashSet<string> labels = eachName.labels;
                    return labelFilters.Any(eachOr => eachOr.All(eachAnd => labels.Contains(eachAnd)));
                    // Debug.Log($"{eachName.address} {match}: {string.Join(",", labels)}");
                });
            }

            IReadOnlyList<string> keys = entries
                .Select(each => each.address)
                .ToList();

            return ("", keys);
        }
    }
}
