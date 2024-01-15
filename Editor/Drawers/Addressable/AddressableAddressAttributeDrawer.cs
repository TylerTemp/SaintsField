#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable
{
    [CustomPropertyDrawer(typeof(AddressableAddressAttribute))]
    public class AddressableAddressAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private string _error = "";

        private IReadOnlyList<string> _targetKeys;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;

            (string error, IReadOnlyList<string> keys) = SetupAssetGroup(addressableAddressAttribute);

            _error = error;
            _targetKeys = keys;
            if (_error != "")
            {
                DefaultDrawer(position, property, label);
                return;
            }

            int index = Util.ListIndexOfAction(keys, each => each == property.stringValue);

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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, IReadOnlyList<string> _) = SetupAssetGroup(addressableAddressAttribute);
            _error = error;
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, IReadOnlyList<string> _) = SetupAssetGroup(addressableAddressAttribute);
            _error = error;

            return _error == "" ? 0 : Utils.ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : Utils.ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        private static (string error, IReadOnlyList<string> assetGroups) SetupAssetGroup(AddressableAddressAttribute addressableAddressAttribute)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if(settings == null)
            {
                return ("Addressable has not settings created yet", Array.Empty<string>());
            }

            // AddressableAssetGroup[] targetGroups;
            IReadOnlyList<AddressableAssetGroup> assetGroups = string.IsNullOrEmpty(addressableAddressAttribute.Group)
                ? settings.groups.ToArray()
                : settings.groups.Where(each => each.name == addressableAddressAttribute.Group).ToArray();

            string[][] labelFilters = addressableAddressAttribute.LabelFilters;

            IEnumerable<AddressableAssetEntry> entries = assetGroups.SelectMany(each => each.entries);

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
        #endregion

        #region UIToolkit

        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AddressableAddress_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AddressableAddress_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container1, Label fakeLabel, object parent)
        {
            return new DropdownField(new string(' ', property.displayName.Length))
            {
                // style =
                // {
                //     flexGrow = 1,
                // },
                userData = Array.Empty<string>(),
                name = NameDropdownField(property),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
            dropdownField.RegisterValueChangedCallback(v =>
            {
                IReadOnlyList<string> curMetaInfo = (IReadOnlyList<string>) ((DropdownField) v.target).userData;
                string selectedKey = curMetaInfo[dropdownField.index];
                property.stringValue = selectedKey;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(selectedKey);
            });
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            (string error, IReadOnlyList<string> keys) = SetupAssetGroup((AddressableAddressAttribute) saintsAttribute);
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));

            IReadOnlyList<string> curKeys = (IReadOnlyList<string>) dropdownField.userData;

            if(!curKeys.SequenceEqual(keys))
            {
                dropdownField.userData = keys;
                dropdownField.choices = keys.Select(each => each.Replace('/', '\u2215').Replace('&', '＆')).ToList();
                // int curSelect = Util.ListIndexOfAction(keys, each => each == property.stringValue);
                // dropdownField.index = curSelect;
                dropdownField.SetValueWithoutNotify(property.stringValue);
            }

            // Debug.Log($"AnimatorStateAttributeDrawer: {newAnimatorStates}");
            HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
            // ReSharper disable once InvertIf
            if (error != helpBoxElement.text)
            {
                helpBoxElement.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBoxElement.text = error;
            }
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull)
        {
            DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
            dropdownField.label = labelOrNull;
        }
        #endregion
    }
}
#endif
