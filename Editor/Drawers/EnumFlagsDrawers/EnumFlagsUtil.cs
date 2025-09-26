using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public static class EnumFlagsUtil
    {
        public static bool HasFlag(Enum value, Enum flag) => (Convert.ToInt32(value) & Convert.ToInt32(flag)) != 0;
        public static bool HasFlag(int value, int flag) => (value & flag) != 0;

        public struct EnumDisplayInfo
        {
            public string Name;
            public bool HasRichName;
            public string RichName;
        }

        private static string GetNameFromInt(IReadOnlyDictionary<int, EnumDisplayInfo> bitValueToName, int selectedInt, string fallback)
        {
            if(bitValueToName.TryGetValue(selectedInt, out EnumDisplayInfo info))
            {
                return info.HasRichName? info.RichName: info.Name;
            }

            return fallback;
        }

        public static AdvancedDropdownMetaInfo GetDropdownMetaInfo(int curMask, int fullMask, IReadOnlyDictionary<int, EnumDisplayInfo> bitValueToName)
        {
            AdvancedDropdownList<object> dropdownListValue = new AdvancedDropdownList<object>
            {
                {GetNameFromInt(bitValueToName, 0, "Nothing"), 0},
                {GetNameFromInt(bitValueToName, fullMask,"Everything"), fullMask},
            };
            dropdownListValue.AddSeparator();
            foreach (KeyValuePair<int, EnumDisplayInfo> kv in bitValueToName.Where(each => each.Key != 0 & each.Key != fullMask))
            {
                dropdownListValue.Add(kv.Value.HasRichName? kv.Value.RichName: kv.Value.Name, kv.Key);
            }

            #region Get Cur Value

            IReadOnlyList<object> curValues = bitValueToName.Keys
                .Where(kv => IsOn(curMask, kv))
                .Append(curMask)
                .Cast<object>()
                .ToArray();

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                // ReSharper disable once UseIndexFromEndExpression
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) = AdvancedDropdownUtil.GetSelected(curValues[curValues.Count - 1], Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
                curSelected = stacks;
            }

            // string curDisplay = "";

            #endregion

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                // CurDisplay = display,
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        private static readonly Regex EnumLabelRegex = new Regex(@"<label\s*/?>", RegexOptions.Compiled);

        public static EnumFlagsMetaInfo GetMetaInfo(SerializedProperty property, FieldInfo info)
        {
            Type enumType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;;
            bool hasFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;

            Dictionary<int, EnumDisplayInfo> allIntToName = Enum
                .GetValues(enumType)
                .Cast<object>()
                .ToDictionary(
                    each => (int) each,
                    each =>
                    {
                        string normalName = Enum.GetName(enumType, each);
                        (bool found, string richName) = ReflectUtils.GetRichLabelFromEnum(enumType, each);
                        if (found)
                        {
                            richName = EnumLabelRegex.Replace(richName, normalName ?? "");
                        }
                        return new EnumDisplayInfo
                        {
                            Name = normalName,
                            HasRichName = found,
                            RichName = found? richName: null,
                        };
                    }
                );

            int allCheckedInt = allIntToName.Keys.Aggregate(0, (acc, value) => acc | value);
            Dictionary<int, EnumDisplayInfo> bitValueToName = allIntToName
                // .Where(each => each.Key != 0 && each.Key != allCheckedInt)
                // .Where(each => each.Key != 0 && each.Key != allCheckedInt)
                .ToDictionary(each => each.Key, each => each.Value);
            return new EnumFlagsMetaInfo
            {
                HasFlags = hasFlags,
                BitValueToName = bitValueToName,
                AllCheckedInt = allCheckedInt,
            };
        }

        public static bool IsOn(int curValue, int checkValue)
        {
            if (checkValue == 0)
            {
                return false;
            }
            return (curValue & checkValue) == checkValue;
        }

        public static bool IsOn(ulong curValue, ulong checkValue)
        {
            if (checkValue == 0)
            {
                return false;
            }
            return (curValue & checkValue) == checkValue;
        }
        public static bool IsOn(long curValue, long checkValue)
        {
            if (checkValue == 0)
            {
                return false;
            }
            return (curValue & checkValue) == checkValue;
        }

        public static bool IsOnObject(object curValue, object checkValue, bool isULong)
        {
            if (isULong)
            {
                return IsOn((ulong)Convert.ChangeType(curValue, typeof(ulong)),
                    (ulong)Convert.ChangeType(checkValue, typeof(ulong)));
            }
            return IsOn((long)Convert.ChangeType(curValue, typeof(long)),
                (long)Convert.ChangeType(checkValue, typeof(long)));
        }

        public static int ToggleBit(int curValue, int bitValue)
        {
            if (IsOn(curValue, bitValue))
            {
                return SetOffBit(curValue, bitValue);
            }
            return curValue | bitValue;
        }
        public static long ToggleBit(long curValue, long bitValue)
        {
            if (IsOn(curValue, bitValue))
            {
                return SetOffBit(curValue, bitValue);
            }
            return curValue | bitValue;
        }
        public static ulong ToggleBit(ulong curValue, ulong bitValue)
        {
            if (IsOn(curValue, bitValue))
            {
                return SetOffBit(curValue, bitValue);
            }
            return curValue | bitValue;
        }

        public static object ToggleBitObject(object curValue, object bitValue, bool isULong)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (IsOnObject(curValue, bitValue, isULong))
            {
                return SetOffBitObject(curValue, bitValue, isULong);
            }
            return SetOnBitObject(curValue, bitValue, isULong);
        }

        public static int SetOffBit(int curValue, int bitValue)
        {
            int fullBits = curValue | bitValue;
            return fullBits ^ bitValue;
        }

        public static long SetOffBit(long curValue, long bitValue)
        {
            long fullBits = curValue | bitValue;
            return fullBits ^ bitValue;
        }

        public static ulong SetOffBit(ulong curValue, ulong bitValue)
        {
            ulong fullBits = curValue | bitValue;
            return fullBits ^ bitValue;
        }

        public static object SetOnBitObject(object curValue, object curItem, bool isULong)
        {
            if (isULong)
            {
                return (ulong) curValue | (ulong) curItem;
            }
            return (long) curValue | (long) curItem;
        }

        public static object SetOffBitObject(object curValue, object curItem, bool isULong)
        {
            if (isULong)
            {
                ulong fullBits = (ulong)curValue | (ulong)curItem;
                return fullBits ^ (ulong)curItem;
            }
            else
            {
                long fullBits = (long)curValue | (long)curItem;
                return fullBits ^ (long)curItem;
            }
        }

        public static EnumMetaInfo GetEnumMetaInfo(Type enumType)
        {
            bool isFlags = Attribute.IsDefined(enumType, typeof(FlagsAttribute));
            List<EnumMetaInfo.EnumValueInfo> enumNormalValues = new List<EnumMetaInfo.EnumValueInfo>();
            EnumMetaInfo.EnumValueInfo nothingValue = new EnumMetaInfo.EnumValueInfo();
            EnumMetaInfo.EnumValueInfo everythingValue = new EnumMetaInfo.EnumValueInfo();

            bool isULong = enumType.GetEnumUnderlyingType() == typeof(ulong);

            long longValue = 0;
            ulong uLongValue = 0;

            foreach ((object enumValue, string enumLabel, string enumRichLabel) in Util.GetEnumValues(enumType))
            {
                EnumMetaInfo.EnumValueInfo info = new EnumMetaInfo.EnumValueInfo(enumValue, enumRichLabel ?? enumLabel, enumLabel);
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

            // object everythingBit = Convert.ChangeType(isULong ? uLongValue : longValue, enumType.GetEnumUnderlyingType());
            object everythingBit;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (isULong)
            {
                everythingBit = Enum.ToObject(enumType, uLongValue);
            }
            else
            {
                everythingBit = Enum.ToObject(enumType, longValue);
            }

            int foundEverythingIndex = -1;
            for (int everythingIndex = 0; everythingIndex < enumNormalValues.Count; everythingIndex++)
            {
                EnumMetaInfo.EnumValueInfo enumNormalValue = enumNormalValues[everythingIndex];
                if (isFlags)
                {
                    // Debug.Log($"each={enumNormalValue.Value}/{(ulong)enumNormalValue.Value}; everythingBit={everythingBit}");
                    if (enumNormalValue.Value.Equals(everythingBit))
                    {
                        everythingValue = enumNormalValue;
                        foundEverythingIndex = everythingIndex;
                        break;
                    }
                }
            }

            if (foundEverythingIndex != -1)
            {
                enumNormalValues.RemoveAt(foundEverythingIndex);
            }

            return new EnumMetaInfo(enumNormalValues, everythingValue, nothingValue, everythingBit, isFlags, enumType);
        }
    }
}
