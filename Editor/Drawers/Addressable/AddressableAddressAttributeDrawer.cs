#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.Addressable
{
    [CustomPropertyDrawer(typeof(AddressableAddressAttribute))]
    public class AddressableAddressAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private string _error = "";

        // private IReadOnlyList<string> _targetKeys;

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
                .Concat(new []
                {
                    GUIContent.none,
                    new GUIContent("Edit Addressable Group..."),
                })
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if(newIndex < keys.Count)
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
            GUIContent label, ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        private static string ErrorNoSettings => "Addressable has no settings created yet.";

        private static (string error, IReadOnlyList<string> assetGroups) SetupAssetGroup(AddressableAddressAttribute addressableAddressAttribute)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if(settings == null)
            {
                return (ErrorNoSettings, Array.Empty<string>());
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
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AddressableAddress_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AddressableAddress_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container1, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButtonField = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
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
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));

            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            dropdownField.ButtonElement.clicked += () => ShowDropdown(property, addressableAddressAttribute, dropdownField, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property, AddressableAddressAttribute addressableAddressAttribute, UIToolkitUtils.DropdownButtonField dropdownField, Action<object> onValueChangedCallback)
        {
            (string _, IReadOnlyList<string> keys) = SetupAssetGroup(addressableAddressAttribute);

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            foreach (string key in keys)
            {
                string thisKey = key;
                genericDropdownMenu.AddItem(key, property.stringValue == thisKey, () =>
                {
                    // dropdownField.buttonLabelElement.text = thisKey;
                    dropdownField.ButtonLabelElement.text = thisKey;
                    property.stringValue = thisKey;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(thisKey);
                });
            }

            if (keys.Count > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }

            genericDropdownMenu.AddItem("Edit Addressable Group...", false, () =>
            {
                EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            });

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
                UpdateHelpBox(container.Q<HelpBox>(NameHelpBox(property)), ErrorNoSettings);
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

        #endregion

#endif
    }
}
#endif
