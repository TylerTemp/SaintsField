using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.Core;
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

        private struct DisplayColorEntry
        {
            public ColorPaletteArray.ColorInfo ColorEntry;
            public bool IsSelected;
            public Color ReversedColor;
        }

        private static IEnumerable<DisplayColorEntry> GetDisplayColorEntries(Color selectedColor, string searchContent, IReadOnlyList<ColorPaletteArray.ColorInfo> selectedPalettes)
        {
            string[] lowerSearchs = searchContent.ToLower().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
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
                        if (ColorUtility.ToHtmlStringRGBA(colorInfo.color) != ColorUtility.ToHtmlStringRGBA(color))
                        {
                            return false;
                        }
                    }
                    else if(lowLables.All(lowLabel => !lowLabel.Contains(searchSeg)))
                    {
                        return false;
                    }

                    continue;
                }


                if (lowLables.All(lowLabel => !lowLabel.Contains(searchSeg)))
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

        private const string ErrorMessageMissingPalette =
            "No Color Palette found. Please go `Window` - `Saints` - `Color Palette` to create one.";

        private static ColorPaletteArray _colorPaletteArray;

        private static ColorPaletteArray EnsureColorPaletteArray()
        {
            if (_colorPaletteArray)
            {
                return _colorPaletteArray;
            }

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ColorPaletteArray).FullName);
            if (guids.Length == 0)
            {
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return _colorPaletteArray = AssetDatabase.LoadAssetAtPath<ColorPaletteArray>(path);
        }

        private static (HashSet<string> labels, IEnumerable<ColorPaletteArray.ColorInfo> colorInfos) FilterOutColorInfo(ColorPaletteAttribute colorPaletteAttribute,
            SerializedProperty property,
            MemberInfo info, object parent)
        {
            // ColorPaletteArray colorPaletteArray = EnsureColorPaletteArray();
            if (!_colorPaletteArray)
            {
                return (new HashSet<string>(), Array.Empty<ColorPaletteArray.ColorInfo>());
            }

            HashSet<string> labels = GetAttributeLabels(colorPaletteAttribute, property, info, parent);

            if (labels.Count == 0)
            {
                return (labels, _colorPaletteArray);
            }

            return (labels, _colorPaletteArray
                .Where(each => labels.All(checkLabel => each.labels.Contains(checkLabel))));
        }

        private static HashSet<string> GetAttributeLabels(ColorPaletteAttribute colorPaletteAttribute,
            SerializedProperty property,
            MemberInfo info, object parent)
        {
            HashSet<string> foundLabels = new HashSet<string>();
            foreach (ColorPaletteAttribute.ColorPaletteSource colorPaletteSource in colorPaletteAttribute.ColorPaletteSources)
            {
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
                    else if (result is string label)
                    {
                        foundLabels.Add(label);
                    }
                    else if (result is IEnumerable<string> labels)
                    {
                        foundLabels.UnionWith(labels);
                    }
#if SAINTSFIELD_DEBUG
                    else
                    {
                        Debug.LogWarning($"not supported type {result}");
                    }
#endif
                }
                else
                {
                    foundLabels.Add(colorPaletteSource.Name);
                }
            }

            return foundLabels;
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            if (!EnsureColorPaletteArray())
            {
                return new AutoRunnerFixerResult
                {
                    Error = ErrorMessageMissingPalette,
                    ExecError = "",
                };
            }

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
            (HashSet<string> _, IEnumerable<ColorPaletteArray.ColorInfo> colorInfos) = FilterOutColorInfo(colorPaletteAttribute, property, memberInfo, parent);

            // List<ColorPaletteArray.ColorInfo> allPalettes = new List<ColorPaletteArray.ColorInfo>();
            // FillColorPalettes(allPalettes, colorPaletteAttribute.ColorPaletteSources, property, memberInfo, parent);
            Color selectedColor = property.colorValue;

            bool anySelected = colorInfos.Any(eachPalettes =>
                eachPalettes.color == selectedColor);
            return anySelected
                ? null
                : new AutoRunnerFixerResult
                {
                    Error =
                        "Color not found in any of the selected ColorPalettes",
                    ExecError = "",
                };
        }
    }
}
