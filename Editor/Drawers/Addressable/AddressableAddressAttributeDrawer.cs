#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System.Collections.Generic;
using System.Linq;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable
{
    [CustomPropertyDrawer(typeof(AddressableAddressAttribute))]
    public class AddressableAddressAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private IReadOnlyList<AddressableAssetGroup> _targetGroups;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            string group = addressableAddressAttribute.Group;

            SetupAssetGroup(group);

            if (_error != "")
            {
                DefaultDrawer(position, property, label);
                return;
            }

            string[][] labelFilters = addressableAddressAttribute.LabelFilters;

            IEnumerable<AddressableAssetEntry> entries = _targetGroups
                .SelectMany(each => each.entries);

            if (labelFilters != null && labelFilters.Length > 0)
            {
                entries = entries.Where(eachName =>
                {
                    HashSet<string> labels = eachName.labels;
                    return labelFilters.Any(eachOr => eachOr.All(eachAnd => labels.Contains(eachAnd)));
                    // Debug.Log($"{eachName.address} {match}: {string.Join(",", labels)}");
                });
            }

            List<string> keys = entries
                .Select(each => each.address)
                .ToList();

            int index = keys.IndexOf(property.stringValue);

            using(new EditorGUI.PropertyScope(position, label, property))
            {
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUI.Popup(position, label, index, keys.Select(each => new GUIContent(each.Replace('/', '\u2215').Replace('&', '＆'))).ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = keys[newIndex];
                }
            }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            SetupAssetGroup(addressableAddressAttribute.Group);
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            SetupAssetGroup(addressableAddressAttribute.Group);

            return _error == "" ? 0 : Utils.HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : Utils.HelpBox.Draw(position, _error, MessageType.Error);

        private void SetupAssetGroup(string group)
        {
            _error = "";
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if(settings == null)
            {
                _error = "Addressable has not settings created yet";
                return;
            }

            // AddressableAssetGroup[] targetGroups;
            if (string.IsNullOrEmpty(group))
            {
                _targetGroups = settings.groups.ToArray();
            }
            else
            {
                _targetGroups = settings.groups.Where(each => each.name == group).ToArray();
                if(_targetGroups.Count == 0)
                {
                    _error = $"Addressable group {group} not found";
                    // return;
                }
            }
        }
    }
}
#endif
