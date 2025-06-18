using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
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
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;

            (string error, IEnumerable<AddressableAssetEntry> entries) = AddressableUtil.GetAllEntries(addressableAddressAttribute.Group, addressableAddressAttribute.LabelFilters);

            _error = error;
            // _targetKeys = keys;
            if (_error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            string[] keys = entries.Select(each => each.address).ToArray();

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
                    if (newIndex < keys.Length)
                    {
                        property.stringValue = keys[newIndex];
                    }
                    else
                    {
                        AddressableUtil.OpenGroupEditor();
                    }
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, IEnumerable<AddressableAssetEntry> _) = AddressableUtil.GetAllEntries(addressableAddressAttribute.Group, addressableAddressAttribute.LabelFilters);
            _error = error;
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, IEnumerable<AddressableAssetEntry> _) = AddressableUtil.GetAllEntries(addressableAddressAttribute.Group, addressableAddressAttribute.LabelFilters);
            _error = error;

            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}
