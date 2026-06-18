using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaMaskDrawer
{
    public partial class NavMeshAreaMaskAttributeDrawer
    {
        private sealed class NavMeshAreaMaskStatusIMGUI
        {
            public string Error = "";
            public AdvancedDropdownMetaInfo MetaInfo;
        }

        private static readonly Dictionary<string, NavMeshAreaMaskStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, NavMeshAreaMaskStatusIMGUI>();

        private static NavMeshAreaMaskStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out NavMeshAreaMaskStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new NavMeshAreaMaskStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private static NavMeshAreaMaskStatusIMGUI UpdateStatus(SerializedProperty property)
        {
            NavMeshAreaMaskStatusIMGUI cache = EnsureKey(property);
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                cache.Error = $"Type {property.propertyType} is not int.";
                cache.MetaInfo = default;
                return cache;
            }

            cache.Error = "";
            cache.MetaInfo = GetDropdownMetaInfo(property.intValue);
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            UpdateStatus(property);
            return SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            NavMeshAreaMaskStatusIMGUI cache = UpdateStatus(property);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            if (cache.Error != "")
            {
                GUI.Label(fieldRect, GUIContent.none);
                return;
            }

            GUI.SetNextControlName(FieldControlName);
            if (!EditorGUI.DropdownButton(fieldRect, new GUIContent(GetDisplay(property.intValue)), FocusType.Keyboard))
            {
                return;
            }

            PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                cache.MetaInfo,
                Mathf.Max(fieldRect.width, 220f),
                320f,
                true,
                (curItem, isOn) => ApplyDropdownSelection(property, info, parent, (DropdownItem)curItem, isOn,
                    newValue => TriggerChangedIMGUI(property, newValue))));
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => UpdateStatus(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
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
