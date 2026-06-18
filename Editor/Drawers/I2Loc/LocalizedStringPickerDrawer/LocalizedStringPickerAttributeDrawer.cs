using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using I2.Loc;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.I2Loc;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

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

        private static readonly Regex SizeReg = new Regex("</?size(?:=[^>]*)?>");

        private static AdvancedDropdownMetaInfo GetMetaInfo(string curValue, bool isImGui)
        {
            List<string> terms = LocalizationManager.GetTermsList();
            Dropdown<string>
                dropdown = new Dropdown<string>(isImGui? "Pick A Term": "")
                {
                    {"[Null]", ""},
                    {"[Inferred from Text]", " "},
                };

            dropdown.AddSeparator();

            foreach (string term in terms)
            {
                string trans = LocalizationManager.GetTranslation(term) ?? "";
                bool hasExtra;
                string extraDisplay;
                ICollection<string> extraSearches;
                if (string.IsNullOrWhiteSpace(trans))
                {
                    hasExtra = false;
                    extraDisplay = "";
                    extraSearches = null;
                }
                else
                {
                    string noBrTrans = trans.Replace("\r\n", " / ").Replace("\n", " / ").Replace("<br>", " / ");
                    string pureResult = SizeReg.Replace(
                        noBrTrans,
                        string.Empty
                    );

                    hasExtra = true;
                    int pureLength = pureResult.Length;
                    extraDisplay = pureLength > 15 ? pureResult[..15] + "..." : pureResult;
                    extraSearches = new[] { pureResult };
                }

                List<string> sep = RuntimeUtil.SeparatePath(term).ToList();
                if(hasExtra)
                {
                    string last = sep[^1];
                    sep[^1] = $"{last} <color=gray>{extraDisplay}</color>";
                }

                Dropdown<string>.AddByNames(dropdown, new Queue<string>(sep), term, extraSearches: extraSearches);
            }

            dropdown.SelfCompact();

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curStack;
            if (curValue == "")
            {
                curStack = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stack, string _) =
                    AdvancedDropdownUtil.GetSelected(curValue,
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdown);
                curStack = stack;
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = new []{curValue},
                DropdownListValue = dropdown,
                SelectStacks = curStack,
            };
        }

        private static string GetCurrentValue(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return property.stringValue;
            }

            return property.FindPropertyRelative("mTerm")?.stringValue ?? "";
        }

        private static bool SetValue(SerializedProperty property, string newValue)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (property.stringValue == newValue)
                {
                    return false;
                }

                property.stringValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
                return true;
            }

            SerializedProperty mTermProp = property.FindPropertyRelative("mTerm");
            if (mTermProp == null || mTermProp.stringValue == newValue)
            {
                return false;
            }

            mTermProp.stringValue = newValue;
            property.serializedObject.ApplyModifiedProperties();
            return true;
        }

        private static void ApplySelection(SerializedProperty property, FieldInfo info, string newValue,
            Action<object> onValueChangedCallback)
        {
            if (!SetValue(property, newValue))
            {
                return;
            }

            if(property.propertyType == SerializedPropertyType.String)
            {
                onValueChangedCallback.Invoke(newValue);
                return;
            }

            object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (noCacheParent == null)
            {
                Debug.LogWarning("Property disposed unexpectedly, skip onChange callback.");
                return;
            }

            (string error, int _, object reflectedValue) = Util.GetValue(property, info, noCacheParent);
            if (error != "")
            {
                Debug.LogError(error);
                return;
            }

            onValueChangedCallback.Invoke(reflectedValue);
        }
    }
}
