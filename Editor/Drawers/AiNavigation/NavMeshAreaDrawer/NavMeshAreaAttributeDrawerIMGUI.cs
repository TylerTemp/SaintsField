using System.Collections.Generic;
using System.Reflection;
using SaintsField.AiNavigation;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
    public partial class NavMeshAreaAttributeDrawer
    {
        private sealed class NavMeshAreaStatusIMGUI
        {
            public string Error = "";
            public AdvancedDropdownMetaInfo MetaInfo;
        }

        private static readonly Dictionary<string, NavMeshAreaStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, NavMeshAreaStatusIMGUI>();

        private static NavMeshAreaStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out NavMeshAreaStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new NavMeshAreaStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private static NavMeshAreaStatusIMGUI UpdateStatus(SerializedProperty property,
            NavMeshAreaAttribute navMeshAreaAttribute)
        {
            NavMeshAreaStatusIMGUI cache = EnsureKey(property);
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.String:
                    cache.Error = "";
                    cache.MetaInfo = GetDropdownMetaInfo(navMeshAreaAttribute, property);
                    break;
                default:
                    cache.Error = $"Type {property.propertyType} is not int or string.";
                    cache.MetaInfo = default;
                    break;
            }

            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            UpdateStatus(property, (NavMeshAreaAttribute)saintsAttribute);
            return SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            NavMeshAreaAttribute navMeshAreaAttribute = (NavMeshAreaAttribute)saintsAttribute;
            NavMeshAreaStatusIMGUI cache = UpdateStatus(property, navMeshAreaAttribute);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - fieldRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
            if (cache.Error != "")
            {
                GUI.Label(fieldRect, GUIContent.none);
                return;
            }

            GUI.SetNextControlName(FieldControlName);
            if (!EditorGUI.DropdownButton(fieldRect,
                    new GUIContent(GetDisplay(navMeshAreaAttribute, property)), FocusType.Keyboard))
            {
                return;
            }

            bool allowUnselect = property.propertyType == SerializedPropertyType.Integer && navMeshAreaAttribute.IsMask;
            PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                cache.MetaInfo,
                Mathf.Max(fieldRect.width, 220f),
                320f,
                allowUnselect,
                (curItem, isOn) => ApplyDropdownSelection(navMeshAreaAttribute, property, info, parent,
                    (DropdownItem)curItem, isOn, newValue => TriggerChangedIMGUI(property, newValue))));
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => UpdateStatus(property, (NavMeshAreaAttribute)saintsAttribute).Error != "";

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
