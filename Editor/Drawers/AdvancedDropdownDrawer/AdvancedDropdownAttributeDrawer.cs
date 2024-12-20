using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    [CustomPropertyDrawer(typeof(AdvancedDropdownAttribute))]
    public partial class AdvancedDropdownAttributeDrawer: SaintsPropertyDrawer
    {

        #region Util

        public struct SelectStack : IEquatable<SelectStack>
        {
            // ReSharper disable InconsistentNaming
            public int Index;
            public string Display;
            // public object Value;
            // ReSharper enable InconsistentNaming
            public bool Equals(SelectStack other)
            {
                return Index == other.Index && Display == other.Display;
            }

            public override bool Equals(object obj)
            {
                return obj is SelectStack other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(Index, Display);
            }
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(SerializedProperty property, AdvancedDropdownAttribute advancedDropdownAttribute, FieldInfo field, object parentObj)
        {
            string funcName = advancedDropdownAttribute.FuncName;

            string error;
            IAdvancedDropdownList dropdownListValue = null;
            if (funcName is null)
            {
                Type enumType = ReflectUtils.GetElementType(field.FieldType);
                if(enumType.IsEnum)
                {
                    Array enumValues = Enum.GetValues(enumType);
                    AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>();
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
                (string getOfError, IAdvancedDropdownList getOfDropdownListValue) =
                    Util.GetOf<IAdvancedDropdownList>(funcName, null, property, field, parentObj);
                error = getOfError;
                dropdownListValue = getOfDropdownListValue;
            }
            if(dropdownListValue == null || error != "")
            {
                return new AdvancedDropdownMetaInfo
                {
                    Error = error == ""? $"dropdownList is null from `{funcName}` on target `{parentObj}`": error,
                    CurDisplay = "[Error]",
                    CurValues = Array.Empty<object>(),
                    DropdownListValue = null,
                    SelectStacks = Array.Empty<SelectStack>(),
                };
            }

            #region Get Cur Value

            (string curError, int _, object curValue)  = Util.GetValue(property, field, parentObj);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
            Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
#endif
            if (curError != "")
            {
                return new AdvancedDropdownMetaInfo
                {
                    Error = curError,
                    CurDisplay = "[Error]",
                    CurValues = Array.Empty<object>(),
                    DropdownListValue = null,
                    SelectStacks = Array.Empty<SelectStack>(),
                };
            }
            if (curValue is IWrapProp wrapProp)
            {
                curValue = Util.GetWrapValue(wrapProp);
            }

            // process the unique options
            (string uniqueError, IAdvancedDropdownList dropdownListValueUnique) = GetUniqueList(dropdownListValue, advancedDropdownAttribute.EUnique, curValue, property, field, parentObj);

            if (uniqueError != "")
            {
                return new AdvancedDropdownMetaInfo
                {
                    Error = curError,
                    CurDisplay = "[Error]",
                    CurValues = Array.Empty<object>(),
                    DropdownListValue = null,
                    SelectStacks = Array.Empty<SelectStack>(),
                };
            }

            // string curDisplay = "";
            (IReadOnlyList<SelectStack> curSelected, string display) = AdvancedDropdownUtil.GetSelected(curValue, Array.Empty<SelectStack>(), dropdownListValueUnique);
            #endregion

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                CurDisplay = display,
                CurValues = new[]{curValue},
                DropdownListValue = dropdownListValueUnique,
                SelectStacks = curSelected,
            };
        }

        private static (string error, IAdvancedDropdownList dropdownList) GetUniqueList(IAdvancedDropdownList dropdownListValue, EUnique eUnique, object curValue, SerializedProperty property, FieldInfo info, object parent)
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

            // if (eUnique == EUnique.Remove)
            // {
            //     existsValues.Remove(curValue);
            // }

            return ("", ReWrapUniqueList(dropdownListValue, eUnique, existsValues, curValue));
        }

        private static AdvancedDropdownList<object> ReWrapUniqueList(IAdvancedDropdownList dropdownListValue, EUnique eUnique, List<object> existsValues, object curValue)
        {
            AdvancedDropdownList<object> dropdownList = new AdvancedDropdownList<object>(dropdownListValue.displayName, dropdownListValue.disabled, dropdownListValue.icon);
            IReadOnlyList<AdvancedDropdownList<object>> children = ReWrapUniqueChildren(dropdownListValue.children, eUnique, existsValues, curValue);
            dropdownList.SetChildren(children.ToList());
            return dropdownList;
        }

        private static IReadOnlyList<AdvancedDropdownList<object>> ReWrapUniqueChildren(IReadOnlyList<IAdvancedDropdownList> children, EUnique eUnique, IReadOnlyList<object> existsValues, object curValue)
        {
            List<AdvancedDropdownList<object>> newChildren = new List<AdvancedDropdownList<object>>();
            foreach (IAdvancedDropdownList originChild in children)
            {
                if (originChild.isSeparator)
                {
                    newChildren.Add(AdvancedDropdownList<object>.Separator());
                }
                else if (originChild.ChildCount() > 0)  // has sub child
                {
                    IReadOnlyList<AdvancedDropdownList<object>> subChildren = ReWrapUniqueChildren(originChild.children, eUnique, existsValues, curValue);
                    if (subChildren.Any(each => !each.isSeparator))
                    {
                        bool isDisabled = originChild.disabled ||
                                          subChildren.All(each => each.isSeparator || each.disabled);
                        AdvancedDropdownList<object> newChild = new AdvancedDropdownList<object>(originChild.displayName, isDisabled, originChild.icon);
                        newChild.SetChildren(subChildren.ToList());
                        newChildren.Add(newChild);
                    }
                }
                else
                {
                    object childValue = originChild.value;
                    bool exists = existsValues.Any(each => Util.GetIsEqual(each, childValue));
                    if (!exists)
                    {
                        newChildren.Add(new AdvancedDropdownList<object>(
                            originChild.displayName,
                            originChild.value,
                            originChild.disabled,
                            originChild.icon,
                            originChild.isSeparator));
                    }
                    else if (eUnique == EUnique.Disable)
                    {
                        newChildren.Add(new AdvancedDropdownList<object>(
                            originChild.displayName,
                            originChild.value,
                            true,
                            originChild.icon,
                            originChild.isSeparator));
                    }
                    else if (eUnique == EUnique.Remove)
                    {
                        if (Util.GetIsEqual(originChild.value, curValue))
                        {
                            newChildren.Add(new AdvancedDropdownList<object>(
                                originChild.displayName,
                                originChild.value,
                                true,
                                originChild.icon,
                                originChild.isSeparator));
                        }
                    }
                }
            }

            if (newChildren.All(each => each.isSeparator))
            {
                newChildren.Clear();
            }

            return newChildren;
        }

        private static string GetMetaStackDisplay(AdvancedDropdownMetaInfo metaInfo)
        {
            return metaInfo.SelectStacks.Count == 0
                ? "-"
                : string.Join("/", metaInfo.SelectStacks.Skip(1).Select(each => each.Display).Append(metaInfo.CurDisplay));
        }


        private static IEnumerable<(string stackDisplay, string display, string icon, bool disabled, object value)> FlattenChild(string prefix, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList child in children)
            {
                if (child.Count > 0)
                {
                    // List<(string, object, List<object>, bool, string, bool)> grandChildren = child.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((string, string, string, bool, object) grandChild in FlattenChild(prefix, child.children))
                    {
                        yield return grandChild;
                    }
                }
                else
                {
                    yield return (Prefix(prefix, child.displayName), child.displayName, child.icon, child.disabled, child.value);
                }
            }
        }

        public static IEnumerable<(string stackDisplay, string display, string icon, bool disabled, object value)> Flatten(string prefix, IAdvancedDropdownList roots)
        {
            foreach (IAdvancedDropdownList root in roots)
            {
                if (root.Count > 0)
                {
                    // IAdvancedDropdownList children = root.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((string, string, string, bool, object) child in FlattenChild(Prefix(prefix, root.displayName), root.children))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return (Prefix(prefix, root.displayName), root.displayName, root.icon, root.disabled, root.value);
                }
            }
        }

        private static string Prefix(string prefix, string value) => string.IsNullOrEmpty(prefix)? value : $"{prefix}/{value}";
        #endregion

    }
}
