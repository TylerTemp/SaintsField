using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer
    {
        internal float GetSerializedActualFieldHeight(SaintsSerializedActualAttribute saintsSerializedActual,
            SerializedProperty property, GUIContent label, float width, object parent)
        {
            (string error, _, _, _) = GetSerializedActualEnumInfo(saintsSerializedActual, property, parent);
            return error == ""
                ? EditorGUIUtility.singleLineHeight
                : ImGuiHelpBox.GetHeight(error, Mathf.Max(1f, width), MessageType.Error);
        }

        internal bool DrawSerializedActualField(Rect position, SaintsSerializedActualAttribute saintsSerializedActual,
            SerializedProperty property, GUIContent label, object parent, IRichTextTagProvider richTextTagProvider,
            Action<object> onValueChanged)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            (string error, EnumMetaInfo metaInfo, SerializedProperty valueProperty, bool isULong) =
                GetSerializedActualEnumInfo(saintsSerializedActual, property, parent);
            if (error != "")
            {
                cachedInfo.Error = error;
                ImGuiHelpBox.Draw(position, error, MessageType.Error);
                return true;
            }

            cachedInfo.Error = "";

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            AdvancedDropdownMetaInfo dropdownMetaInfo = GetSerializedActualDropdownMeta(metaInfo, valueProperty,
                isULong);

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup) &&
                dropdownMetaInfo.DropdownListValue != null)
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    dropdownMetaInfo,
                    fieldRect.width,
                    320f,
                    metaInfo.IsFlags,
                    (curItem, _) => SetSerializedActualDropdownValue(valueProperty, metaInfo, curItem, isULong,
                        onValueChanged)));
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect,
                RichTextDrawer.ParseRichXmlWithProvider(GetSerializedActualDisplay(metaInfo, valueProperty, isULong),
                    richTextTagProvider));
            return true;
        }

        private static AdvancedDropdownMetaInfo GetSerializedActualDropdownMeta(EnumMetaInfo metaInfo,
            SerializedProperty valueProperty, bool isULong)
        {
            if (metaInfo.IsFlags)
            {
                return isULong
                    ? GetSerializedActualFlagsDropdownMetaULong(metaInfo, valueProperty)
                    : GetSerializedActualFlagsDropdownMetaLong(metaInfo, valueProperty);
            }

            object curValue = GetSerializedActualEnumObject(metaInfo, valueProperty, isULong);
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

        private static AdvancedDropdownMetaInfo GetSerializedActualFlagsDropdownMetaLong(EnumMetaInfo metaInfo,
            SerializedProperty valueProperty)
        {
            long curMask = valueProperty.longValue;
            long fullMask = Convert.ToInt64(metaInfo.EverythingBit);
            AdvancedDropdownList<long> dropdown = new AdvancedDropdownList<long>
            {
                { GetNothingLabel(metaInfo), 0L },
                { GetEverythingLabel(metaInfo), fullMask },
            };
            dropdown.AddSeparator();
            foreach (EnumMetaInfo.EnumValueInfo valueInfo in metaInfo.EnumValues)
            {
                long value = Convert.ToInt64(valueInfo.Value);
                if (value == 0L || value == fullMask)
                {
                    continue;
                }

                dropdown.Add(valueInfo.Label, value);
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = GetCurrentFlagValuesLong(metaInfo, curMask),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        private static AdvancedDropdownMetaInfo GetSerializedActualFlagsDropdownMetaULong(EnumMetaInfo metaInfo,
            SerializedProperty valueProperty)
        {
#if UNITY_2022_1_OR_NEWER
            ulong curMask = valueProperty.ulongValue;
            ulong fullMask = Convert.ToUInt64(metaInfo.EverythingBit);
            AdvancedDropdownList<ulong> dropdown = new AdvancedDropdownList<ulong>
            {
                { GetNothingLabel(metaInfo), 0UL },
                { GetEverythingLabel(metaInfo), fullMask },
            };
            dropdown.AddSeparator();
            foreach (EnumMetaInfo.EnumValueInfo valueInfo in metaInfo.EnumValues)
            {
                ulong value = Convert.ToUInt64(valueInfo.Value);
                if (value == 0UL || value == fullMask)
                {
                    continue;
                }

                dropdown.Add(valueInfo.Label, value);
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = GetCurrentFlagValuesULong(metaInfo, curMask),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
#else
            return new AdvancedDropdownMetaInfo
            {
                Error = "EnumULong is not supported in this Unity version",
                CurValues = Array.Empty<object>(),
                DropdownListValue = null,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
#endif
        }

        private static IReadOnlyList<object> SetSerializedActualDropdownValue(SerializedProperty valueProperty,
            EnumMetaInfo metaInfo, object curItem, bool isULong, Action<object> onValueChanged)
        {
            if (metaInfo.IsFlags)
            {
                return isULong
                    ? SetSerializedActualDropdownFlagsULong(valueProperty, metaInfo, curItem, onValueChanged)
                    : SetSerializedActualDropdownFlagsLong(valueProperty, metaInfo, curItem, onValueChanged);
            }

            SetSerializedActualEnumValue(valueProperty, metaInfo, curItem, isULong, onValueChanged);
            return null;
        }

        private static IReadOnlyList<object> SetSerializedActualDropdownFlagsLong(SerializedProperty valueProperty,
            EnumMetaInfo metaInfo, object curItem, Action<object> onValueChanged)
        {
            long selectedValue = Convert.ToInt64(curItem);
            long newMask = selectedValue == 0L
                ? 0L
                : EnumFlagsUtil.ToggleBit(valueProperty.longValue, selectedValue);
            SetSerializedActualEnumValue(valueProperty, metaInfo, newMask, false, onValueChanged);
            return GetCurrentFlagValuesLong(metaInfo, newMask);
        }

        private static IReadOnlyList<object> SetSerializedActualDropdownFlagsULong(SerializedProperty valueProperty,
            EnumMetaInfo metaInfo, object curItem, Action<object> onValueChanged)
        {
#if UNITY_2022_1_OR_NEWER
            ulong selectedValue = Convert.ToUInt64(curItem);
            ulong newMask = selectedValue == 0UL
                ? 0UL
                : EnumFlagsUtil.ToggleBit(valueProperty.ulongValue, selectedValue);
            SetSerializedActualEnumValue(valueProperty, metaInfo, newMask, true, onValueChanged);
            return GetCurrentFlagValuesULong(metaInfo, newMask);
#else
            return Array.Empty<object>();
#endif
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

        private static IReadOnlyList<object> GetCurrentFlagValuesLong(EnumMetaInfo metaInfo, long curMask)
        {
            List<object> curValues = metaInfo.EnumValues
                .Select(each => Convert.ToInt64(each.Value))
                .Where(each => EnumFlagsUtil.IsOn(curMask, each))
                .Cast<object>()
                .ToList();
            curValues.Add(curMask);
            return curValues;
        }

        private static IReadOnlyList<object> GetCurrentFlagValuesULong(EnumMetaInfo metaInfo, ulong curMask)
        {
            List<object> curValues = metaInfo.EnumValues
                .Select(each => Convert.ToUInt64(each.Value))
                .Where(each => EnumFlagsUtil.IsOn(curMask, each))
                .Cast<object>()
                .ToList();
            curValues.Add(curMask);
            return curValues;
        }

        private static string GetSerializedActualDisplay(EnumMetaInfo metaInfo, SerializedProperty valueProperty,
            bool isULong)
        {
            if (metaInfo.IsFlags)
            {
                return isULong
#if UNITY_2022_1_OR_NEWER
                    ? GetSelectedNamesULong(metaInfo, valueProperty.ulongValue)
#else
                    ? "-"
#endif
                    : GetSelectedNamesLong(metaInfo, valueProperty.longValue);
            }

            object currentEnum = GetSerializedActualEnumObject(metaInfo, valueProperty, isULong);
            foreach ((object enumValue, string enumLabel, string enumRichLabel) in Util.GetEnumValues(metaInfo.EnumType))
            {
                if (Util.GetIsEqual(currentEnum, enumValue))
                {
                    return enumRichLabel ?? enumLabel;
                }
            }

            return currentEnum.ToString();
        }

        private static object GetSerializedActualEnumObject(EnumMetaInfo metaInfo, SerializedProperty valueProperty,
            bool isULong) =>
            Enum.ToObject(metaInfo.EnumType,
                isULong
#if UNITY_2022_1_OR_NEWER
                    ? valueProperty.ulongValue
#else
                    ? 0UL
#endif
                    : valueProperty.longValue);

        private static string GetSelectedNamesLong(EnumMetaInfo metaInfo, long selectedValue)
        {
            string[] names = metaInfo.EnumValues
                .Where(each => EnumFlagsUtil.IsOn(selectedValue, Convert.ToInt64(each.Value)))
                .Select(each => each.Label.Split('/').Last())
                .ToArray();
            return names.Length == 0 ? "-" : string.Join(",", names);
        }

        private static string GetSelectedNamesULong(EnumMetaInfo metaInfo, ulong selectedValue)
        {
            string[] names = metaInfo.EnumValues
                .Where(each => EnumFlagsUtil.IsOn(selectedValue, Convert.ToUInt64(each.Value)))
                .Select(each => each.Label.Split('/').Last())
                .ToArray();
            return names.Length == 0 ? "-" : string.Join(",", names);
        }

        private static string GetNothingLabel(EnumMetaInfo metaInfo) =>
            metaInfo.NothingValue.HasValue ? metaInfo.NothingValue.Label : "Nothing";

        private static string GetEverythingLabel(EnumMetaInfo metaInfo) =>
            metaInfo.EverythingValue.HasValue ? metaInfo.EverythingValue.Label : "Everything";

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
    }
}
