using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

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

        private const float DefaultSepHeight = 4f;
        private const float TitleHeight = AdvancedDropdownAttribute.TitleHeight;

        public static Vector2 GetSizeIMGUI(IAdvancedDropdownList dropdownListValue, float positionWidth)
        {
            float maxChildCount = GetDropdownPageHeight(dropdownListValue, EditorGUIUtility.singleLineHeight, DefaultSepHeight).Max();
            return new Vector2(positionWidth, maxChildCount + TitleHeight);
        }

        public static IEnumerable<float> GetDropdownPageHeight(IAdvancedDropdownList dropdownList, float itemHeight, float sepHeight)
        {
            if (dropdownList.ChildCount() == 0)
            {
                // Debug.Log($"yield 0");
                yield return 0;
                yield break;
            }

            // Debug.Log($"yield {dropdownList.children.Count}");
            yield return dropdownList.ChildCount() * itemHeight + dropdownList.SepCount() * sepHeight;
            foreach (IEnumerable<float> eachChildHeight in dropdownList.children.Select(child => GetDropdownPageHeight(child, itemHeight, sepHeight)))
            {
                foreach (int i in eachChildHeight)
                {
                    yield return i;
                }
            }
        }
    }
}
