#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer: ISaintsSerializedPropertyDrawer
    {
        private class DrawPayload
        {
            public DrawInfo DrawInfo;
            public object Value;
        }

        public static VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual, ISaintsAttribute _, string label, SerializedProperty property, object parent)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                return new HelpBox($"Failed to get type for {property.propertyPath}", HelpBoxMessageType.Error);
            }

            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
                {
                    EnumMetaInfo enumMetaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    DropdownButtonLongElement ele = new DropdownButtonLongElement(enumMetaInfo);
                    SerializedProperty subProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                    // ele.BindProperty(subProp);
                    ele.bindingPath = subProp.propertyPath;

                    DropdownFieldLong r = new DropdownFieldLong(label, ele);
                    r.AddToClassList(DropdownFieldLong.alignedFieldUssClassName);
                    r.AddToClassList(ClassAllowDisable);

                    UIToolkitUtils.AddContextualMenuManipulator(r, subProp, () => { });

                    // ele.Button.clicked += () => ClickDropdown(ele.Button, enumMetaInfo, Enum.ToObject(enumMetaInfo.EnumType, subProp.longValue), v =>
                    // {
                    //     subProp.longValue = (long)v;
                    //     subProp.serializedObject.ApplyModifiedProperties();
                    // });

                    return r;
                }
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                {
                    EnumMetaInfo enumMetaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    DropdownButtonULongElement ele = new DropdownButtonULongElement(enumMetaInfo);
                    SerializedProperty subProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SERIALIZED_DEBUG
                    Debug.Log($"bind {targetType} to {subProp.propertyPath}");
#endif
                    // ele.BindProperty(subProp);
                    ele.bindingPath = subProp.propertyPath;

                    DropdownFieldULong r = new DropdownFieldULong(label, ele);
                    r.AddToClassList(DropdownFieldULong.alignedFieldUssClassName);
                    r.AddToClassList(ClassAllowDisable);

                    UIToolkitUtils.AddContextualMenuManipulator(r, subProp, () => { });

                    // ele.Button.clicked += () => ClickDropdown(ele.Button, enumMetaInfo, Enum.ToObject(enumMetaInfo.EnumType, subProp.ulongValue), v =>
                    // {
                    //     ulong uv = (ulong)v;
                    //     subProp.ulongValue = uv;
                    //     subProp.serializedObject.ApplyModifiedProperties();
                    // });

                    return r;
                }
#endif
                case SaintsPropertyType.Undefined:
                default:
                    return null;
            }
        }

        public static VisualElement DrawEnumUIToolkit(VisualElement oldElement, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            Type fieldType = valueType ?? value.GetType();
            Debug.Assert(fieldType.IsEnum, fieldType);

            bool isFlags = Attribute.IsDefined(fieldType, typeof(FlagsAttribute));
            string fieldClass = isFlags? "saintsfield-editor-enum-flags-field" : "saintsfield-editor-enum-field";
            if (oldElement is UIToolkitUtils.DropdownButtonField dropdownButton && dropdownButton.ClassListContains(fieldClass))
            {
                DrawPayload drawPayload = (DrawPayload)oldElement.userData;
                drawPayload.Value = value;
                // Debug.Log($"update to {value}");
                DrawEnumUIToolkitUpdateButtonLabel(dropdownButton);
                return null;
            }

            List<DrawInfo.EnumValueInfo> enumNormalValues = new List<DrawInfo.EnumValueInfo>();
            DrawInfo.EnumValueInfo nothingValue = new DrawInfo.EnumValueInfo();
            DrawInfo.EnumValueInfo everythingValue = new DrawInfo.EnumValueInfo();

            bool isULong = fieldType.GetEnumUnderlyingType() == typeof(ulong);

            long longValue = 0;
            ulong uLongValue = 0;

            foreach ((object enumValue, string enumLabel, string enumRichLabel) in Util.GetEnumValues(fieldType))
            {
                DrawInfo.EnumValueInfo info = new DrawInfo.EnumValueInfo(enumValue, enumRichLabel ?? enumLabel);
                if (isFlags)
                {
                    if (isULong)
                    {
                        uLongValue |= (ulong)enumValue;
                        if ((ulong)enumValue == 0)
                        {
                            nothingValue = info;
                            continue;
                        }
                    }
                    else
                    {
                        long longEnumValue = Convert.ToInt64(enumValue);
                        longValue |= longEnumValue;
                        if (longEnumValue == 0)
                        {
                            nothingValue = info;
                            continue;
                        }
                    }
                }
                enumNormalValues.Add(info);
            }

            int foundEverythingIndex = -1;
            for (int everythingIndex = 0; everythingIndex < enumNormalValues.Count; everythingIndex++)
            {
                DrawInfo.EnumValueInfo enumNormalValue = enumNormalValues[everythingIndex];
                if (isFlags)
                {
                    if (isULong)
                    {
                        if ((ulong)enumNormalValue.Value == uLongValue)
                        {
                            everythingValue = enumNormalValue;
                            foundEverythingIndex = everythingIndex;
                            break;
                        }
                    }
                    else
                    {
                        long enumLongValue = Convert.ToInt64(enumNormalValue.Value);
                        // ReSharper disable once InvertIf
                        if (enumLongValue == longValue)
                        {
                            everythingValue = enumNormalValue;
                            foundEverythingIndex = everythingIndex;
                            break;
                        }
                    }
                }
            }

            if (foundEverythingIndex != -1)
            {
                enumNormalValues.RemoveAt(foundEverythingIndex);
            }

            DrawInfo newInfo = new DrawInfo(enumNormalValues, everythingValue, nothingValue, isULong? uLongValue: longValue, isFlags, isULong);
            DrawPayload refDrawPayload = new DrawPayload
            {
                DrawInfo = newInfo,
                Value = value,
            };

            UIToolkitUtils.DropdownButtonField newDropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(label);
            newDropdownButton.style.flexGrow = 1;
            newDropdownButton.userData = refDrawPayload;
            if (labelGrayColor)
            {
                newDropdownButton.labelElement.style.color = EColor.EditorSeparator.GetColor();
            }

            newDropdownButton.AddToClassList(ClassAllowDisable);
            newDropdownButton.AddToClassList(fieldClass);

            newDropdownButton.ButtonElement.clicked += () =>
            {
                #region MetaInfo

                AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>("");
                List<object> curValues = new List<object>();
                bool containsEverythingOrNothing = false;

                if(newInfo.IsFlags)
                {
                    if (newInfo.NothingValue.HasValue)
                    {
                        enumDropdown.Add(newInfo.NothingValue.Label, newInfo.NothingValue.Value);
                    }
                    else
                    {
                        enumDropdown.Add("Nothing", newInfo.IsULong? 0UL: 0L);
                    }

                    if (newInfo.IsULong)
                    {
                        containsEverythingOrNothing = (ulong)refDrawPayload.Value == 0;
                    }
                    else
                    {
                        containsEverythingOrNothing = Convert.ToInt64(refDrawPayload.Value) == 0;
                    }
                    if (containsEverythingOrNothing)
                    {
                        curValues.Add(newInfo.IsULong? 0UL: 0L);
                    }

                    if (newInfo.EverythingValue.HasValue)
                    {
                        enumDropdown.Add(newInfo.EverythingValue.Label, newInfo.IsULong? (ulong)newInfo.EverythingValue.Value: (long)newInfo.EverythingValue.Value);
                    }
                    else
                    {
                        enumDropdown.Add("Everything", newInfo.IsULong? (ulong)newInfo.EverythingBit: (long)newInfo.EverythingBit);
                    }

                    if (!containsEverythingOrNothing)
                    {
                        if (newInfo.IsULong)
                        {
                            containsEverythingOrNothing = ((ulong)refDrawPayload.Value & (ulong)newInfo.EverythingBit) == (ulong) newInfo.EverythingBit;
                        }
                        else
                        {
                            long refValue = Convert.ToInt64(refDrawPayload.Value);
                            long everythingBit = Convert.ToInt64(newInfo.EverythingBit);
                            containsEverythingOrNothing = EnumFlagsUtil.IsOn(refValue, everythingBit);
                        }
                        if (containsEverythingOrNothing)
                        {
                            curValues.Add(newInfo.EverythingBit);
                        }
                    }

                    enumDropdown.AddSeparator();
                }

                foreach (DrawInfo.EnumValueInfo enumInfo in newInfo.EnumValues)
                {
                    // Debug.Log($"Add {enumInfo.Label} {enumInfo.Value}");
                    enumDropdown.Add(enumInfo.Label, enumInfo.Value);
                    if (!containsEverythingOrNothing)
                    {
                        if (isFlags)
                        {
                            if (isULong)
                            {
                                if (((ulong)refDrawPayload.Value & (ulong)enumInfo.Value) != 0)
                                {
                                    curValues.Add(enumInfo.Value);
                                }
                            }
                            else
                            {
                                if (EnumFlagsUtil.IsOn(Convert.ToInt64(refDrawPayload.Value), Convert.ToInt64(enumInfo.Value)))
                                {
                                    curValues.Add(enumInfo.Value);
                                }
                            }
                        }
                        else
                        {
                            if (enumInfo.Value == refDrawPayload.Value)
                            {
                                curValues.Add(value);
                            }
                        }
                    }
                }

                #endregion

                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    DropdownListValue = enumDropdown,
                    CurValues = curValues,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                    Error = "",
                };

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(newDropdownButton.worldBound);

                SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    newDropdownButton.worldBound.width,
                    maxHeight,
                    false,
                    (curItem, on) =>
                    {
                        // Debug.Log($"curItem={curItem}");
                        beforeSet?.Invoke(refDrawPayload.Value);
                        if(setterOrNull != null)
                        {
                            if (newInfo.IsFlags)
                            {
                                if (newInfo.IsULong)
                                {
                                    ulong curValue = (ulong)curItem;
                                    ulong newValue;
                                    if (curValue == 0)
                                    {
                                        newValue = 0UL;
                                    }
                                    else if (curValue == (ulong)newInfo.EverythingBit)
                                    {
                                        newValue = (ulong)newInfo.EverythingBit;
                                    }
                                    else if (on)
                                    {
                                        if ((ulong)refDrawPayload.Value == (ulong)refDrawPayload.DrawInfo.EverythingBit)
                                        {
                                            newValue = curValue;
                                        }
                                        else
                                        {
                                            newValue = (ulong)refDrawPayload.Value | curValue;
                                        }
                                    }
                                    else
                                    {
                                        newValue = EnumFlagsUtil.SetOffBit((ulong)refDrawPayload.Value, curValue);
                                    }
                                    refDrawPayload.Value = newValue;
                                    // Debug.Log($"setterOrNull({newValue})");
                                    setterOrNull(newValue);
                                }
                                else
                                {
                                    long curItemLong = Convert.ToInt64(curItem);
                                    long everthingBit = Convert.ToInt64(newInfo.EverythingBit);
                                    long refValueLong = Convert.ToInt64(refDrawPayload.Value);
                                    long newValue;
                                    if (curItemLong == 0)
                                    {
                                        newValue = 0L;
                                    }
                                    else if (curItemLong == everthingBit)
                                    {
                                        newValue = everthingBit;
                                    }
                                    else if (on)
                                    {
                                        if (refValueLong == everthingBit)
                                        {
                                            newValue = curItemLong;
                                        }
                                        else
                                        {
                                            newValue = refValueLong | curItemLong;
                                        }
                                    }
                                    else
                                    {
                                        newValue = EnumFlagsUtil.SetOffBit(refValueLong, curItemLong);
                                    }
                                    refDrawPayload.Value = newValue;
                                    // Debug.Log($"setterOrNull({newValue})");
                                    setterOrNull(Convert.ChangeType(newValue, fieldType.GetEnumUnderlyingType()));
                                }
                            }
                            else
                            {
                                // Debug.Log($"setterOrNull({curItem})");
                                setterOrNull(Convert.ChangeType(curItem, fieldType.GetEnumUnderlyingType()));
                            }
                        }
                        return null;
                    }
                );
                UnityEditor.PopupWindow.Show(worldBound, sa);
            };

            DrawEnumUIToolkitUpdateButtonLabel(newDropdownButton);

            return newDropdownButton;
        }

        private static void DrawEnumUIToolkitUpdateButtonLabel(UIToolkitUtils.DropdownButtonField dropdownButton)
        {
            DrawPayload refDrawPayload = (DrawPayload)dropdownButton.userData;
            string label = DrawEnumGetButtonLabel(refDrawPayload);
            Label btnLabel = dropdownButton.ButtonLabelElement;
            UIToolkitUtils.SetLabel(btnLabel, RichTextDrawer.ParseRichXml(label, "", null, null, null), new RichTextDrawer());
        }

        private static string DrawEnumGetButtonLabel(DrawPayload refDrawPayload)
        {
            if(refDrawPayload.DrawInfo.IsFlags)
            {
                bool isNothing;
                if (refDrawPayload.DrawInfo.IsULong)
                {
                    isNothing = (ulong)refDrawPayload.Value == 0;
                }
                else
                {
                    isNothing = Convert.ToInt64(refDrawPayload.Value) == 0;
                }


                if (isNothing)
                {
                    if (refDrawPayload.DrawInfo.NothingValue.HasValue)
                    {
                        return refDrawPayload.DrawInfo.NothingValue.Label;
                    }

                    return "Nothing";
                }

                bool isEverything;
                if (refDrawPayload.DrawInfo.IsULong)
                {
                    isEverything = ((ulong)refDrawPayload.Value & (ulong)refDrawPayload.DrawInfo.EverythingBit) ==
                                   (ulong)refDrawPayload.DrawInfo.EverythingBit;
                }
                else
                {
                    long longValue = Convert.ToInt64(refDrawPayload.Value);
                    long everythingBit = Convert.ToInt64(refDrawPayload.DrawInfo.EverythingBit);
                    isEverything = (longValue & everythingBit) == everythingBit;
                }

                if (isEverything)
                {
                    if (refDrawPayload.DrawInfo.EverythingValue.HasValue)
                    {
                        return refDrawPayload.DrawInfo.EverythingValue.Label;
                    }

                    return "Everything";
                }
            }

            List<string> labels = new List<string>();
            foreach (DrawInfo.EnumValueInfo drawInfoEnumValue in refDrawPayload.DrawInfo.EnumValues)
            {
                if (refDrawPayload.DrawInfo.IsFlags)
                {
                    if (refDrawPayload.DrawInfo.IsULong)
                    {
                        if (((ulong)refDrawPayload.Value & (ulong)drawInfoEnumValue.Value) != 0)
                        {
                            labels.Add(drawInfoEnumValue.Label);
                        }
                    }
                    else
                    {
                        long longValue = Convert.ToInt64(refDrawPayload.Value);
                        long infoValue = Convert.ToInt64(drawInfoEnumValue.Value);
                        if ((longValue & infoValue) != 0)
                        {
                            labels.Add(drawInfoEnumValue.Label);
                        }
                    }
                }
                else
                {
                    if (drawInfoEnumValue.Value.Equals(refDrawPayload.Value))
                    {
                        labels.Add(drawInfoEnumValue.Label);
                        break;
                    }
                }
            }

            return labels.Count == 0
                ? "<color=red>?</color>"
                : string.Join(", ", labels);
        }
    }
}
#endif
