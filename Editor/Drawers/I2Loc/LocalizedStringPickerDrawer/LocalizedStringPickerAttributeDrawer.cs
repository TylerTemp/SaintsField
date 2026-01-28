using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.I2Loc;
using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.I2Loc.LocalizedStringPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(LocalizedStringPickerAttribute), true)]
    public partial class LocalizedStringPickerAttributeDrawer: SaintsPropertyDrawer
    {
        private static string MismatchError(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return "";
            }

            return property.FindPropertyRelative("mTerm") == null
                ? $"Expect string or LocalizedString type, got {property.propertyType}"
                : "";
        }

        private static AdvancedDropdownList<string> _dropdownListCache;

        private static AdvancedDropdownMetaInfo GetMetaInfo(string curValue, bool isImGui)
        {
            if (_dropdownListCache == null)
            {
                List<string> terms = LocalizationManager.GetTermsList();
                AdvancedDropdownList<string>
                    advancedDropdownList = new AdvancedDropdownList<string>(isImGui? "Pick A Term": "")
                    {
                        {"[Null]", ""},
                        {"[Inferred from Text]", " "},
                    };

                advancedDropdownList.AddSeparator();

                foreach (string term in terms)
                {
                    string trans = LocalizationManager.GetTranslation(term) ?? "";
                    string noBrTrans = trans.Replace("\r\n", "↵").Replace("\n", "↵");
                    // string trans = "";

                    List<string> sep = RuntimeUtil.SeparatePath(term).ToList();
                    string last = sep[^1];
                    sep[^1] = $"{last} <color=gray>{noBrTrans}</color>";

                    // string displayName = $"{term} <color=gray>{noBrTrans}</color>";
                    // advancedDropdownList.Add(displayName, term);
                    AdvancedDropdownList<string>.AddByNames(advancedDropdownList, new Queue<string>(sep), term);
                }

                _dropdownListCache = advancedDropdownList;
            }

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curStack;
            if (curValue == "")
            {
                curStack = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stack, string _) = AdvancedDropdownUtil.GetSelected(curValue, Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), _dropdownListCache);
                curStack = stack;
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                // CurDisplay = display,
                CurValues = new []{curValue},
                DropdownListValue = _dropdownListCache,
                SelectStacks = curStack,
            };
        }

        private static void SetValue(SerializedProperty property, string newValue)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty mTermProp = property.FindPropertyRelative("mTerm");
            mTermProp.stringValue = newValue;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
