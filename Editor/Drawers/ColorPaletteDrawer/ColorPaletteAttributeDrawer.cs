using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ColorPaletteAttribute), true)]
    public partial class ColorPaletteAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static Texture2D _colorPaletteIcon;
        private static Texture2D _colorPaletteWarningIcon;
        private const int ColorButtonSize = 20;

        private static AdvancedDropdownMetaInfo GetMetaInfo(IReadOnlyList<SaintsField.ColorPalette> curSelect,
            IReadOnlyList<SaintsField.ColorPalette> allColorPalette, bool isImGui)
        {
            AdvancedDropdownList<IReadOnlyList<SaintsField.ColorPalette>> dropdownListValue =
                new AdvancedDropdownList<IReadOnlyList<SaintsField.ColorPalette>>(isImGui? "Select Palette": "");
            if (allColorPalette.Count > 1)
            {
                dropdownListValue.Add("All", allColorPalette);
            }

            dropdownListValue.Add("Edit...", null);
            dropdownListValue.AddSeparator();

            IReadOnlyList<IReadOnlyList<SaintsField.ColorPalette>> curValues =
                Array.Empty<IReadOnlyList<SaintsField.ColorPalette>>();

            foreach (SaintsField.ColorPalette colorPalette in allColorPalette)
            {
                SaintsField.ColorPalette[] thisValue = { colorPalette };
                // ReSharper disable once MergeIntoPattern
                if (curSelect != null && curSelect.Count == 1 && curSelect.Contains(colorPalette))
                {
                    curValues = new[] { thisValue };
                }

                dropdownListValue.Add(colorPalette.displayName, thisValue);
            }

            // ReSharper disable once MergeIntoPattern
            if (curSelect != null && curSelect.Count > 1)
            {
                curValues = new[] { allColorPalette };
            }


            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues[0],
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
                curSelected = stacks;
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        private static bool FillColorPalettes(List<SaintsField.ColorPalette> colorPalettes,
            IReadOnlyList<ColorPaletteAttribute.ColorPaletteSource> colorPaletteSources, SerializedProperty property,
            MemberInfo info, object parent)
        {
            List<SaintsField.ColorPalette> foundColorPalettes = new List<SaintsField.ColorPalette>();
            if (colorPaletteSources.Count == 0)
            {
                foundColorPalettes.AddRange(ColorPaletteRegister.ColorPalettes);
            }
            else
            {
                foreach (ColorPaletteAttribute.ColorPaletteSource colorPaletteSource in colorPaletteSources)
                {
                    SaintsField.ColorPalette findTarget = null;
                    if (colorPaletteSource.IsCallback)
                    {
                        string callback = colorPaletteSource.Name;
                        (string error, object result) =
                            Util.GetOf<object>(callback, null, property, info, parent);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogError(error);
#endif
                        }
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        else if (result is SaintsField.ColorPalette palette)
                        {
                            findTarget = palette;
                        }
                        else if (result is string paletteName)
                        {
                            findTarget =
                                ColorPaletteRegister.ColorPalettes.FirstOrDefault(each =>
                                    each.displayName == paletteName);
                        }
                    }
                    else
                    {
                        findTarget =
                            ColorPaletteRegister.ColorPalettes.FirstOrDefault(each =>
                                each.displayName == colorPaletteSource.Name);
                    }

                    if (findTarget != null && !foundColorPalettes.Contains(findTarget))
                    {
                        foundColorPalettes.Add(findTarget);
                    }
                }
            }

            bool changed = false;

            // check add
            foreach (SaintsField.ColorPalette found in
                     foundColorPalettes.Where(found => !colorPalettes.Contains(found)))
            {
                changed = true;
                colorPalettes.Add(found);
            }

            // check delete
            foreach (SaintsField.ColorPalette exist in colorPalettes.ToArray())
            {
                // ReSharper disable once InvertIf
                if (!foundColorPalettes.Contains(exist))
                {
                    changed = true;
                    colorPalettes.Remove(exist);
                }
            }

            return changed;
        }

        private struct DisplayColorEntry
        {
            public SaintsField.ColorPalette.ColorEntry ColorEntry;
            public bool IsSelected;
            public Color ReversedColor;
        }

        private static IEnumerable<DisplayColorEntry> GetDisplayColorEntries(Color selectedColor, string searchContent, IReadOnlyList<SaintsField.ColorPalette> selectedPalettes)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (SaintsField.ColorPalette.ColorEntry colorEntry in selectedPalettes.SelectMany(each => each.colors).Where(each => string.IsNullOrEmpty(searchContent) || each.displayName.Contains(searchContent)))
            {
                Color reverseColor = ReverseColor(colorEntry.color);
                bool isSelected = selectedColor == colorEntry.color;
                yield return new DisplayColorEntry
                {
                    ColorEntry = colorEntry,
                    IsSelected = isSelected,
                    ReversedColor = reverseColor,
                };
            }
        }

        private static Color ReverseColor(Color oriColor)
        {
            Color.RGBToHSV(oriColor, out float h, out float s, out float v);
            float negativeH = (h + 0.5f) % 1f;
            return Color.HSVToRGB(negativeH, s, v);
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            // Debug.Log($"{property.propertyPath}/{property.propertyType}");
            if (property.propertyType != SerializedPropertyType.Color)
            {
                return new AutoRunnerFixerResult
                {
                    Error = "ColorPaletteAttribute can only be used on Color fields",
                    ExecError = "",
                };
            }

            ColorPaletteAttribute colorPaletteAttribute = (ColorPaletteAttribute)propertyAttribute;
            List<SaintsField.ColorPalette> allPalettes = new List<SaintsField.ColorPalette>();
            FillColorPalettes(allPalettes, colorPaletteAttribute.ColorPaletteSources, property, memberInfo, parent);
            Color selectedColor = property.colorValue;
            bool anySelected = allPalettes.Any(eachPalettes =>
                eachPalettes.colors.Any(eachColorEntry => eachColorEntry.color == selectedColor));
            return anySelected
                ? null
                : new AutoRunnerFixerResult
                {
                    Error =
                        $"Color not found in any of the selected ColorPalettes: {string.Join(", ", allPalettes.Select(each => each.displayName))}",
                    ExecError = "",
                };
        }
    }
}
