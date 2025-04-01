using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static int ToggleBit(int curValue, int bitValue)
        {
            if (IsOn(curValue, bitValue))
            {
                int fullBits = curValue | bitValue;
                return fullBits ^ bitValue;
            }

            // int bothOnBits = curValue & bitValue;
            // Debug.Log($"curValue={curValue}, bitValue={bitValue}, bothOnBits={bothOnBits}");
            // return bothOnBits ^ curValue;
            return curValue | bitValue;
        }
    }
}
