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
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
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

        private static IReadOnlyList<ColorPaletteArray.ColorInfo> FillColorPalettes(
            IReadOnlyList<ColorPaletteAttribute.ColorPaletteSource> colorPaletteSources, SerializedProperty property,
            MemberInfo info, object parent)
        {
            ColorPaletteArray allPalette = EnsureColorPaletteArray();

            List<ColorPaletteArray.ColorInfo> foundColorPalettes = new List<ColorPaletteArray.ColorInfo>();
            if (colorPaletteSources.Count == 0)
            {
                foundColorPalettes.AddRange(allPalette);
            }
            else
            {
                foreach (ColorPaletteAttribute.ColorPaletteSource colorPaletteSource in colorPaletteSources)
                {
                    // List<ColorPaletteArray.ColorInfo> findTargets = new List<ColorPaletteArray.ColorInfo>();
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
                        else if (result is string paletteName)
                        {
                            foundColorPalettes.AddRange(allPalette.Where(each => each.labels.Contains(paletteName)));
                        }
                        else if(result is IEnumerable<string> paletteNames)
                        {
                            foreach (string name in paletteNames)
                            {
                                foundColorPalettes.AddRange(allPalette.Where(each => each.labels.Contains(name)));
                            }
                        }
                    }
                    else
                    {
                        foundColorPalettes.AddRange(allPalette.Where(colorInfo => colorInfo.labels.Contains(colorPaletteSource.Name)));
                    }
                }
            }

            return foundColorPalettes;
        }

        private struct DisplayColorEntry
        {
            public ColorPaletteArray.ColorInfo ColorEntry;
            public bool IsSelected;
            public Color ReversedColor;
        }

        private static IEnumerable<DisplayColorEntry> GetDisplayColorEntries(Color selectedColor, string searchContent, IReadOnlyList<ColorPaletteArray.ColorInfo> selectedPalettes)
        {
            string[] lowerSearchs = searchContent.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (ColorPaletteArray.ColorInfo colorEntry in selectedPalettes.Where(each => ColorInfoSearch(each, lowerSearchs)))
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

        private static bool ColorInfoSearch(ColorPaletteArray.ColorInfo colorInfo, string[] searchContent)
        {
            if (searchContent.Length == 0)
            {
                return true;
            }

            string[] lowLables = colorInfo.labels.Select(each => each.ToLower()).ToArray();
            foreach (string searchSeg in searchContent)
            {
                if (searchSeg.StartsWith("#"))
                {
                    if (ColorUtility.TryParseHtmlString(searchSeg, out Color color))
                    {
                        if (colorInfo.color != color)
                        {
                            return false;
                        }
                    }
                    else if(Array.IndexOf(lowLables, searchSeg) == -1)
                    {
                        return false;
                    }
                }


                if (Array.IndexOf(lowLables, searchSeg) == -1)
                {
                    return false;
                }
            }

            return true;
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
            // FillColorPalettes(allPalettes, colorPaletteAttribute.ColorPaletteSources, property, memberInfo, parent);
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
