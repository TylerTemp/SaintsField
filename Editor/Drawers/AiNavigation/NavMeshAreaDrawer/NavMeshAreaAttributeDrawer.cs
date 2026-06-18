using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.AiNavigation;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute), true)]
    public partial class NavMeshAreaAttributeDrawer: SaintsPropertyDrawer
    {
        private enum ValueType
        {
            Mask,
            Index,
            String,
        }

        private static ValueType GetValueType(SerializedProperty property, NavMeshAreaAttribute navMeshAreaAttribute)
        {
            if(property.propertyType == SerializedPropertyType.Integer)
            {
                return navMeshAreaAttribute.IsMask
                    ? ValueType.Mask
                    : ValueType.Index;
            }
            return ValueType.String;
        }

        private static string FormatAreaName(AiNavigationUtils.NavMeshArea area, ValueType valueType) =>
            $"{(valueType == ValueType.Mask ? area.Mask : area.Value)}: {area.Name}";

        private enum SpecialDropdownPick
        {
            Value,
            Everything,
            Nothing,
            EmptyString,
            OpenSettings,
        }

        private readonly struct DropdownItem: IEquatable<DropdownItem>
        {
            public readonly AiNavigationUtils.NavMeshArea Area;
            public readonly SpecialDropdownPick Pick;

            public DropdownItem(AiNavigationUtils.NavMeshArea area, SpecialDropdownPick pick)
            {
                Area = area;
                Pick = pick;
            }

            public DropdownItem(SpecialDropdownPick pick)
            {
                Area = default;
                Pick = pick;
            }

            public bool Equals(DropdownItem other) =>
                Pick == other.Pick
                && Area.Value == other.Area.Value
                && Area.Mask == other.Area.Mask
                && Area.Name == other.Area.Name;

            public override bool Equals(object obj) => obj is DropdownItem other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (int)Pick;
                    hashCode = (hashCode * 397) ^ Area.Value;
                    hashCode = (hashCode * 397) ^ Area.Mask;
                    hashCode = (hashCode * 397) ^ (Area.Name != null ? Area.Name.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private static bool IsMatchedInt(NavMeshAreaAttribute navMeshAreaAttribute, AiNavigationUtils.NavMeshArea area, int value)
        {
            return navMeshAreaAttribute.IsMask
                ? (area.Mask & value) != 0
                : area.Value == value;
        }

        private static AdvancedDropdownMetaInfo GetDropdownMetaInfo(NavMeshAreaAttribute navMeshAreaAttribute,
            SerializedProperty property)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            AdvancedDropdownList<DropdownItem> dropdown = new AdvancedDropdownList<DropdownItem>();
            if (isString)
            {
                dropdown.Add("[Empty String]", new DropdownItem(SpecialDropdownPick.EmptyString));
                dropdown.AddSeparator();
            }

            AiNavigationUtils.NavMeshArea[] allAreas = AiNavigationUtils.GetNavMeshAreas().ToArray();

            List<DropdownItem> selectedItems = new List<DropdownItem>();

            if (!isString && navMeshAreaAttribute.IsMask)
            {
                int allMask = allAreas.Aggregate(0, (current, area) => current | area.Mask);
                AiNavigationUtils.NavMeshArea allArea = new AiNavigationUtils.NavMeshArea
                {
                    Name = "[Everything]",
                    Value = int.MaxValue,
                    Mask = allMask,
                };
                DropdownItem allItem = new DropdownItem(allArea, SpecialDropdownPick.Everything);
                dropdown.Add($"<color=yellow>Everything</color> <color=#808080>({allMask})</color>", allItem);
                if ((property.intValue & allMask) == allMask)
                {
                    selectedItems.Add(allItem);
                }

                AiNavigationUtils.NavMeshArea noArea = new AiNavigationUtils.NavMeshArea
                {
                    Name = "[Nothing]",
                    Value = int.MinValue,
                    Mask = 0,
                };
                DropdownItem noItem = new DropdownItem(noArea, SpecialDropdownPick.Nothing);
                dropdown.Add("<color=yellow>Nothing</color>", noItem);
                if (property.intValue == 0)
                {
                    selectedItems.Add(noItem);
                }

                dropdown.AddSeparator();
            }

            foreach (AiNavigationUtils.NavMeshArea area in allAreas)
            {
                DropdownItem dropdownItem = new DropdownItem(area, SpecialDropdownPick.Value);
                string displayName;
                if (isString)
                {
                    displayName = area.Name;
                }
                else if (navMeshAreaAttribute.IsMask)
                {
                    displayName = $"{area.Name} <color=#808080>({area.Mask})</color>";
                }
                else
                {
                    displayName = $"{area.Name} <color=#808080>({area.Value})</color>";
                }
                dropdown.Add(displayName, dropdownItem);
                if (isString && area.Name == property.stringValue
                    || !isString && IsMatchedInt(navMeshAreaAttribute, area, property.intValue))
                {
                    selectedItems.Add(dropdownItem);
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Open Area Settings...", new DropdownItem(SpecialDropdownPick.OpenSettings), false, "d_editicon.sml");

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = selectedItems.Cast<object>().ToArray(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        private static string GetDisplay(NavMeshAreaAttribute navMeshAreaAttribute, SerializedProperty property)
        {
            AiNavigationUtils.NavMeshArea[] allAreas = AiNavigationUtils.GetNavMeshAreas().ToArray();
            if (property.propertyType == SerializedPropertyType.String)
            {
                foreach (AiNavigationUtils.NavMeshArea area in allAreas)
                {
                    if (area.Name == property.stringValue)
                    {
                        return $"{area.Name} ({area.Value})";
                    }
                }

                return string.IsNullOrEmpty(property.stringValue)
                    ? "-"
                    : $"? {property.stringValue}";
            }

            if (navMeshAreaAttribute.IsMask)
            {
                int value = property.intValue;
                if (value == 0)
                {
                    return "Nothing";
                }

                int allMask = allAreas.Aggregate(0, (current, area) => current | area.Mask);
                if ((value & allMask) == allMask)
                {
                    return "Everything";
                }

                string[] matched = allAreas
                    .Where(area => (value & area.Mask) != 0)
                    .Select(area => area.Name)
                    .ToArray();

                return matched.Length == 0
                    ? $"? {value}"
                    : string.Join(", ", matched);
            }

            foreach (AiNavigationUtils.NavMeshArea area in allAreas)
            {
                if (area.Value == property.intValue)
                {
                    return $"{area.Name} ({area.Value})";
                }
            }

            return $"? {property.intValue}";
        }

        private static IReadOnlyList<object> ApplyDropdownSelection(NavMeshAreaAttribute navMeshAreaAttribute,
            SerializedProperty property, FieldInfo info, object parent, DropdownItem curValue, bool isOn,
            Action<object> onValueChangedCallback)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            switch (curValue.Pick)
            {
                case SpecialDropdownPick.EmptyString:
                    Debug.Assert(isString);
                    SetStringValue(property, info, parent, "", onValueChangedCallback);
                    break;
                case SpecialDropdownPick.OpenSettings:
                    NavMeshEditorHelpers.OpenAreaSettings();
                    return null;
                case SpecialDropdownPick.Everything:
                    SetIntValue(property, info, parent, isOn ? curValue.Area.Mask : 0, onValueChangedCallback);
                    break;
                case SpecialDropdownPick.Nothing:
                    SetIntValue(property, info, parent, 0, onValueChangedCallback);
                    break;
                case SpecialDropdownPick.Value:
                    if (isString)
                    {
                        SetStringValue(property, info, parent, curValue.Area.Name, onValueChangedCallback);
                    }
                    else
                    {
                        int useValue = curValue.Area.Value;
                        if (navMeshAreaAttribute.IsMask)
                        {
                            int oldMask = property.intValue;
                            useValue = isOn
                                ? oldMask | curValue.Area.Mask
                                : EnumFlagsUtil.SetOffBit(oldMask, curValue.Area.Mask);
                        }
                        SetIntValue(property, info, parent, useValue, onValueChangedCallback);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(curValue.Pick), curValue.Pick, null);
            }

            return !isString && navMeshAreaAttribute.IsMask
                ? GetDropdownMetaInfo(navMeshAreaAttribute, property).CurValues
                : null;
        }

        private static void SetIntValue(SerializedProperty property, FieldInfo info, object parent, int value,
            Action<object> onValueChangedCallback)
        {
            if (property.intValue == value)
            {
                return;
            }

            property.intValue = value;
            property.serializedObject.ApplyModifiedProperties();
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, value);
            onValueChangedCallback.Invoke(value);
        }

        private static void SetStringValue(SerializedProperty property, FieldInfo info, object parent, string value,
            Action<object> onValueChangedCallback)
        {
            if (property.stringValue == value)
            {
                return;
            }

            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, value);
            onValueChangedCallback.Invoke(value);
        }
    }
}
