using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    public partial class AddressableLabelAttributeDrawer
    {
        private sealed class AddressableLabelStatusIMGUI
        {
            public string Error = "";
            public AddressableLabelDropdownInfo DropdownInfo;
        }

        private static readonly Dictionary<string, AddressableLabelStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, AddressableLabelStatusIMGUI>();

        private static AddressableLabelStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out AddressableLabelStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new AddressableLabelStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private static (string error, AddressableLabelDropdownInfo dropdownInfo) UpdateStatus(
            SerializedProperty property, out AddressableLabelStatusIMGUI cache)
        {
            cache = EnsureKey(property);
            (string error, AddressableLabelDropdownInfo dropdownInfo) =
                GetAddressableLabelDropdownInfo(property, true);
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
            UpdateStatus(property, out _);
            return SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            (string _, AddressableLabelDropdownInfo dropdownInfo) =
                UpdateStatus(property, out AddressableLabelStatusIMGUI cache);

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
            string display = GetAddressableLabelDisplay(property.stringValue, dropdownInfo.Labels, false);
            if (!EditorGUI.DropdownButton(fieldRect, new GUIContent(display), FocusType.Keyboard))
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
                    bool changed = ApplyAddressableLabelSelection(property, info, parent, dropdownInfo.Settings,
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
            object parent) => UpdateStatus(property, out _).error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            (string error, AddressableLabelDropdownInfo _) = UpdateStatus(property, out _);
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
