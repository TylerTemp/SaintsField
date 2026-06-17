using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    public partial class DropdownAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private static MetaInfo UpdateStatus(SerializedProperty property, MenuDropdownAttribute dropdownAttribute,
            FieldInfo info, object parent, out InfoIMGUI cache)
        {
            cache = EnsureKey(property);
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);
            cache.Error = metaInfo.Error;
            return metaInfo;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            MenuDropdownAttribute dropdownAttribute = (MenuDropdownAttribute)saintsAttribute;
            MetaInfo metaInfo = UpdateStatus(property, dropdownAttribute, info, parent, out InfoIMGUI cache);

            bool hasLabel = label.text != "";
            float labelWidth = hasLabel ? EditorGUIUtility.labelWidth : 0;
            Rect labelRect = new Rect(position)
            {
                width = labelWidth,
            };

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string curDisplay = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(curDisplay), FocusType.Keyboard))
            {
                metaInfo = UpdateStatus(property, dropdownAttribute, info, parent, out cache);
                if (metaInfo.Error != "")
                {
                    return;
                }

                ShowGenericMenu(metaInfo, curDisplay, fieldRect, (_, item) =>
                {
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, item);
                    Util.SignPropertyValue(property, info, parent, item);
                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, item);
                    property.serializedObject.ApplyModifiedProperties();
                }, !dropdownAttribute.SlashAsSub);
            }

            if (hasLabel)
            {
                ClickFocus(labelRect, FieldControlName);
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => UpdateStatus(property, (MenuDropdownAttribute)saintsAttribute, info, parent, out _).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
