using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsDropdownDrawer
{
    public partial class FlagsDropdownAttributeDrawer
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

        private static InfoIMGUI UpdateStatus(SerializedProperty property, FieldInfo info)
        {
            InfoIMGUI cache = EnsureKey(property);
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                cache.Error = $"Type {property.propertyType} is not a enum type";
                return cache;
            }

            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            AdvancedDropdownMetaInfo dropdownMetaInfo = EnumFlagsUtil.GetDropdownMetaInfo(
                property.intValue,
                metaInfo.AllCheckedLong,
                metaInfo.BitValueToName);
            cache.Error = dropdownMetaInfo.Error;
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cache = UpdateStatus(property, info);
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label, GUIContent.none);
                return;
            }

            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            AdvancedDropdownMetaInfo dropdownMetaInfo = EnumFlagsUtil.GetDropdownMetaInfo(
                property.intValue,
                metaInfo.AllCheckedLong,
                metaInfo.BitValueToName);
            cache.Error = dropdownMetaInfo.Error;

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - leftRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);

            GUI.SetNextControlName(FieldControlName);
            string display = GetSelectedNames(metaInfo.BitValueToName, property.intValue);
            // Debug.Assert(false, "Here");
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                PopupWindow.Show(leftRect, new SaintsTreeDropdownIMGUI(
                    dropdownMetaInfo,
                    leftRect.width,
                    320f,
                    true,
                    (curItem, _) =>
                    {
                        long selectedValue = (long)curItem;
                        long currentMask = EnumFlagsUtil.GetSerializedPropertyEnumValue(metaInfo.EnumType, property);
                        long newMask = selectedValue == 0
                            ? 0
                            : EnumFlagsUtil.ToggleBit(currentMask, selectedValue);

                        EnumFlagsUtil.SetSerializedPropertyEnumValue(metaInfo.EnumType, property, newMask);
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, newMask);
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                            System.Enum.ToObject(metaInfo.EnumType, newMask));
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        return EnumFlagsUtil.GetDropdownMetaInfo(newMask, metaInfo.AllCheckedLong, metaInfo.BitValueToName).CurValues;
                    }));
            }

            #endregion
        }

        private static int GetValueItemCounts(IDropdown dropdownList)
        {
            if (dropdownList.isSeparator)
            {
                return 0;
            }

            if(dropdownList.ChildCount() == 0)
            {
                return 1;
            }

            return dropdownList.children.Sum(child => GetValueItemCounts(child));
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => UpdateStatus(property, info).Error != "";

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
