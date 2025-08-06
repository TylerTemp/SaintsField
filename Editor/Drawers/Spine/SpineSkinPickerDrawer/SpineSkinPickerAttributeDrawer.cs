using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineSkinPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineSkinPickerAttribute), true)]
    public partial class SpineSkinPickerAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {


        private static string Validate(string callback, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                return "";
            }

            (string error, ExposedList<Skin> skins) = SpineSkinUtils.GetSkins(callback, property, info, parent);
            if (error != "")
            {
                return error;
            }

            return skins.Any(skin => skin.Name == property.stringValue)
                ? ""
                : $"{property.stringValue} is not a valid skin: {string.Join(", ", skins)}";
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string curValue, ExposedList<Skin> skins, bool isImGui)
        {
            AdvancedDropdownList<string> dropdownListValue =
                new AdvancedDropdownList<string>(isImGui? "Select Skin": "")
                {
                    { "[Empty String]", "" },
                };

            dropdownListValue.AddSeparator();

            object[] curValues = { curValue };

            foreach (Skin skin in skins)
            {
                dropdownListValue.Add(skin.Name, skin.Name, icon: SpineSkinUtils.IconPath);
            }

            // if (curValues.Length == 0)
            // {
            //     curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            // }
            // else
            // {
            (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                AdvancedDropdownUtil.GetSelected(curValues[0],
                    Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);

            // }
            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = stacks,
            };
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            SpineSkinPickerAttribute spineSkinPickerAttribute = (SpineSkinPickerAttribute)propertyAttribute;
            string error = Validate(spineSkinPickerAttribute.SkeletonTarget, property, memberInfo, parent);
            return error == ""
                ? null
                : new AutoRunnerFixerResult
                {
                    ExecError = "",
                    Error = error,
                };
        }
    }
}
