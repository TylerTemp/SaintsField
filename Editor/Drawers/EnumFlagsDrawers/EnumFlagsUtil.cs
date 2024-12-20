using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public static class EnumFlagsUtil
    {
        public struct EnumDisplayInfo
        {
            public string Name;
            public bool HasRichName;
            public string RichName;
        }

        public static EnumFlagsMetaInfo GetMetaInfo(FieldInfo info)
        {
            Type enumType = ReflectUtils.GetElementType(info.FieldType);

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
                BitValueToName = bitValueToName,
                AllCheckedInt = allCheckedInt,
            };
        }

        public static bool isOn(int curValue, int checkValue)
        {
            if (checkValue == 0)
            {
                return false;
            }
            return (curValue & checkValue) == checkValue;
        }

        public static int ToggleBit(int curValue, int bitValue)
        {
            if (isOn(curValue, bitValue))
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
