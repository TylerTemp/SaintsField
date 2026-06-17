using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer
    {
        private sealed class ImGuiInfo
        {
            public string Error = "";
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
        }

        private static readonly Dictionary<string, ImGuiInfo> InfoCacheIMGUI =
            new Dictionary<string, ImGuiInfo>();

        private GUIStyle _iconButtonStyle;

        private static ImGuiInfo EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out ImGuiInfo cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new ImGuiInfo();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        private void EnsureImGuiResources()
        {
            if (_checkboxCheckedTexture2D == null)
            {
                LoadIcons();
            }

            if (_iconButtonStyle != null)
            {
                return;
            }

            const int padding = 2;
            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(padding, padding, padding, padding),
            };
        }

        private AdvancedDropdownMetaInfo UpdateNonFlagsStatus(SerializedProperty property,
            EnumToggleButtonsAttribute enumToggleButtonsAttribute, MemberInfo info, object parent, out ImGuiInfo cache,
            out ValueButtonRawInfo[] rawInfos)
        {
            cache = EnsureKey(property);
            AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property,
                enumToggleButtonsAttribute, info, parent, true, true);
            cache.Error = metaInfo.Error;
            rawInfos = ValueButtonsAttributeDrawer.UtilMakeButtonRawInfos(metaInfo, this);
            return metaInfo;
        }

        private static ValueButtonRawInfo[] GetFlagRawInfos(EnumMetaInfo metaInfo,
            IRichTextTagProvider richTextTagProvider)
        {
            return metaInfo.EnumValues
                .Select(each => new ValueButtonRawInfo(
                    RichTextDrawer.ParseRichXmlWithProvider(each.Label, richTextTagProvider).ToArray(),
                    false,
                    each.Value))
                .ToArray();
        }

        private static void SetFlagValue(SerializedProperty property, MemberInfo info, object parent,
            EnumMetaInfo metaInfo, long newValue)
        {
            EnumFlagsUtil.SetSerializedPropertyEnumValue(metaInfo.EnumType, property, newValue);
            property.serializedObject.ApplyModifiedProperties();

            object changedValue = Enum.ToObject(metaInfo.EnumType, newValue);
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                changedValue);
            TriggerChangedIMGUI(property, changedValue);
        }

        private static bool IsEverything(long curValue, EnumMetaInfo metaInfo)
        {
            object currentEnum = Enum.ToObject(metaInfo.EnumType, curValue);
            return currentEnum.Equals(metaInfo.EverythingBit);
        }

        private void DrawFlagToggleGroup(Rect position, SerializedProperty property, MemberInfo info, object parent,
            EnumMetaInfo metaInfo, bool showFullToggles)
        {
            long curValue = EnumFlagsUtil.GetSerializedPropertyEnumValue(metaInfo.EnumType, property);

            if (showFullToggles)
            {
                (Rect checkAllRect, Rect afterCheckAllRect) =
                    RectUtils.SplitWidthRect(position, EditorGUIUtility.singleLineHeight);
                using (new EditorGUI.DisabledScope(IsEverything(curValue, metaInfo)))
                {
                    if (GUI.Button(checkAllRect, _checkboxCheckedTexture2D, _iconButtonStyle))
                    {
                        SetFlagValue(property, info, parent, metaInfo, Convert.ToInt64(metaInfo.EverythingBit));
                    }
                }

                (Rect emptyRect, _) = RectUtils.SplitWidthRect(afterCheckAllRect, EditorGUIUtility.singleLineHeight);
                using (new EditorGUI.DisabledScope(curValue == 0))
                {
                    if (GUI.Button(emptyRect, _checkboxEmptyTexture2D, _iconButtonStyle))
                    {
                        SetFlagValue(property, info, parent, metaInfo, 0L);
                    }
                }

                return;
            }

            Texture2D toggleTexture;
            long targetValue;
            if (curValue == 0)
            {
                toggleTexture = _checkboxEmptyTexture2D;
                targetValue = Convert.ToInt64(metaInfo.EverythingBit);
            }
            else if (IsEverything(curValue, metaInfo))
            {
                toggleTexture = _checkboxCheckedTexture2D;
                targetValue = 0L;
            }
            else
            {
                toggleTexture = _checkboxIndeterminateTexture2D;
                targetValue = Convert.ToInt64(metaInfo.EverythingBit);
            }

            if (GUI.Button(position, toggleTexture, _iconButtonStyle))
            {
                SetFlagValue(property, info, parent, metaInfo, targetValue);
            }
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            EnsureImGuiResources();

            EnumToggleButtonsAttribute enumToggleButtonsAttribute = (EnumToggleButtonsAttribute)saintsAttribute;
            EnumFlagsMetaInfo flagsMetaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            if (!flagsMetaInfo.HasFlags)
            {
                AdvancedDropdownMetaInfo metaInfo = UpdateNonFlagsStatus(property, enumToggleButtonsAttribute, info,
                    parent, out ImGuiInfo cache, out ValueButtonRawInfo[] rawInfos);

                Rect fieldRect = EditorGUI.PrefixLabel(position, label);
                ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                    ValueButtonsAttributeDrawer.UtilGetButtonLayout(fieldRect.width, fieldRect.width,
                        !enumToggleButtonsAttribute.NoFold && !property.isExpanded, rawInfos, cache.RichTextDrawer);

                Rect buttonsRect = fieldRect;
                if (!enumToggleButtonsAttribute.NoFold && layout.HasSubRow)
                {
                    buttonsRect = ValueButtonsAttributeDrawer.UtilDrawFoldout(buttonsRect, property);
                }

                if (layout.Rows.Count == 0)
                {
                    return;
                }

                ValueButtonsAttributeDrawer.UtilDrawButtonRow(buttonsRect, layout.Rows[0], layout.MainAvailableWidth,
                    cache.RichTextDrawer,
                    buttonInfo => metaInfo.CurValues.Any(each => Util.GetIsEqual(each, buttonInfo.Value)),
                    buttonInfo =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parent, buttonInfo.Value);
                        Util.SignPropertyValue(property, info, parent, buttonInfo.Value);
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, buttonInfo.Value);
                    });
                return;
            }

            ImGuiInfo flagsCache = EnsureKey(property);
            flagsCache.Error = "";

            EnumMetaInfo metaInfoFlags = EnumFlagsUtil.GetEnumMetaInfo(flagsMetaInfo.EnumType);
            ValueButtonRawInfo[] flagRawInfos = GetFlagRawInfos(metaInfoFlags, this);

            Rect flagsFieldRect = EditorGUI.PrefixLabel(position, label);
            bool showFullToggles = enumToggleButtonsAttribute.NoFold || property.isExpanded;
            float fullToggleWidth = EditorGUIUtility.singleLineHeight * (showFullToggles ? 2f : 1f);

            ValueButtonsAttributeDrawer.ImGuiButtonLayout flagLayout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(
                    Mathf.Max(1f, flagsFieldRect.width - fullToggleWidth),
                    flagsFieldRect.width,
                    !showFullToggles,
                    flagRawInfos,
                    flagsCache.RichTextDrawer);

            Rect controlRect = flagsFieldRect;
            if (!enumToggleButtonsAttribute.NoFold && flagLayout.HasSubRow)
            {
                controlRect = ValueButtonsAttributeDrawer.UtilDrawFoldout(controlRect, property);
            }

            (Rect toggleGroupRect, Rect buttonRect) = RectUtils.SplitWidthRect(controlRect, fullToggleWidth);
            DrawFlagToggleGroup(toggleGroupRect, property, info, parent, metaInfoFlags, showFullToggles);

            if (flagLayout.Rows.Count == 0)
            {
                return;
            }

            ValueButtonsAttributeDrawer.UtilDrawButtonRow(buttonRect, flagLayout.Rows[0], flagLayout.MainAvailableWidth,
                flagsCache.RichTextDrawer,
                buttonInfo => EnumFlagsUtil.IsOn(property.intValue, Convert.ToInt64(buttonInfo.Value)),
                buttonInfo =>
                {
                    long toggle = Convert.ToInt64(buttonInfo.Value);
                    long newValue = EnumFlagsUtil.ToggleBit(property.intValue, toggle);
                    SetFlagValue(property, info, parent, metaInfoFlags, newValue);
                });
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            EnumToggleButtonsAttribute enumToggleButtonsAttribute = (EnumToggleButtonsAttribute)saintsAttribute;
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            if (!metaInfo.HasFlags)
            {
                UpdateNonFlagsStatus(property, enumToggleButtonsAttribute, info, parent, out _, out _);
                return EnsureKey(property).Error != "" || enumToggleButtonsAttribute.NoFold || property.isExpanded;
            }

            return enumToggleButtonsAttribute.NoFold || property.isExpanded;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            EnumToggleButtonsAttribute enumToggleButtonsAttribute = (EnumToggleButtonsAttribute)saintsAttribute;
            EnumFlagsMetaInfo flagsMetaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            if (!flagsMetaInfo.HasFlags)
            {
                AdvancedDropdownMetaInfo metaInfo = UpdateNonFlagsStatus(property, enumToggleButtonsAttribute, info,
                    parent, out ImGuiInfo cache, out ValueButtonRawInfo[] rawInfos);
                float inputWidth = ValueButtonsAttributeDrawer.UtilGetFieldInputWidth(width, label);
                ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                    ValueButtonsAttributeDrawer.UtilGetButtonLayout(inputWidth, width,
                        !enumToggleButtonsAttribute.NoFold && !property.isExpanded, rawInfos, cache.RichTextDrawer);

                return ValueButtonsAttributeDrawer.UtilGetBelowHeight(width,
                    enumToggleButtonsAttribute.NoFold || property.isExpanded, metaInfo.Error, layout);
            }

            ImGuiInfo flagsCache = EnsureKey(property);
            flagsCache.Error = "";

            EnumMetaInfo metaInfoFlags = EnumFlagsUtil.GetEnumMetaInfo(flagsMetaInfo.EnumType);
            ValueButtonRawInfo[] rawInfosFlags = GetFlagRawInfos(metaInfoFlags, this);
            float flagsInputWidth = ValueButtonsAttributeDrawer.UtilGetFieldInputWidth(width, label);
            bool showFullToggles = enumToggleButtonsAttribute.NoFold || property.isExpanded;
            float fullToggleWidth = EditorGUIUtility.singleLineHeight * (showFullToggles ? 2f : 1f);
            ValueButtonsAttributeDrawer.ImGuiButtonLayout flagsLayout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(
                    Mathf.Max(1f, flagsInputWidth - fullToggleWidth),
                    width,
                    !showFullToggles,
                    rawInfosFlags,
                    flagsCache.RichTextDrawer);

            return ValueButtonsAttributeDrawer.UtilGetBelowHeight(width, showFullToggles, "", flagsLayout);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            EnumToggleButtonsAttribute enumToggleButtonsAttribute = (EnumToggleButtonsAttribute)saintsAttribute;
            EnumFlagsMetaInfo flagsMetaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            if (!flagsMetaInfo.HasFlags)
            {
                AdvancedDropdownMetaInfo metaInfo = UpdateNonFlagsStatus(property, enumToggleButtonsAttribute, info,
                    parent, out ImGuiInfo cache, out ValueButtonRawInfo[] rawInfos);
                float inputWidth = ValueButtonsAttributeDrawer.UtilGetFieldInputWidth(position.width, label);
                ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                    ValueButtonsAttributeDrawer.UtilGetButtonLayout(inputWidth, position.width,
                        !enumToggleButtonsAttribute.NoFold && !property.isExpanded, rawInfos, cache.RichTextDrawer);

                return ValueButtonsAttributeDrawer.UtilDrawBelow(position,
                    enumToggleButtonsAttribute.NoFold || property.isExpanded, metaInfo.Error, layout,
                    cache.RichTextDrawer,
                    buttonInfo => metaInfo.CurValues.Any(each => Util.GetIsEqual(each, buttonInfo.Value)),
                    buttonInfo =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parent, buttonInfo.Value);
                        Util.SignPropertyValue(property, info, parent, buttonInfo.Value);
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, buttonInfo.Value);
                    });
            }

            ImGuiInfo flagsCache = EnsureKey(property);
            flagsCache.Error = "";

            EnumMetaInfo metaInfoFlags = EnumFlagsUtil.GetEnumMetaInfo(flagsMetaInfo.EnumType);
            ValueButtonRawInfo[] rawInfosFlags = GetFlagRawInfos(metaInfoFlags, this);
            float flagsInputWidth = ValueButtonsAttributeDrawer.UtilGetFieldInputWidth(position.width, label);
            bool showFullToggles = enumToggleButtonsAttribute.NoFold || property.isExpanded;
            float fullToggleWidth = EditorGUIUtility.singleLineHeight * (showFullToggles ? 2f : 1f);
            ValueButtonsAttributeDrawer.ImGuiButtonLayout flagsLayout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(
                    Mathf.Max(1f, flagsInputWidth - fullToggleWidth),
                    position.width,
                    !showFullToggles,
                    rawInfosFlags,
                    flagsCache.RichTextDrawer);

            return ValueButtonsAttributeDrawer.UtilDrawBelow(position, showFullToggles, "", flagsLayout,
                flagsCache.RichTextDrawer,
                buttonInfo => EnumFlagsUtil.IsOn(property.intValue, Convert.ToInt64(buttonInfo.Value)),
                buttonInfo =>
                {
                    long toggle = Convert.ToInt64(buttonInfo.Value);
                    long newValue = EnumFlagsUtil.ToggleBit(property.intValue, toggle);
                    SetFlagValue(property, info, parent, metaInfoFlags, newValue);
                });
        }
    }
}
