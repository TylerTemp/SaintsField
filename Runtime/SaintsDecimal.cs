using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct SaintsDecimal:
        IComparable,
        IComparable<decimal>,
        IComparable<SaintsDecimal>,
        IConvertible,
        IEquatable<decimal>,
        IEquatable<SaintsDecimal>,
        IFormattable,
        IDeserializationCallback,
        ISerializationCallbackReceiver
    {
        [FieldOffset(0)]
        public int flags;
        [FieldOffset(4)]
        public int hi;
        [FieldOffset(8)]
        public int lo;
        [FieldOffset(12)]
        public int mid;

        [FieldOffset(16)]
        [NonSerialized] public bool cached;
        [FieldOffset(20)]
        [NonSerialized] public decimal value;

        public SaintsDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            lo = bits[0];
            mid = bits[1];
            hi = bits[2];
            flags = bits[3];

            cached = true;
            this.value = value;
        }

        public SaintsDecimal(double value): this(new decimal(value)){}

        public SaintsDecimal(int value): this(new decimal(value)){}

        public SaintsDecimal(int lo, int mid, int hi, bool isNegative, byte scale): this(new decimal(lo, mid, hi, isNegative, scale)){}

        public SaintsDecimal(int[] bits): this(new decimal(bits)){}

        public SaintsDecimal(long value): this(new decimal(value)){}

        public SaintsDecimal(float value): this(new decimal(value)){}

        public SaintsDecimal(uint value): this(new decimal(value)){}

        public SaintsDecimal(ulong value): this(new decimal(value)){}

        public decimal GetValue()
        {
            return new decimal(lo, mid, hi,
                (flags & unchecked((int)0x80000000)) != 0,
                (byte)((flags >> 16) & 0x7F));
        }

        public decimal GetValueAllowCache()
        {
            return cached ? value : GetValue();
        }

        public int CompareTo(decimal other)
        {
            return GetValueAllowCache().CompareTo(other);
        }

        public int CompareTo(SaintsDecimal other)
        {
            return GetValueAllowCache().CompareTo(other.GetValueAllowCache());
        }

        public bool Equals(decimal other)
        {
            return GetValueAllowCache().Equals(other);
        }

        public bool Equals(SaintsDecimal other)
        {
            return GetValueAllowCache().Equals(other.GetValueAllowCache());
        }

        public override string ToString()
        {
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            return GetValueAllowCache().ToString();
        }

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback)GetValueAllowCache()).OnDeserialization(sender);
            value = GetValue();
            cached = true;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return GetValueAllowCache().ToString(format, formatProvider);
        }

        public int CompareTo(object obj)
        {
            return GetValueAllowCache().CompareTo(obj);
        }


        public static implicit operator decimal(SaintsDecimal d) => d.GetValue();
        public static implicit operator SaintsDecimal(decimal d) => new SaintsDecimal(d);

        public TypeCode GetTypeCode()
        {
            return GetValueAllowCache().GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToByte(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToSingle(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return GetValueAllowCache().ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)GetValueAllowCache()).ToUInt64(provider);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            value = GetValue();
            cached = true;
        }
    }
}
