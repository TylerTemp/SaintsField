using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.I2Loc.LocalizedStringPickerDrawer
{
    public partial class LocalizedStringPickerAttributeDrawer
    {
        private sealed class LocalizedStringPickerStatusIMGUI
        {
            public string Error = "";
            public Texture2D IconDown;
            public GUIStyle ButtonStyle;
        }

        private static readonly Dictionary<string, LocalizedStringPickerStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, LocalizedStringPickerStatusIMGUI>();

        private static LocalizedStringPickerStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out LocalizedStringPickerStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new LocalizedStringPickerStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private static LocalizedStringPickerStatusIMGUI UpdateStatus(SerializedProperty property)
        {
            LocalizedStringPickerStatusIMGUI cache = EnsureKey(property);
            cache.Error = MismatchError(property);
            return cache;
        }

        private static GUIStyle GetButtonStyle(LocalizedStringPickerStatusIMGUI cache)
        {
            return cache.ButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(3, 3, 3, 3),
            };
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            UpdateStatus(property);
            return SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            LocalizedStringPickerStatusIMGUI cache = UpdateStatus(property);
            if (cache.Error != "")
            {
                return true;
            }

            cache.IconDown ??= Util.LoadResource<Texture2D>("classic-dropdown-gray.png");

            if(GUI.Button(position, cache.IconDown, GetButtonStyle(cache)))
            {
                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(GetCurrentValue(property), true);
                PopupWindow.Show(position, new SaintsTreeDropdownIMGUI(
                    metaInfo,
                    Mathf.Max(fullRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        ApplySelection(property, info, (string)curItem,
                            newValue => TriggerChangedIMGUI(property, newValue));
                        return null;
                    }));
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            return UpdateStatus(property).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == ""? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
