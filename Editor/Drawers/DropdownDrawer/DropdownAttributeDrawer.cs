#if UNITY_2021_3_OR_NEWER
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public partial class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";
        private const BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                      BindingFlags.Public | BindingFlags.DeclaredOnly;

        private static void ShowGenericMenu(MetaInfo metaInfo, string curDisplay, Rect fieldRect, Action<string, object> onSelect, bool hackSlashNoSub)
        {
            // create the menu and add items to it
            GenericMenu menu = new GenericMenu();

            Debug.Assert(metaInfo.DropdownListValue != null);
            foreach ((string curName, object curItem, bool disabled, bool curIsSeparator) in metaInfo.DropdownListValue)
            {
                string replacedCurName = curName.Replace('/', '\u2215');
                if (curIsSeparator)
                {
                    menu.AddSeparator(hackSlashNoSub? "": curName);
                }
                else if (disabled)
                {
                    // Debug.Log($"disabled: {curName}");
                    menu.AddDisabledItem(new GUIContent(hackSlashNoSub? replacedCurName: curName), curName == curDisplay);
                }
                else
                {
                    menu.AddItem(new GUIContent(hackSlashNoSub? replacedCurName: curName), curName == curDisplay, () => onSelect(curName, curItem));
                }
            }

            // display the menu
            // menu.ShowAsContext();
            menu.DropDown(fieldRect);
        }


        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public IReadOnlyList<ValueTuple<string, object, bool, bool>> DropdownListValue;
            public int SelectedIndex;
            // ReSharper enable InconsistentNaming

            public override string ToString() =>
                $"MetaInfo(index={SelectedIndex}, items={string.Join(",", DropdownListValue.Select(each => each.Item1))}";
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo field,
            object parentObj)
        {
            Debug.Assert(field != null);
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;


            string error;
            IDropdownList dropdownListValue = null;
            if (dropdownAttribute.FuncName == null)
            {
                Type enumType = ReflectUtils.GetElementType(field.FieldType);
                if(enumType.IsEnum)
                {
                    Array enumValues = Enum.GetValues(enumType);
                    DropdownList<object> enumDropdown = new DropdownList<object>();
                    foreach (object enumValue in enumValues)
                    {
                        enumDropdown.Add(ReflectUtils.GetRichLabelFromEnum(enumType, enumValue).value, enumValue);
                    }

                    error = "";
                    dropdownListValue = enumDropdown;
                }
                else
                {
                    error = $"{property.displayName}({enumType}) is not a enum";
                }
            }
            else
            {
                (string getOfError, IDropdownList getOfDropdownListValue) =
                    Util.GetOf<IDropdownList>(dropdownAttribute.FuncName, null, property, field, parentObj);
                error = getOfError;
                dropdownListValue = getOfDropdownListValue;
            }
            if(dropdownListValue == null || error != "")
            {
                return new MetaInfo
                {
                    Error = error == ""? $"dropdownList is null from `{dropdownAttribute.FuncName}` on target `{parentObj}`": error,
                    SelectedIndex = -1,
                    DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                };
            }

            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            (string curError, int _, object curValue) = Util.GetValue(property, field, parentObj);
            if (curError != "")
            {
                return new MetaInfo
                {
                    Error = curError,
                    SelectedIndex = -1,
                    DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                };
            }

            if (curValue is IWrapProp wrapProp)
            {
                curValue = Util.GetWrapValue(wrapProp);
            }
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            // string curDisplay = "";
            Debug.Assert(dropdownListValue != null);

            (string uniqueError, IDropdownList dropdownListValueUnique) = GetUniqueList(dropdownListValue, dropdownAttribute.EUnique, curValue, property, field, parentObj);

            if (uniqueError != "")
            {
                return new MetaInfo
                {
                    Error = curError,
                    SelectedIndex = -1,
                    DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                };
            }

            int selectedIndex = -1;

            IReadOnlyList<(string, object, bool, bool)> dropdownActualList = dropdownListValueUnique.ToArray();

            foreach (int dropdownIndex in Enumerable.Range(0, dropdownActualList.Count))
            {
                (string _, object itemValue, bool _, bool isSeparator) = dropdownActualList[dropdownIndex];
                if (isSeparator)
                {
                    continue;
                }

                if (Util.GetIsEqual(curValue, itemValue))
                {
                    selectedIndex = dropdownIndex;
                    break;
                }
            }

            return new MetaInfo
            {
                Error = "",
                DropdownListValue = dropdownActualList,
                SelectedIndex = selectedIndex,
            };
        }

        private static (string uniqueError, IDropdownList dropdownListValueUnique) GetUniqueList(IDropdownList dropdownListValue, EUnique eUnique, object curValue, SerializedProperty property, FieldInfo info, object parent)
        {
            if(eUnique == EUnique.None)
            {
                return ("", dropdownListValue);
            }

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (arrayIndex == -1)
            {
                return ("", dropdownListValue);
            }

            (SerializedProperty arrProp, int _, string error) = Util.GetArrayProperty(property, info, parent);
            if (error != "")
            {
                return (error, null);
            }

            List<object> existsValues = new List<object>();

            foreach (SerializedProperty element in Enumerable.Range(0, arrProp.arraySize).Where(index => index != arrayIndex).Select(arrProp.GetArrayElementAtIndex))
            {
                (string otherError, int _, object otherValue) = Util.GetValue(element, info, parent);
                if (otherError != "")
                {
                    return (otherError, null);
                }

                if (otherValue is IWrapProp wrapProp)
                {
                    otherValue = Util.GetWrapValue(wrapProp);
                }

                existsValues.Add(otherValue);
            }

            DropdownList<object> newResult = new DropdownList<object>();
            foreach ((string name, object value, bool disabled, bool separator) eachValue in dropdownListValue)
            {
                bool exists = existsValues.Any(each => Util.GetIsEqual(each, eachValue.value));
                if (!exists)
                {
                    newResult.Add(eachValue);
                }
                else if (eUnique == EUnique.Disable)
                {
                    newResult.Add((eachValue.name, eachValue.value, true, eachValue.separator));
                }
                else if (eUnique == EUnique.Remove)
                {
                    // if it's the value from other element, then just disable it rather than remove it
                    if(Util.GetIsEqual(curValue, eachValue.value))
                    {
                        newResult.Add((eachValue.name, eachValue.value, true, eachValue.separator));
                    }
                }
            }

            return ("", newResult);
        }
    }
}
