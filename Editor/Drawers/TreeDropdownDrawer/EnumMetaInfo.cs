using System;
using System.Collections.Generic;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public readonly struct EnumMetaInfo
    {
        public readonly struct EnumValueInfo
        {
            public readonly bool HasValue;
            public readonly object Value;
            public readonly string Label;
            public readonly string OriginalLabel;

            public EnumValueInfo(object value, string label, string originalLabel)
            {
                HasValue = true;
                Value = value;
                Label = label;
                OriginalLabel = originalLabel;
            }

            public override string ToString()
            {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (!HasValue)
                {
                    return "";
                }
                return $"<{OriginalLabel}={Value}/>";
            }
        }

        public readonly IReadOnlyList<EnumValueInfo> EnumValues;
        public readonly EnumValueInfo NothingValue;
        public readonly EnumValueInfo EverythingValue;
        public readonly object EverythingBit;
        public readonly bool IsFlags;
        public readonly Type EnumType;
        public readonly Type UnderType;

        public EnumMetaInfo(IReadOnlyList<EnumValueInfo> enumValues, EnumValueInfo everythingValue, EnumValueInfo nothingValue, object everythingBit, bool isFlags, Type enumType)
        {
            EnumValues = enumValues;
            EverythingValue = everythingValue;
            NothingValue = nothingValue;
            EverythingBit = everythingBit;
            IsFlags = isFlags;
            EnumType = enumType;
            UnderType = enumType.GetEnumUnderlyingType();
        }
    }
}
