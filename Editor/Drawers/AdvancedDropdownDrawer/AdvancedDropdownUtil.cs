using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public static class AdvancedDropdownUtil
    {
        public static (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stack, string display) GetSelected(object curValue, IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curStacks, IAdvancedDropdownList dropdownPage)
        {
            foreach ((IAdvancedDropdownList item, int index) in dropdownPage.children.WithIndex())
            {
                if (item.isSeparator)
                {
                    continue;
                }

                if (item.children.Count > 0)  // it's a group
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                    Debug.Log($"GetSelected group {dropdownPage.displayName}");
#endif
                    (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> subResult, string display) = GetSelected(curValue, curStacks.Append(new AdvancedDropdownAttributeDrawer.SelectStack
                    {
                        Display = dropdownPage.displayName,
                        Index = index,
                    }).ToArray(), item);
                    if (subResult.Count > 0)
                    {
                        return (subResult, display);
                    }

                    continue;
                }

                IEnumerable<AdvancedDropdownAttributeDrawer.SelectStack> thisLoopResult = curStacks.Append(new AdvancedDropdownAttributeDrawer.SelectStack
                {
                    Display = dropdownPage.displayName,
                    Index = index,
                });

                if (curValue is IWrapProp wrapProp)
                {
                    curValue = Util.GetWrapValue(wrapProp);
                }

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (Util.GetIsEqual(curValue, item.value))
                {
                    return (thisLoopResult.ToArray(), item.displayName);
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
                Debug.Log($"Not Equal: {curValue} != {item.value}");
#endif
            }

            // Debug.Log($"GetSelected end in empty");
            // nothing selected
            return (Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), "");
        }
    }
}
