using System;

namespace SaintsField.Utils
{
    public enum SourceType
    {
        NotSupported,  // not a supported value type
        NoLimit,  // no limit
        Callback,  // callback string

        // Sbyte, Byte, Short, Ushort, Int, Uint, Long,  // long can save them
        Long,

        Ulong,  // ulong

        // Float, Double,  // double
        Double,

        Decimal,  // decimal
    }

    public readonly struct NumberLimitParam
    {
        public readonly SourceType SourceType;

        public readonly string Callback;

        public readonly long LongV;
        public readonly ulong UlongV;
        public readonly double DoubleV;
        public readonly decimal DecimalV;

        public NumberLimitParam(object value)
        {
            switch (value)
            {
                case null:
                    SourceType = SourceType.NoLimit;
                    Callback = null;
                    LongV = 0;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;

                case string callback:
                    SourceType = SourceType.Callback;
                    Callback = callback;

                    LongV = 0;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case sbyte sbyteV:
                    SourceType = SourceType.Long;
                    LongV = sbyteV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case byte byteV:
                    SourceType = SourceType.Long;
                    LongV = byteV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case short shortV:
                    SourceType = SourceType.Long;
                    LongV = shortV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case ushort ushortV:
                    SourceType = SourceType.Long;
                    LongV = ushortV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case int intV:
                    SourceType = SourceType.Long;
                    LongV = intV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case uint uintV:
                    SourceType = SourceType.Long;
                    LongV = uintV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case long longV:
                    SourceType = SourceType.Long;
                    LongV = longV;

                    Callback = null;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case ulong ulongV:
                    SourceType = SourceType.Ulong;
                    UlongV = ulongV;

                    Callback = null;
                    LongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
                case float floatV:
                    SourceType = SourceType.Double;
                    DoubleV = floatV;

                    Callback = null;
                    LongV = 0;
                    UlongV = 0;
                    DecimalV = 0;
                    break;
                case double doubleV:
                    SourceType = SourceType.Double;
                    DoubleV = doubleV;

                    Callback = null;
                    LongV = 0;
                    UlongV = 0;
                    DecimalV = 0;
                    break;
                case decimal decimalV:
                    SourceType = SourceType.Decimal;
                    DecimalV = decimalV;

                    Callback = null;
                    LongV = 0;
                    UlongV = 0;
                    DoubleV = 0;
                    break;

                default:
                    SourceType = SourceType.NotSupported;
                    Callback = null;
                    LongV = 0;
                    UlongV = 0;
                    DoubleV = 0;
                    DecimalV = 0;
                    break;
            }

        }

        // public NumberLimitParam(sbyte sbyteV)
        // {
        //     SourceType = SourceType.Sbyte;
        //     LongV = sbyteV;
        //
        //     Callback = null;
        //     UlongV = 0;
        //     DoubleV = 0;
        //     FloatV = 0;
        // }
    }
}
