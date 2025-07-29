using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(AdvancedDropdownAttribute), true)]
    public partial class AdvancedDropdownAttributeDrawer: SaintsPropertyDrawer
    {
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

            public override string ToString()
            {
                return $"[{Index}]{Display}";
            }
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(SerializedProperty property, AdvancedDropdownAttribute advancedDropdownAttribute, FieldInfo field, object parentObj, bool isImGui)
        {
            string funcName = advancedDropdownAttribute.FuncName;

            string error;
            IAdvancedDropdownList dropdownListValue = null;
            if (advancedDropdownAttribute.BehaveMode == AdvancedDropdownAttribute.Mode.Options)
            {
                AdvancedDropdownList<object> optionsDropdown = new AdvancedDropdownList<object>(isImGui? "Pick an Option": "");
                foreach (object value in advancedDropdownAttribute.Options)
                {
                    optionsDropdown.Add(RuntimeUtil.IsNull(value)? "[Null]": value.ToString(), value);
                }

                error = "";
                dropdownListValue = optionsDropdown;
            }
            else if (advancedDropdownAttribute.BehaveMode == AdvancedDropdownAttribute.Mode.Tuples)
            {
                AdvancedDropdownList<object> tuplesDropdown = new AdvancedDropdownList<object>(isImGui? "Pick an Option": "");
                foreach ((string path, object value) in advancedDropdownAttribute.Tuples)
                {
                    tuplesDropdown.Add(path, value);
                }

                error = "";
                dropdownListValue = tuplesDropdown;
            }
            else if (funcName is null)
            {
                Type elementType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(field.FieldType): field.FieldType;
                if(elementType.IsEnum)
                {
                    Array enumValues = Enum.GetValues(elementType);
                    AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>(isImGui? "Pick an Enum": "");
                    foreach (object enumValue in enumValues)
                    {
                        enumDropdown.Add(ReflectUtils.GetRichLabelFromEnum(elementType, enumValue).value, enumValue);
                    }

                    error = "";
                    dropdownListValue = enumDropdown;
                }
                else
                {
                    AdvancedDropdownList<object> staticDropdown = new AdvancedDropdownList<object>(isImGui? $"Pick a {elementType.Name}": "");

                    Dictionary<object, List<string>> valueToNames = new Dictionary<object, List<string>>();

                    // Get static fields
                    FieldInfo[] staticFields = elementType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (FieldInfo eachField in staticFields)
                    {
                        object value = eachField.GetValue(null);
                        // ReSharper disable once InvertIf
                        if (value != null && elementType.IsAssignableFrom(value.GetType()))
                        {
                            if (!valueToNames.TryGetValue(value, out List<string> names))
                            {
                                valueToNames[value] = names = new List<string>();
                            }
                            names.Add(eachField.Name);
                        }
                    }

                    // Get static properties
                    PropertyInfo[] staticProperties = elementType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (PropertyInfo eachProp in staticProperties)
                    {
                        object value = eachProp.GetValue(null);
                        if (elementType.IsAssignableFrom(value.GetType()))
                        {
                            if (!valueToNames.TryGetValue(value, out List<string> names))
                            {
                                valueToNames[value] = names = new List<string>();
                            }
                            names.Add(eachProp.Name);
                        }
                    }

                    // ReSharper disable once UseDeconstruction
                    foreach (KeyValuePair<object, List<string>> kv in valueToNames)
                    {
                        object value = kv.Key;
                        List<string> names = kv.Value;
                        names.Sort();
                        string displayName;
                        if (isImGui)
                        {
                            displayName = names[0] + (names.Count <= 1? "": $" ({string.Join(",", names.Skip(1))})");
                        }
                        else
                        {
                            displayName = names[0] + (names.Count <= 1? "": $" <color=#808080ff>({string.Join(",", names.Skip(1))})</color>");
                        }

                        staticDropdown.Add(new AdvancedDropdownList<object>(displayName, value));
                    }

                    error = "";
                    dropdownListValue = staticDropdown;
                    // error = $"{property.displayName}({elementType}) is not a enum";
                }
            }
            else
            {
                (string getOfError, object obj) =
                    Util.GetOf<object>(funcName, null, property, field, parentObj);
                error = getOfError;
                if (obj is IAdvancedDropdownList getOfDropdownListValue)
                {
                    dropdownListValue = getOfDropdownListValue;
                }
                else if (obj is IEnumerable<object> ieObj)
                {
                    AdvancedDropdownList<object> list = new AdvancedDropdownList<object>(isImGui? "Pick an item": "");
                    foreach (object each in ieObj)
                    {
                        list.Add($"{each}", each);
                    }

                    dropdownListValue = list;
                }
                else
                {
                    error = $"{funcName} return value is not a AdvancedDropdownList";
                }
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


        private static IEnumerable<(IReadOnlyList<string> stackDisplays, string display, string icon, bool disabled, object value)> FlattenChild(IReadOnlyList<string> stackDisplays, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList child in children)
            {
                if (child.ChildCount() > 0)
                {
                    // List<(string, object, List<object>, bool, string, bool)> grandChildren = child.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((IReadOnlyList<string> stackDisplays, string, string, bool, object) grandChild in FlattenChild(Prefix(stackDisplays, child.displayName), child.children.Where(each => !each.isSeparator)))
                    {
                        yield return grandChild;
                    }
                }
                else
                {
                    yield return (Prefix(stackDisplays, child.displayName), child.displayName, child.icon, child.disabled, child.value);
                }
            }
        }

        public static IEnumerable<(IReadOnlyList<string> stackDisplays, string display, string icon, bool disabled, object value)> Flatten(IAdvancedDropdownList roots)
        {
            foreach (IAdvancedDropdownList root in roots)
            {
                if (root.ChildCount() > 0)
                {
                    // IAdvancedDropdownList children = root.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((IReadOnlyList<string> stackDisplays, string, string, bool, object) child in FlattenChild(new[]{root.displayName}, root.children.Where(each => !each.isSeparator)))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return (new []{root.displayName}, root.displayName, root.icon, root.disabled, root.value);
                }
            }
        }

        private static IReadOnlyList<string> Prefix(IReadOnlyList<string> stackDisplays, string value)
        {
            return stackDisplays == null ? new[] { value } : stackDisplays.Append(value).ToArray();
        }

    }
}
