using System.Collections.Generic;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer
{
    public partial class AddressableAddressAttributeDrawer
    {
        private sealed class AddressableAddressStatusIMGUI
        {
            public string Error = "";
            public AddressableAddressDropdownInfo DropdownInfo;
        }

        private static readonly Dictionary<string, AddressableAddressStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, AddressableAddressStatusIMGUI>();

        private static AddressableAddressStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out AddressableAddressStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new AddressableAddressStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private static (string error, AddressableAddressDropdownInfo dropdownInfo) UpdateStatus(
            SerializedProperty property, AddressableAddressAttribute addressableAddressAttribute,
            out AddressableAddressStatusIMGUI cache)
        {
            cache = EnsureKey(property);
            (string error, AddressableAddressDropdownInfo dropdownInfo) =
                GetAddressableAddressDropdownInfo(property, addressableAddressAttribute, true);
            cache.Error = error;
            cache.DropdownInfo = dropdownInfo;
            return (error, dropdownInfo);
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            UpdateStatus(property, (AddressableAddressAttribute)saintsAttribute, out _);
            return SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string _, AddressableAddressDropdownInfo dropdownInfo) =
                UpdateStatus(property, addressableAddressAttribute, out AddressableAddressStatusIMGUI cache);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - fieldRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
            if (!dropdownInfo.IsString)
            {
                GUI.Label(fieldRect, GUIContent.none);
                return;
            }

            GUI.SetNextControlName(FieldControlName);
            string display = GetAddressableAddressDisplay(property.stringValue, dropdownInfo.Addresses, false);
            if (!EditorGUI.DropdownButton(fieldRect, new GUIContent(display), FocusType.Keyboard))
            {
                return;
            }

            if (dropdownInfo.MetaInfo.DropdownListValue == null)
            {
                return;
            }

            PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                dropdownInfo.MetaInfo,
                Mathf.Max(fieldRect.width, 220f),
                320f,
                false,
                (curItem, _) =>
                {
                    bool changed = ApplyAddressableAddressSelection(property, info, parent, dropdownInfo.Settings,
                        (string)curItem, newValue => TriggerChangedIMGUI(property, newValue));
                    if (changed)
                    {
                        cache.Error = "";
                    }
                    return null;
                }));
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            return UpdateStatus(property, addressableAddressAttribute, out _).error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            AddressableAddressAttribute addressableAddressAttribute = (AddressableAddressAttribute)saintsAttribute;
            (string error, AddressableAddressDropdownInfo _) = UpdateStatus(property, addressableAddressAttribute, out _);

            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
