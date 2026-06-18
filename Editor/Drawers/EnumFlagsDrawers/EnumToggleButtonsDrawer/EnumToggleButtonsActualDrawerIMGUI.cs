using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer
    {
        internal float GetSerializedActualFieldHeight(SaintsSerializedActualAttribute saintsSerializedActual,
            EnumToggleButtonsAttribute enumToggleButtonsAttribute, SerializedProperty property, GUIContent label,
            float width, object parent, IRichTextTagProvider richTextTagProvider)
        {
            (string error, EnumMetaInfo metaInfo, SerializedProperty valueProperty, bool isULong) =
                GetSerializedActualEnumInfo(saintsSerializedActual, property, parent);
            if (error != "")
            {
                return GetErrorHeight(error, width);
            }

            ImGuiInfo cache = EnsureKey(property);
            cache.Error = "";

            float inputWidth = ValueButtonsAttributeDrawer.UtilGetFieldInputWidth(width, label);
            bool showSubRows = enumToggleButtonsAttribute.NoFold || property.isExpanded;
            ValueButtonsAttributeDrawer.ImGuiButtonLayout layout = GetSerializedActualButtonLayout(metaInfo,
                valueProperty, isULong, inputWidth, inputWidth, !showSubRows, cache, richTextTagProvider);

            return EditorGUIUtility.singleLineHeight +
                   ValueButtonsAttributeDrawer.UtilGetBelowHeight(width, showSubRows, "", layout);
        }

        internal bool DrawSerializedActualField(Rect position, SaintsSerializedActualAttribute saintsSerializedActual,
            EnumToggleButtonsAttribute enumToggleButtonsAttribute, SerializedProperty property, GUIContent label,
            object parent, IRichTextTagProvider richTextTagProvider, Action<object> onValueChanged)
        {
            (string error, EnumMetaInfo metaInfo, SerializedProperty valueProperty, bool isULong) =
                GetSerializedActualEnumInfo(saintsSerializedActual, property, parent);
            if (error != "")
            {
                ImGuiHelpBox.Draw(position, error, MessageType.Error);
                return true;
            }

            EnsureImGuiResources();

            ImGuiInfo cache = EnsureKey(property);
            cache.Error = "";

            Rect lineRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect fieldRect = EditorGUI.PrefixLabel(lineRect, label);

            if (!metaInfo.IsFlags)
            {
                DrawSerializedActualNonFlags(fieldRect, position, property, valueProperty, metaInfo,
                    enumToggleButtonsAttribute, isULong, cache, richTextTagProvider, onValueChanged);
                return true;
            }

            DrawSerializedActualFlags(fieldRect, position, property, valueProperty, metaInfo,
                enumToggleButtonsAttribute, isULong, cache, richTextTagProvider, onValueChanged);
            return true;
        }

        private ValueButtonsAttributeDrawer.ImGuiButtonLayout GetSerializedActualButtonLayout(EnumMetaInfo metaInfo,
            SerializedProperty valueProperty, bool isULong, float mainWidth, float subWidth, bool greedy,
            ImGuiInfo cache, IRichTextTagProvider richTextTagProvider)
        {
            ValueButtonRawInfo[] rawInfos;
            float useMainWidth = mainWidth;

            if (metaInfo.IsFlags)
            {
                rawInfos = GetFlagRawInfos(metaInfo, richTextTagProvider);
                bool showFullToggles = !greedy;
                float fullToggleWidth = EditorGUIUtility.singleLineHeight * (showFullToggles ? 2f : 1f);
                useMainWidth = Mathf.Max(1f, mainWidth - fullToggleWidth);
            }
            else
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetSerializedActualNonFlagsMeta(metaInfo, valueProperty,
                    isULong);
                rawInfos = ValueButtonsAttributeDrawer.UtilMakeButtonRawInfos(dropdownMetaInfo, richTextTagProvider);
            }

            return ValueButtonsAttributeDrawer.UtilGetButtonLayout(useMainWidth, subWidth, greedy, rawInfos,
                cache.RichTextDrawer);
        }

        private void DrawSerializedActualNonFlags(Rect fieldRect, Rect position, SerializedProperty property,
            SerializedProperty valueProperty, EnumMetaInfo metaInfo, EnumToggleButtonsAttribute enumToggleButtonsAttribute,
            bool isULong, ImGuiInfo cache, IRichTextTagProvider richTextTagProvider, Action<object> onValueChanged)
        {
            AdvancedDropdownMetaInfo dropdownMetaInfo = GetSerializedActualNonFlagsMeta(metaInfo, valueProperty,
                isULong);
            ValueButtonRawInfo[] rawInfos = ValueButtonsAttributeDrawer.UtilMakeButtonRawInfos(dropdownMetaInfo,
                richTextTagProvider);
            bool showSubRows = enumToggleButtonsAttribute.NoFold || property.isExpanded;

            ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(fieldRect.width, fieldRect.width, !showSubRows,
                    rawInfos, cache.RichTextDrawer);

            Rect buttonsRect = fieldRect;
            if (!enumToggleButtonsAttribute.NoFold && layout.HasSubRow)
            {
                buttonsRect = ValueButtonsAttributeDrawer.UtilDrawFoldout(buttonsRect, property);
            }

            if (layout.Rows.Count > 0)
            {
                ValueButtonsAttributeDrawer.UtilDrawButtonRow(buttonsRect, layout.Rows[0], layout.MainAvailableWidth,
                    cache.RichTextDrawer,
                    buttonInfo => dropdownMetaInfo.CurValues.Any(each => Util.GetIsEqual(each, buttonInfo.Value)),
                    buttonInfo => SetSerializedActualEnumValue(valueProperty, metaInfo, buttonInfo.Value, isULong,
                        onValueChanged));
            }

            Rect belowRect = new Rect(position)
            {
                y = fieldRect.yMax,
                height = Mathf.Max(0f, position.yMax - fieldRect.yMax),
            };
            ValueButtonsAttributeDrawer.UtilDrawBelow(belowRect, showSubRows, "", layout, cache.RichTextDrawer,
                buttonInfo => dropdownMetaInfo.CurValues.Any(each => Util.GetIsEqual(each, buttonInfo.Value)),
                buttonInfo => SetSerializedActualEnumValue(valueProperty, metaInfo, buttonInfo.Value, isULong,
                    onValueChanged));
        }

        private void DrawSerializedActualFlags(Rect fieldRect, Rect position, SerializedProperty property,
            SerializedProperty valueProperty, EnumMetaInfo metaInfo, EnumToggleButtonsAttribute enumToggleButtonsAttribute,
            bool isULong, ImGuiInfo cache, IRichTextTagProvider richTextTagProvider, Action<object> onValueChanged)
        {
            ValueButtonRawInfo[] rawInfos = GetFlagRawInfos(metaInfo, richTextTagProvider);
            bool showFullToggles = enumToggleButtonsAttribute.NoFold || property.isExpanded;
            float fullToggleWidth = EditorGUIUtility.singleLineHeight * (showFullToggles ? 2f : 1f);

            ValueButtonsAttributeDrawer.ImGuiButtonLayout layout =
                ValueButtonsAttributeDrawer.UtilGetButtonLayout(
                    Mathf.Max(1f, fieldRect.width - fullToggleWidth),
                    fieldRect.width,
                    !showFullToggles,
                    rawInfos,
                    cache.RichTextDrawer);

            Rect controlRect = fieldRect;
            if (!enumToggleButtonsAttribute.NoFold && layout.HasSubRow)
            {
                controlRect = ValueButtonsAttributeDrawer.UtilDrawFoldout(controlRect, property);
            }

            (Rect toggleGroupRect, Rect buttonRect) = RectUtils.SplitWidthRect(controlRect, fullToggleWidth);
            DrawSerializedActualFlagToggleGroup(toggleGroupRect, valueProperty, metaInfo, isULong, showFullToggles,
                onValueChanged);

            if (layout.Rows.Count > 0)
            {
                ValueButtonsAttributeDrawer.UtilDrawButtonRow(buttonRect, layout.Rows[0],
                    layout.MainAvailableWidth, cache.RichTextDrawer,
                    buttonInfo => IsSerializedActualFlagOn(valueProperty, buttonInfo.Value, isULong),
                    buttonInfo => ToggleSerializedActualFlag(valueProperty, metaInfo, buttonInfo.Value, isULong,
                        onValueChanged));
            }

            Rect belowRect = new Rect(position)
            {
                y = fieldRect.yMax,
                height = Mathf.Max(0f, position.yMax - fieldRect.yMax),
            };
            ValueButtonsAttributeDrawer.UtilDrawBelow(belowRect, showFullToggles, "", layout, cache.RichTextDrawer,
                buttonInfo => IsSerializedActualFlagOn(valueProperty, buttonInfo.Value, isULong),
                buttonInfo => ToggleSerializedActualFlag(valueProperty, metaInfo, buttonInfo.Value, isULong,
                    onValueChanged));
        }

        private void DrawSerializedActualFlagToggleGroup(Rect position, SerializedProperty valueProperty,
            EnumMetaInfo metaInfo, bool isULong, bool showFullToggles, Action<object> onValueChanged)
        {
            if (isULong)
            {
                DrawSerializedActualFlagToggleGroupULong(position, valueProperty, metaInfo, showFullToggles,
                    onValueChanged);
                return;
            }

            long curValue = valueProperty.longValue;
            long everything = Convert.ToInt64(metaInfo.EverythingBit);

            if (showFullToggles)
            {
                (Rect checkAllRect, Rect afterCheckAllRect) =
                    RectUtils.SplitWidthRect(position, EditorGUIUtility.singleLineHeight);
                using (new EditorGUI.DisabledScope(curValue == everything))
                {
                    if (GUI.Button(checkAllRect, _checkboxCheckedTexture2D, _iconButtonStyle))
                    {
                        SetSerializedActualEnumValue(valueProperty, metaInfo, everything, false, onValueChanged);
                    }
                }

                (Rect emptyRect, _) = RectUtils.SplitWidthRect(afterCheckAllRect, EditorGUIUtility.singleLineHeight);
                using (new EditorGUI.DisabledScope(curValue == 0L))
                {
                    if (GUI.Button(emptyRect, _checkboxEmptyTexture2D, _iconButtonStyle))
                    {
                        SetSerializedActualEnumValue(valueProperty, metaInfo, 0L, false, onValueChanged);
                    }
                }

                return;
            }

            Texture2D toggleTexture;
            long targetValue;
            if (curValue == 0L)
            {
                toggleTexture = _checkboxEmptyTexture2D;
                targetValue = everything;
            }
            else if (curValue == everything)
            {
                toggleTexture = _checkboxCheckedTexture2D;
                targetValue = 0L;
            }
            else
            {
                toggleTexture = _checkboxIndeterminateTexture2D;
                targetValue = everything;
            }

            if (GUI.Button(position, toggleTexture, _iconButtonStyle))
            {
                SetSerializedActualEnumValue(valueProperty, metaInfo, targetValue, false, onValueChanged);
            }
        }

        private void DrawSerializedActualFlagToggleGroupULong(Rect position, SerializedProperty valueProperty,
            EnumMetaInfo metaInfo, bool showFullToggles, Action<object> onValueChanged)
        {
#if UNITY_2022_1_OR_NEWER
            ulong curValue = valueProperty.ulongValue;
            ulong everything = Convert.ToUInt64(metaInfo.EverythingBit);

            if (showFullToggles)
            {
                (Rect checkAllRect, Rect afterCheckAllRect) =
                    RectUtils.SplitWidthRect(position, EditorGUIUtility.singleLineHeight);
                using (new EditorGUI.DisabledScope(curValue == everything))
                {
                    if (GUI.Button(checkAllRect, _checkboxCheckedTexture2D, _iconButtonStyle))
                    {
                        SetSerializedActualEnumValue(valueProperty, metaInfo, everything, true, onValueChanged);
                    }
                }

                (Rect emptyRect, _) = RectUtils.SplitWidthRect(afterCheckAllRect, EditorGUIUtility.singleLineHeight);
                using (new EditorGUI.DisabledScope(curValue == 0UL))
                {
                    if (GUI.Button(emptyRect, _checkboxEmptyTexture2D, _iconButtonStyle))
                    {
                        SetSerializedActualEnumValue(valueProperty, metaInfo, 0UL, true, onValueChanged);
                    }
                }

                return;
            }

            Texture2D toggleTexture;
            ulong targetValue;
            if (curValue == 0UL)
            {
                toggleTexture = _checkboxEmptyTexture2D;
                targetValue = everything;
            }
            else if (curValue == everything)
            {
                toggleTexture = _checkboxCheckedTexture2D;
                targetValue = 0UL;
            }
            else
            {
                toggleTexture = _checkboxIndeterminateTexture2D;
                targetValue = everything;
            }

            if (GUI.Button(position, toggleTexture, _iconButtonStyle))
            {
                SetSerializedActualEnumValue(valueProperty, metaInfo, targetValue, true, onValueChanged);
            }
#endif
        }

        private static bool IsSerializedActualFlagOn(SerializedProperty valueProperty, object flagValue, bool isULong)
        {
            if (isULong)
            {
#if UNITY_2022_1_OR_NEWER
                ulong toggle = Convert.ToUInt64(flagValue);
                return EnumFlagsUtil.IsOn(valueProperty.ulongValue, toggle);
#else
                return false;
#endif
            }

            long longToggle = Convert.ToInt64(flagValue);
            return EnumFlagsUtil.IsOn(valueProperty.longValue, longToggle);
        }

        private static void ToggleSerializedActualFlag(SerializedProperty valueProperty, EnumMetaInfo metaInfo,
            object flagValue, bool isULong, Action<object> onValueChanged)
        {
            if (isULong)
            {
#if UNITY_2022_1_OR_NEWER
                ulong toggle = Convert.ToUInt64(flagValue);
                ulong newValue = EnumFlagsUtil.ToggleBit(valueProperty.ulongValue, toggle);
                SetSerializedActualEnumValue(valueProperty, metaInfo, newValue, true, onValueChanged);
#endif
                return;
            }

            long longToggle = Convert.ToInt64(flagValue);
            long longNewValue = EnumFlagsUtil.ToggleBit(valueProperty.longValue, longToggle);
            SetSerializedActualEnumValue(valueProperty, metaInfo, longNewValue, false, onValueChanged);
        }

        private static AdvancedDropdownMetaInfo GetSerializedActualNonFlagsMeta(EnumMetaInfo metaInfo,
            SerializedProperty valueProperty, bool isULong)
        {
            object curValue = Enum.ToObject(metaInfo.EnumType,
                isULong
#if UNITY_2022_1_OR_NEWER
                    ? valueProperty.ulongValue
#else
                    ? 0UL
#endif
                    : valueProperty.longValue);

            Dropdown<object> enumDropdown = new Dropdown<object>("");
            foreach ((object enumValue, string enumLabel, string enumRichLabel) in Util.GetEnumValues(metaInfo.EnumType))
            {
                HashSet<string> extraSearches = enumRichLabel == enumLabel
                    ? new HashSet<string>
                    {
                        enumValue.ToString(),
                    }
                    : new HashSet<string>();
                enumDropdown.Add(enumRichLabel ?? enumLabel, enumValue, extraSearches: extraSearches);
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = new[] { curValue },
                DropdownListValue = enumDropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        private static void SetSerializedActualEnumValue(SerializedProperty valueProperty, EnumMetaInfo metaInfo,
            object rawValue, bool isULong, Action<object> onValueChanged)
        {
            object changedValue;
            if (isULong)
            {
#if UNITY_2022_1_OR_NEWER
                ulong newValue = Convert.ToUInt64(rawValue);
                valueProperty.ulongValue = newValue;
                changedValue = Enum.ToObject(metaInfo.EnumType, newValue);
#else
                return;
#endif
            }
            else
            {
                long newValue = Convert.ToInt64(rawValue);
                valueProperty.longValue = newValue;
                changedValue = Enum.ToObject(metaInfo.EnumType, newValue);
            }

            valueProperty.serializedObject.ApplyModifiedProperties();
            onValueChanged?.Invoke(changedValue);
        }

        private static (string error, EnumMetaInfo metaInfo, SerializedProperty valueProperty, bool isULong)
            GetSerializedActualEnumInfo(SaintsSerializedActualAttribute saintsSerializedActual,
                SerializedProperty property, object parent)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                return ($"Failed to get type for {property.propertyPath}", default, null, false);
            }

            if (!targetType.IsEnum)
            {
                return ($"{targetType} is not an enum type", default, null, false);
            }

            SerializedProperty propertyTypeProperty =
                property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType));
            if (propertyTypeProperty == null)
            {
                return ($"propertyType not found in {property.propertyPath}", default, null, false);
            }

            bool isULong = false;
            SerializedProperty valueProperty;
            SaintsPropertyType propertyType = (SaintsPropertyType)propertyTypeProperty.intValue;
            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
                    valueProperty = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                    break;
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                    isULong = true;
                    valueProperty = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                    break;
#endif
                default:
                    return ($"{propertyType} is not an enum serialized actual type", default, null, false);
            }

            if (valueProperty == null)
            {
                return ($"Enum value not found in {property.propertyPath}", default, null, false);
            }

            return ("", EnumFlagsUtil.GetEnumMetaInfo(targetType), valueProperty, isULong);
        }

        private static float GetErrorHeight(string error, float width) =>
            ImGuiHelpBox.GetHeight(error, Mathf.Max(1f, width), MessageType.Error);
    }
}
