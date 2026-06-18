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

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaMaskDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute), true)]
    public partial class NavMeshAreaMaskAttributeDrawer: SaintsPropertyDrawer
    {
        private enum SpecialDropdownPick
        {
            Value,
            Everything,
            Nothing,
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

        private static AdvancedDropdownMetaInfo GetDropdownMetaInfo(int curValue)
        {
            AdvancedDropdownList<DropdownItem> dropdown = new AdvancedDropdownList<DropdownItem>();
            AiNavigationUtils.NavMeshArea[] allAreas = AiNavigationUtils.GetNavMeshAreas().ToArray();

            List<DropdownItem> selectedItems = new List<DropdownItem>();

            int allMask = allAreas.Aggregate(0, (current, area) => current | area.Mask);
            AiNavigationUtils.NavMeshArea allArea = new AiNavigationUtils.NavMeshArea
            {
                Name = "[Everything]",
                Value = int.MaxValue,
                Mask = allMask,
            };
            DropdownItem allItem = new DropdownItem(allArea, SpecialDropdownPick.Everything);
            dropdown.Add($"<b>Everything</b> <color=#808080>({allMask})</color>", allItem);
            if ((curValue & allMask) == allMask)
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
            dropdown.Add("<b>Nothing</b>", noItem);
            if (curValue == 0)
            {
                selectedItems.Add(noItem);
            }

            dropdown.AddSeparator();

            foreach (AiNavigationUtils.NavMeshArea area in allAreas)
            {
                DropdownItem dropdownItem = new DropdownItem(area, SpecialDropdownPick.Value);
                dropdown.Add($"{area.Name} <color=#808080>({area.Mask})</color>", dropdownItem);
                if ((area.Mask & curValue) != 0)
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

        private static string GetDisplay(int curValue)
        {
            AiNavigationUtils.NavMeshArea[] allAreas = AiNavigationUtils.GetNavMeshAreas().ToArray();
            if (curValue == 0)
            {
                return "Nothing";
            }

            int allMask = allAreas.Aggregate(0, (current, area) => current | area.Mask);
            if ((curValue & allMask) == allMask)
            {
                return "Everything";
            }

            string[] matched = allAreas
                .Where(area => (curValue & area.Mask) != 0)
                .Select(area => area.Name)
                .ToArray();

            return matched.Length == 0
                ? $"? {curValue}"
                : string.Join(", ", matched);
        }

        private static IReadOnlyList<object> ApplyDropdownSelection(SerializedProperty property,
            FieldInfo info, object parent, DropdownItem curValue, bool isOn, Action<object> onValueChangedCallback)
        {
            switch (curValue.Pick)
            {
                case SpecialDropdownPick.OpenSettings:
                    NavMeshEditorHelpers.OpenAreaSettings();
                    return null;
                case SpecialDropdownPick.Everything:
                    SetValue(property, info, parent, isOn ? curValue.Area.Mask : 0, onValueChangedCallback);
                    break;
                case SpecialDropdownPick.Nothing:
                    SetValue(property, info, parent, 0, onValueChangedCallback);
                    break;
                case SpecialDropdownPick.Value:
                {
                    int oldMask = property.intValue;
                    int useValue = isOn
                        ? oldMask | curValue.Area.Mask
                        : EnumFlagsUtil.SetOffBit(oldMask, curValue.Area.Mask);
                    SetValue(property, info, parent, useValue, onValueChangedCallback);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(curValue.Pick), curValue.Pick, null);
            }

            return GetDropdownMetaInfo(property.intValue).CurValues;
        }

        private static void SetValue(SerializedProperty property, FieldInfo info, object parent, int value,
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
    }
}
