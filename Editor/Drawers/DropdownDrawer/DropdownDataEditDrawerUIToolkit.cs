#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    public partial class DropdownAttributeDrawer
    {
        private class DrawPayload
        {
            public DrawInfo DrawInfo;
            public object Value;
            public EnumFlagsMetaInfo? FlagsMeta;
            public Func<object, bool, IReadOnlyList<object>> SetFlagsValue;
            public Type EnumType;
        }

        public static VisualElement DrawEnumUIToolkit(VisualElement oldElement, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            Type fieldType = valueType ?? value.GetType();
            Debug.Assert(fieldType.IsEnum, fieldType);

            bool isFlags = Attribute.IsDefined(fieldType, typeof(FlagsAttribute));
            string fieldClass = isFlags? "saintsfield-editor-enum-flags-field" : "saintsfield-editor-enum-field";
            if (oldElement is UIToolkitUtils.DropdownButtonField dropdownButton && dropdownButton.ClassListContains(fieldClass)
                && oldElement.userData is DrawPayload oldPayload && oldPayload.EnumType == fieldType)
            {
                DrawPayload drawPayload = (DrawPayload)oldElement.userData;
                drawPayload.Value = drawPayload.FlagsMeta.HasValue
                    ? Enum.ToObject(fieldType, EnumFlagsUtil.NormalizeDropdownValue(drawPayload.FlagsMeta.Value, EnumFlagsUtil.GetEnumBits(value)))
                    : value;
                // Debug.Log($"update to {value}");
                DrawEnumUIToolkitUpdateButtonLabel(dropdownButton);
                return null;
            }

            List<DrawInfo.EnumValueInfo> enumNormalValues = new List<DrawInfo.EnumValueInfo>();
            DrawInfo.EnumValueInfo nothingValue = new DrawInfo.EnumValueInfo();
            DrawInfo.EnumValueInfo everythingValue = new DrawInfo.EnumValueInfo();

            bool isULong = fieldType.GetEnumUnderlyingType() == typeof(ulong);
            EnumFlagsMetaInfo flagsMeta = new EnumFlagsMetaInfo
            {
                EnumType = fieldType,
                BitValueToName = new Dictionary<long, EnumFlagsUtil.EnumDisplayInfo>(),
            };

            long longValue = isFlags ? EnumFlagsUtil.GetEverythingValue(fieldType) : 0;
            ulong uLongValue = unchecked((ulong)longValue);

            foreach ((object enumValue, string enumLabel, string enumRichLabel, bool obsolete) in Util.GetEnumValues(fieldType))
            {
                DrawInfo.EnumValueInfo info = new DrawInfo.EnumValueInfo(enumValue, enumRichLabel ?? enumLabel);
                if (isFlags)
                {
                    long enumBits = EnumFlagsUtil.GetEnumBits(enumValue);
                    flagsMeta.BitValueToName[enumBits] = new EnumFlagsUtil.EnumDisplayInfo
                    {
                        Name = enumLabel,
                        HasRichName = enumRichLabel != null,
                        RichName = enumRichLabel,
                    };
                    if (enumBits == 0)
                    {
                        nothingValue = info;
                        continue;
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
                Value = isFlags
                    ? Enum.ToObject(fieldType, EnumFlagsUtil.NormalizeDropdownValue(flagsMeta, EnumFlagsUtil.GetEnumBits(value)))
                    : value,
                FlagsMeta = isFlags ? flagsMeta : (EnumFlagsMetaInfo?)null,
                EnumType = fieldType,
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

            if (isFlags && setterOrNull != null)
            {
                refDrawPayload.SetFlagsValue = (curItem, on) =>
                {
                    beforeSet?.Invoke(refDrawPayload.Value);
                    long newMask = EnumFlagsUtil.GetDropdownValue(flagsMeta,
                        EnumFlagsUtil.GetEnumBits(refDrawPayload.Value), EnumFlagsUtil.GetEnumBits(curItem), on);
                    object newValue = Enum.ToObject(fieldType, newMask);
                    refDrawPayload.Value = newValue;
                    setterOrNull(newValue);
                    DrawEnumUIToolkitUpdateButtonLabel(newDropdownButton);
                    return EnumFlagsUtil.GetDropdownMetaInfo(newMask, longValue, flagsMeta.BitValueToName)
                        .CurValues.Select(item => (object)Enum.ToObject(fieldType, (long)item)).ToArray();
                };
            }

            newDropdownButton.ButtonElement.clicked += () =>
            {
                #region MetaInfo

                Dropdown<object> enumDropdown = new Dropdown<object>("");
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
                        enumDropdown.Add("Nothing", Enum.ToObject(fieldType, newInfo.IsULong? 0UL: 0L));
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
                        // curValues.Add(Convert.ChangeType(newInfo.IsULong ? 0UL : 0L, fieldType));
                        curValues.Add(Enum.ToObject(fieldType, newInfo.IsULong ? 0UL : 0L));
                    }

                    if (newInfo.EverythingValue.HasValue)
                    {
                        enumDropdown.Add(
                            newInfo.EverythingValue.Label,
                            newInfo.EverythingValue.Value);
                    }
                    else
                    {
                        enumDropdown.Add("Everything", Enum.ToObject(fieldType, newInfo.EverythingBit));
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
                            curValues.Add(Enum.ToObject(fieldType, newInfo.EverythingBit));
                        }
                    }

                    enumDropdown.AddSeparator();
                }

                foreach (DrawInfo.EnumValueInfo enumInfo in newInfo.EnumValues)
                {
                    // Debug.Log($"Add {enumInfo.Label} {enumInfo.Value}");
                    enumDropdown.Add(enumInfo.Label, enumInfo.Value);
                    if (!containsEverythingOrNothing || isFlags)
                    {
                        if (isFlags)
                        {
                            if (isULong)
                            {
                                if (((ulong)refDrawPayload.Value & (ulong)enumInfo.Value) == (ulong)enumInfo.Value)
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
                                curValues.Add(enumInfo.Value);
                            }
                        }
                    }
                }

                if (!isFlags)
                {
                    curValues.Add(refDrawPayload.Value);
                }

                #endregion

                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    DropdownListValue = enumDropdown,
                    CurValues = curValues,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                    Error = "",
                };

                // Debug.Log($"list={string.Join(",", enumDropdown.Select(each => each.value))}, cur={string.Join(",", curValues)}");

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(newDropdownButton.worldBound);

                SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    newDropdownButton.worldBound.width,
                    maxHeight,
                    isFlags,
                    (curItem, on) =>
                    {
                        // Debug.Log($"curItem={curItem}");
                        if (refDrawPayload.SetFlagsValue != null)
                        {
                            return refDrawPayload.SetFlagsValue(curItem, on);
                        }
                        beforeSet?.Invoke(refDrawPayload.Value);
                        setterOrNull?.Invoke(curItem);
                        return null;
                    }
                );
                UnityEditor.PopupWindow.Show(worldBound, sa);
            };

            if (setterOrNull == null)
            {
                newDropdownButton.SetEnabled(false);
            }

            DrawEnumUIToolkitUpdateButtonLabel(newDropdownButton);

            return newDropdownButton;
        }

        private static void DrawEnumUIToolkitUpdateButtonLabel(UIToolkitUtils.DropdownButtonField dropdownButton)
        {
            DrawPayload refDrawPayload = (DrawPayload)dropdownButton.userData;
            string label = DrawEnumGetButtonLabel(refDrawPayload);
            Label btnLabel = dropdownButton.ButtonLabelElement;
            UIToolkitUtils.SetLabel(btnLabel, RichTextDrawer.ParseRichXmlWithProvider(label, new RichTextDrawer.EmptyRichTextTagProvider()), new RichTextDrawer());
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
            if (refDrawPayload.DrawInfo.IsFlags)
            {
                if (refDrawPayload.DrawInfo.IsULong)
                {
                    Dictionary<ulong, DrawInfo.EnumValueInfo> onNumberToInfo =
                        new Dictionary<ulong, DrawInfo.EnumValueInfo>();
                    foreach (DrawInfo.EnumValueInfo drawInfoEnumValue in refDrawPayload.DrawInfo.EnumValues)
                    {
                        ulong payloadValue = (ulong)refDrawPayload.Value;
                        ulong infoValue = (ulong)drawInfoEnumValue.Value;
                        if ((payloadValue & infoValue) == infoValue)  // it's on
                        {
                            bool foundConflict = false;
                            foreach (ulong alreadyOn in onNumberToInfo.Keys.ToArray())
                            {
                                ulong onBits = alreadyOn & infoValue;
                                if (onBits == infoValue)  // im the sub bits, skip
                                {
                                    foundConflict = true;
                                    break;
                                }

                                if (onBits == alreadyOn)  // im the super bits, use me instead
                                {
                                    onNumberToInfo.Remove(alreadyOn);
                                    onNumberToInfo[infoValue] = drawInfoEnumValue;
                                    foundConflict = true;
                                    break;
                                }
                            }

                            if (!foundConflict)
                            {
                                onNumberToInfo[infoValue] = drawInfoEnumValue;
                            }
                        }
                    }

                    labels.AddRange(onNumberToInfo.Select(each => each.Value.Label));
                }
                else
                {
                    Dictionary<long, DrawInfo.EnumValueInfo> onNumberToInfo =
                        new Dictionary<long, DrawInfo.EnumValueInfo>();
                    foreach (DrawInfo.EnumValueInfo drawInfoEnumValue in refDrawPayload.DrawInfo.EnumValues)
                    {
                        long payloadValue = Convert.ToInt64(refDrawPayload.Value);
                        long infoValue = Convert.ToInt64(drawInfoEnumValue.Value);
                        if ((payloadValue & infoValue) == infoValue)  // it's on
                        {
                            bool foundConflict = false;
                            foreach (long alreadyOn in onNumberToInfo.Keys.ToArray())
                            {
                                long onBits = alreadyOn & infoValue;
                                if (onBits == infoValue)  // im the sub bits, skip
                                {
                                    foundConflict = true;
                                    break;
                                }

                                if (onBits == alreadyOn)  // im the super bits, use me instead
                                {
                                    onNumberToInfo.Remove(alreadyOn);
                                    onNumberToInfo[infoValue] = drawInfoEnumValue;
                                    foundConflict = true;
                                    break;
                                }
                            }

                            if (!foundConflict)
                            {
                                onNumberToInfo[infoValue] = drawInfoEnumValue;
                            }
                        }
                    }

                    labels.AddRange(onNumberToInfo.Select(each => each.Value.Label));
                }
            }
            else
            {
                foreach ((DrawInfo.EnumValueInfo drawInfoEnumValue, int index) in refDrawPayload.DrawInfo.EnumValues
                             .WithIndex())
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

        private readonly struct DrawInfo
        {
            public readonly struct EnumValueInfo
            {
                public readonly bool HasValue;
                public readonly object Value;
                public readonly string Label;

                public EnumValueInfo(object value, string label)
                {
                    HasValue = true;
                    Value = value;
                    Label = label;
                }
            }

            public readonly IReadOnlyList<EnumValueInfo> EnumValues;
            public readonly EnumValueInfo NothingValue;
            public readonly EnumValueInfo EverythingValue;
            public readonly object EverythingBit;
            public readonly bool IsFlags;
            public readonly bool IsULong;

            public DrawInfo(IReadOnlyList<EnumValueInfo> enumValues, EnumValueInfo everythingValue,
                EnumValueInfo nothingValue, object everythingBit, bool isFlags, bool isULong)
            {
                EnumValues = enumValues;
                EverythingValue = everythingValue;
                NothingValue = nothingValue;
                EverythingBit = everythingBit;
                IsFlags = isFlags;
                IsULong = isULong;
            }
        }
    }
}
#endif
