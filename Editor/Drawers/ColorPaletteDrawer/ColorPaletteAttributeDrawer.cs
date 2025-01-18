using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
    [CustomPropertyDrawer(typeof(ColorPaletteAttribute))]
    public partial class ColorPaletteAttributeDrawer: SaintsPropertyDrawer
    {
        private static AdvancedDropdownMetaInfo GetMetaInfo(IReadOnlyList<SaintsField.ColorPalette> curSelect, IReadOnlyList<SaintsField.ColorPalette> allColorPalette)
        {
            AdvancedDropdownList<IReadOnlyList<SaintsField.ColorPalette>> dropdownListValue =
                new AdvancedDropdownList<IReadOnlyList<SaintsField.ColorPalette>>();
            if (allColorPalette.Count > 1)
            {
                dropdownListValue.Add("All", allColorPalette);
            }
            dropdownListValue.Add("Edit...", null);
            dropdownListValue.AddSeparator();

            IReadOnlyList<IReadOnlyList<SaintsField.ColorPalette>> curValues = Array.Empty<IReadOnlyList<SaintsField.ColorPalette>>();

            foreach (SaintsField.ColorPalette colorPalette in allColorPalette)
            {
                SaintsField.ColorPalette[] thisValue = { colorPalette };
                // ReSharper disable once MergeIntoPattern
                if (curSelect != null && curSelect.Count == 1 && curSelect.Contains(colorPalette))
                {
                    curValues = new[]{thisValue};
                }
                dropdownListValue.Add(colorPalette.displayName, thisValue);
            }
            // ReSharper disable once MergeIntoPattern
            if(curSelect != null && curSelect.Count > 1)
            {
                curValues = new[]{allColorPalette};
            }


            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) = AdvancedDropdownUtil.GetSelected(curValues[0], Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
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
    }
}
